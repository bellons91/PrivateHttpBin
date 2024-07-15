using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Immutable;

namespace PrivateHttpBin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IMemoryCache _cache;

        public IndexModel(ILogger<IndexModel> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public IReadOnlyList<RequestDetails> AllRequests { get; private set; }

        public void OnGet()
        {
            HttpRequestCollection collections = _cache.Get<HttpRequestCollection>("requests");


            this.AllRequests = collections?.GetRequests()?.AsReadOnly() ?? new List<RequestDetails>().AsReadOnly();


        }
    }
}
