using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpotifyAPI.Web;

namespace Spotty.Pages
{
    public class RedirectModel : PageModel
    {
        /// <summary>
        ///     Redirects to authorization url
        /// </summary>
        /// <returns>A redirection to the authorization page</returns>
        public IActionResult OnGet()
        {
            LoginRequest loginRequest = new LoginRequest(
                new Uri(""),
                "",
                LoginRequest.ResponseType.Code
            )
            {
                Scope = new[] { Scopes.PlaylistReadPrivate, Scopes.UserLibraryRead, Scopes.UserReadPrivate, Scopes.PlaylistModifyPrivate, Scopes.PlaylistModifyPublic }
            };

            Uri uri = loginRequest.ToUri();
            string redirection_url = uri.ToString();

            return Redirect(redirection_url);
        }
    }
}