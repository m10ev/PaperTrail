using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PaperTrail.Models.UserModels;
using PaperTrail.Models.UserModels.PaperTrail.Models.UserModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PaperTrail.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IdentityResult> RegisterAsync(AuthModel model)
        {
            var user = new User { UserName = model.Email, Email = model.Email };
            return await _userManager.CreateAsync(user, model.Password);
        }

        public async Task<LoginResult> LoginAsync(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return new LoginResult { Succeeded = false, ErrorMessage = "Invalid email or password." };
            }

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? model.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
            {
                throw new InvalidOperationException("JWT Secret Key is improperly configured.");
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.UtcNow.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new LoginResult
            {
                Succeeded = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            };
        }
    }

    public class LoginResult
    {
        public bool Succeeded { get; set; }
        public string? Token { get; set; }
        public DateTime? Expiration { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
