using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace ONT_3rdyear_Project.Areas.Identity.Pages.Account
{
    public class EmailNotConfirmedModel : PageModel
    {
        [TempData]
        public string UnconfirmedEmail { get; set; }

        public void OnGet()
        {
            // UnconfirmedEmail will be populated automatically from TempData if it exists.
        }
    }
}
