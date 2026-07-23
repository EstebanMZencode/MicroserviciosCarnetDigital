using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloAuth;

public class LogoutModel : PageModel
{
    public IActionResult OnPost()
    {
        HttpContext.Session.Clear();

        return RedirectToPage("/ModuloAuth/Login");
    }
}
