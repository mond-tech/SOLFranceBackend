using SOLFranceBackend.Models.Dto;

namespace SOLFranceBackend.Service.IService
{
    public interface IAuthService
    {
        Task<string> Register(RegistrationRequestDto registrationRequestDto);
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
        Task<bool> AssignRole(string email, string roleName);
        Task<string> GoogleLogin(GoogleLoginRequestDto googleLoginRequestDto);
        Task<string> ChangePassword(string userId, ChangePasswordRequestDto changePasswordRequestDto);
        Task<string> ConfirmEmail(string userId, string token);
    }
}
