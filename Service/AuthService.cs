using SOLFranceBackend.Data;
using SOLFranceBackend.Models;
using SOLFranceBackend.Models.Dto;
using SOLFranceBackend.Service.IService;
using Microsoft.AspNetCore.Identity;
using Google.Apis.Auth;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using SOLFranceBackend.Interfaces;
using SOLFranceBackend.Interfaces.Implementation;

namespace SOLFranceBackend.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailQueue _emailQueue;

        public AuthService(AppDbContext db, IJwtTokenGenerator jwtTokenGenerator,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<AuthService> logger, IConfiguration configuration, IEmailQueue emailQueue)
        {
            _db = db;
            _jwtTokenGenerator = jwtTokenGenerator;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _configuration = configuration;
            _emailQueue = emailQueue;
        }

        public async Task<bool> AssignRole(string email, string roleName)
        {
            try
            {
                _logger.LogInformation("Assigning role starts");
                var user = _db.ApplicationUsers.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
                if (user != null)
                {
                    if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                    {
                        //create role if it does not exist
                        _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                    }
                    await _userManager.AddToRoleAsync(user, roleName);
                    return true;
                }
                _logger.LogInformation("Assigning role starts");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception in assigining role. " + ex.Message);
                return false;
            }
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            try
            {
                _logger.LogInformation("Login starts");
                var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDto.UserName.ToLower());

                bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

                if (user == null || isValid == false)
                {
                    return new LoginResponseDto() { User = null, Token = "" };
                }

                UserDto userDTO = new()
                {
                    Email = user.Email,
                    ID = user.Id,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber
                };

                if (!user.EmailConfirmed)
                {

                    return new LoginResponseDto() { User = userDTO, Token = "" };
                }

                //if user was found , Generate JWT Token
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtTokenGenerator.GenerateToken(user, roles);
                

                LoginResponseDto loginResponseDto = new LoginResponseDto()
                {
                    User = userDTO,
                    Token = token
                };

                _logger.LogInformation("Login ends");
                return loginResponseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception in sign in. " + ex.Message);
                return new LoginResponseDto() { User = null, Token = "" };
            }
        }

        public async Task<string> Register(RegistrationRequestDto registrationRequestDto)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Registeration of user starts");
                ApplicationUser user = new()
                {
                    UserName = registrationRequestDto.Email,
                    Email = registrationRequestDto.Email,
                    NormalizedEmail = registrationRequestDto.Email.ToUpper(),
                    Name = registrationRequestDto.Name,
                    PhoneNumber = registrationRequestDto.PhoneNumber
                };


                var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
                if (result.Succeeded)
                {
                    var userToReturn = _db.ApplicationUsers.First(u => u.UserName == registrationRequestDto.Email);

                    UserDto userDto = new()
                    {
                        Email = userToReturn.Email,
                        ID = userToReturn.Id,
                        Name = userToReturn.Name,
                        PhoneNumber = userToReturn.PhoneNumber
                    };

                    _logger.LogInformation("Registeration of user ends and registeration is successful");

                    // 🔐 Generate email confirmation token
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var encodedToken = WebEncoders.Base64UrlEncode(
                        Encoding.UTF8.GetBytes(token));

                    var confirmUrl =
                        $"{_configuration["FrontendUrl"]}/confirm-email?userId={user.Id}&token={encodedToken}";

                    _emailQueue.QueueEmail(
                        user.Email,
                        "Confirm your email",
                        $"<p>Please confirm your account by clicking <a href='{confirmUrl}'>here</a></p>"
                    );
                    _logger.LogInformation("Registeration Email Sent");
                    await transaction.CommitAsync();
                    return "";

                }
                else
                {
                    _logger.LogInformation("Registeration of user ends and registeration is unsuccessful");
                    return result.Errors.FirstOrDefault().Description;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("Exception in user registeration. " + ex.Message);
                return ex.Message;
            }
        }

        public async Task<string> GoogleLogin(GoogleLoginRequestDto dto)
        {
            try
            {
                _logger.LogInformation("Google login started");

                // 1️⃣ Validate Google ID token
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);

                // 2️⃣ Extract user info from Google
                var email = payload.Email;
                var name = payload.Name;
                var googleUserId = payload.Subject;

                // 3️⃣ Check if user already exists
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    // 4️⃣ Create new Identity user
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        NormalizedEmail = email.ToUpper(),
                        Name = name,
                        EmailConfirmed = true // Google already verified email
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        _logger.LogError("Google user creation failed");
                        return result.Errors.FirstOrDefault()?.Description ?? "User creation failed";
                    }

                    // 5️⃣ Link Google login provider
                    var loginInfo = new UserLoginInfo(
                        loginProvider: "Google",
                        providerKey: googleUserId,
                        displayName: "Google");

                    await _userManager.AddLoginAsync(user, loginInfo);
                }

                // 6️⃣ Generate JWT for your app
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtTokenGenerator.GenerateToken(user, roles);

                _logger.LogInformation("Google login successful");
                return token;
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogError("Invalid Google token: " + ex.Message);
                return "Invalid Google token";
            }
            catch (Exception ex)
            {
                _logger.LogError("Google login exception: " + ex.Message);
                return ex.Message;
            }
        }

        public async Task<string> ChangePassword(string userId, ChangePasswordRequestDto changePasswordRequestDto)
        {
            try
            {
                _logger.LogInformation("Change password starts for user: {UserId}", userId);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return "User not found";
                }

                // Verify current password
                var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, changePasswordRequestDto.CurrentPassword);
                if (!isCurrentPasswordValid)
                {
                    _logger.LogWarning("Invalid current password for user: {UserId}", userId);
                    return "Current password is incorrect";
                }

                // Change password
                var result = await _userManager.ChangePasswordAsync(user, changePasswordRequestDto.CurrentPassword, changePasswordRequestDto.NewPassword);
                if (!result.Succeeded)
                {
                    var errorMessage = result.Errors.FirstOrDefault()?.Description ?? "Failed to change password";
                    _logger.LogError("Password change failed for user: {UserId}. Error: {Error}", userId, errorMessage);
                    return errorMessage;
                }

                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception in change password. " + ex.Message);
                return ex.Message;
            }
        }

        public async Task<string> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return "Invalid user";

            var decodedToken = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(token));

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
                return "Invalid or expired token";

            _logger.LogInformation("Email verified successfully.");
            return string.Empty;
        }
    }
}
