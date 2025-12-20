using SOLFranceBackend.Data;
using SOLFranceBackend.Models;
using SOLFranceBackend.Models.Dto;
using SOLFranceBackend.Service.IService;
using Microsoft.AspNetCore.Identity;
using Google.Apis.Auth;

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

        public AuthService(AppDbContext db, IJwtTokenGenerator jwtTokenGenerator,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<AuthService> logger, IConfiguration configuration)
        {
            _db = db;
            _jwtTokenGenerator = jwtTokenGenerator;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _configuration = configuration;

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

                //if user was found , Generate JWT Token
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtTokenGenerator.GenerateToken(user, roles);

                UserDto userDTO = new()
                {
                    Email = user.Email,
                    ID = user.Id,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber
                };

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
    }
}
