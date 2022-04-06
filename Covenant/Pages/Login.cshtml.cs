using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EasyPeasy.Core;
using EasyPeasy.Models;
using EasyPeasy.Models.EasyPeasy;

namespace EasyPeasy.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<EasyPeasyUser> _signInManager;
        private readonly UserManager<EasyPeasyUser> _userManager;

        public LoginModel(SignInManager<EasyPeasyUser> signInManager, UserManager<EasyPeasyUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public EasyPeasyUserRegister EasyPeasyUserRegister { get; set; }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                if (!_userManager.Users.ToList().Where(U => _userManager.IsInRoleAsync(U, "Administrator").WaitResult()).Any())
                {
                    if (EasyPeasyUserRegister.Password != EasyPeasyUserRegister.ConfirmPassword)
                    {
                        return BadRequest($"BadRequest - Password does not match ConfirmPassword.");
                    }

                    EasyPeasyUser user = new EasyPeasyUser { UserName = EasyPeasyUserRegister.UserName };
                    IdentityResult userResult = await _userManager.CreateAsync(user, EasyPeasyUserRegister.Password);
                    await _userManager.AddToRoleAsync(user, "User");
                    await _userManager.AddToRoleAsync(user, "Administrator");
                    await _signInManager.PasswordSignInAsync(EasyPeasyUserRegister.UserName, EasyPeasyUserRegister.Password, true, lockoutOnFailure: false);
                    // return RedirectToAction(nameof(Index));
                    return LocalRedirect("/home/index");
                }
                else
                {
                    var result = await _signInManager.PasswordSignInAsync(EasyPeasyUserRegister.UserName, EasyPeasyUserRegister.Password, true, lockoutOnFailure: false);
                    if (!result.Succeeded == true)
                    {
                        ModelState.AddModelError(string.Empty, "Incorrect username or password");
                        return Page();
                    }
                    // if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    // {
                    //     return LocalRedirect(returnUrl);
                    // }
                    // return RedirectToAction("Index", "Home");
                    return LocalRedirect("/home/index");
                }
            }
            catch (Exception e) when (e is ControllerNotFoundException || e is ControllerBadRequestException || e is ControllerUnauthorizedException)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return Page();
            }
        }
    }
}
