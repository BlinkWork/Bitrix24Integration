using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bitrix24Website.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string ClientId { get; set; }

        [BindProperty]
        public string ClientSecret { get; set; }

        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(ClientSecret))
            {
                ErrorMessage = "Client ID và Client Secret là bắt buộc.";
                return Page();
            }

            // Save to session
            HttpContext.Session.SetString("clientId", ClientId);
            HttpContext.Session.SetString("clientSecret", ClientSecret);

            return RedirectToPage("/Index");
        }
    }
}
