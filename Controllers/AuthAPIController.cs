using Microsoft.AspNetCore.Mvc;
using SOLFranceBackend.Models.Dto;
using SOLFranceBackend.Service.IService;

namespace SOLFranceBackend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService _authService;
        //private readonly IPublishEndpoint _publishEndpoint;
        protected ResponseDto _response;
        public AuthAPIController(IAuthService authService/*, IPublishEndpoint publishEndpoint*/)
        {
            _authService = authService;
            //_publishEndpoint = publishEndpoint;
            _response = new();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
        {

            var errorMessage = await _authService.Register(model);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _response.IsSuccess = false;
                _response.Message = errorMessage;
                return BadRequest(_response);
            }

            //var eventMessage = new NotificationEvent();
            //eventMessage.Message = "Registartion successful";
            //await _publishEndpoint.Publish(eventMessage);
            return Ok(_response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var loginResponse = await _authService.Login(model);
            if (loginResponse.User == null)
            {
                _response.IsSuccess = false;
                _response.Message = "Username or password is incorrect";
                return BadRequest(_response);
            }
            _response.Result = loginResponse;
            return Ok(_response);

        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] RegistrationRequestDto model)
        {
            var assignRoleSuccessful = await _authService.AssignRole(model.Email, model.Role.ToUpper());
            if (!assignRoleSuccessful)
            {
                _response.IsSuccess = false;
                _response.Message = "Error encountered";
                return BadRequest(_response);
            }
            return Ok(_response);
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
        {
            var token = await _authService.GoogleLogin(dto);

            if (string.IsNullOrEmpty(token))
                return BadRequest("Google login failed");

            return Ok(new { token });
        }
    }
}
