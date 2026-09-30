namespace PaperTrail.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using PaperTrail.Models.UserModels;
    using PaperTrail.Models.UserModels.PaperTrail.Models.UserModels;
    using PaperTrail.Services;

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(model);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { Message = "User registered successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.LoginAsync(model);

                if (!result.Succeeded)
                {
                    return Unauthorized(new { Message = result.ErrorMessage });
                }

                return Ok(new
                {
                    token = result.Token,
                    expiration = result.Expiration
                });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}