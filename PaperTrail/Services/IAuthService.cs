using Microsoft.AspNetCore.Identity;
using PaperTrail.Models.UserModels.PaperTrail.Models.UserModels;

namespace PaperTrail.Services
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterAsync(AuthModel model);
        Task<LoginResult> LoginAsync(LoginModel model);
    }
}
