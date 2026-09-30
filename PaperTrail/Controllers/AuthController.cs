namespace PaperTrail.Controllers
{
    using PaperTrail.Models.UserModels;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { Message = "User registered successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id), // Added user ID claim
                    new Claim(ClaimTypes.Name, user.UserName ?? model.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                // Fetch secret key and ensure it's long enough (at least 256 bits / 32 bytes)
                var jwtKey = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "JWT Secret Key is improperly configured.");
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.UtcNow.AddHours(3), // Fixed to UTC
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }

            return Unauthorized(new { Message = "Invalid email or password." });
        }
    }

    public class AuthModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "The password must be at least 8 characters long.")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}