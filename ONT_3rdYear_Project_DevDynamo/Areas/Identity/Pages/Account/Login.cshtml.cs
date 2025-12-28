using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ONT_3rdyear_Project.Models;

namespace ONT_3rdyear_Project.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        TempData["UnconfirmedEmail"] = Input.Email;
                        return RedirectToPage("./EmailNotConfirmed");
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    // Get roles and redirect accordingly
                    var roles = await _userManager.GetRolesAsync(user);

                    /* if (roles.Contains("Nurse"))
                     {
                         return RedirectToAction("Index","Dashboard", new { area = "" });
                     }*/
                    if (await _userManager.IsInRoleAsync(user, "Nurse"))
                    {
                        return LocalRedirect("~/Nurse/Dashboard"); // Adjust path to your nurse dashboard
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Sister"))
                    {
                        return LocalRedirect("~/NurseSister/Dashboard"); // Adjust path to your nurse dashboard
                    }
                    else if (roles.Contains("Sister"))
                    {
                        return RedirectToAction("Index", "SisterDashboard", new { area = "" });
                    }
                    else if (await _userManager.IsInRoleAsync(user, "WardAdmin"))
                    {
                        return LocalRedirect("~/WardAdmin/Index"); // Adjust path to your ward admin dashboard
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return LocalRedirect("~/Admin/Dashboard"); // Adjust path to your admin dashboard
                    }else if (await _userManager.IsInRoleAsync(user, "ScriptManager"))
                    {
                        return LocalRedirect("~/Script/Dashboard"); // Adjust path to your admin dashboard
                    }else if (await _userManager.IsInRoleAsync(user, "ConsumableManager"))
                    {
                        return LocalRedirect("~/Consumable/Dashboard"); // Adjust path to your admin dashboard
                    }

                    // Default redirect
                    return LocalRedirect(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            return Page();
        }

    }
}
