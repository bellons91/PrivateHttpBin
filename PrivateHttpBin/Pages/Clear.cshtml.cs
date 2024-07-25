using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace PrivateHttpBin.Pages
{
    public class ClearModel : PageModel
    {
        private readonly IMemoryCache _cache;

        public ClearModel(IMemoryCache cache)
        {
            _cache = cache;
        }
        public IActionResult OnGet()
        {
            _cache.Remove("requests");
            return RedirectToPage("Index");
        }
    }
}
