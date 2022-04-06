using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using EasyPeasy.Models.EasyPeasy;

namespace EasyPeasy.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<EasyPeasyUser> _signInManager;

        public LogoutModel(SignInManager<EasyPeasyUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await _signInManager.SignOutAsync();
            return LocalRedirect("/covenantuser/login");
        }
    }
}
