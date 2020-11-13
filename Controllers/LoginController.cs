using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace blazor_auth_individual_experiment
{
    [Route("[controller]/[action]")]
    public class LoginController : Controller
    {
        SignInManager<IdentityUser> _signInMgr;

        public LoginController(SignInManager<IdentityUser> signInMgr)
        {
            _signInMgr = signInMgr;
        }

        public async Task<IActionResult> SignIn(string email, string password, bool rememberMe)
        {
            var result = await _signInMgr.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
            }
            if (result.RequiresTwoFactor)
            {
            }
            if (result.IsLockedOut)
            {
            }
            return LocalRedirect("/");
        }

        public async Task<IActionResult> SignOut()
        {
            await _signInMgr.SignOutAsync();
            return LocalRedirect("/");
        }
    }
}
