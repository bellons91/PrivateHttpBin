using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace PrivateHttpBin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews()
               .AddRazorRuntimeCompilation();


            builder.Services.AddMemoryCache();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();


            app.Map("/api/{path}", (IMemoryCache cache, HttpRequest request) =>
            {

                HttpRequestCollection collection = cache.Get<HttpRequestCollection>("requests");
                if (collection == null)
                {
                    collection = new HttpRequestCollection();
                }

                collection.Add(request);
                cache.Set("requests", collection);

                return DateTime.Now;
            });


            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }

    public class HttpRequestCollection
    {
        private List<RequestDetails> _requests;
        public HttpRequestCollection()
        {
            _requests = new List<RequestDetails>();
        }


        public void Add(HttpRequest request)
        {
            _requests.Add(new RequestDetails(request));
        }

        public IList<RequestDetails> GetRequests() => _requests.OrderByDescending(x => x.RequestDate).ToList();


    }
    public class RequestDetails
    {
        public DateTime RequestDate { get; }
        public string Method { get; set; }
        public IQueryCollection QueryStringItems { get; }
        public string FullPath { get; }
        public Guid Id { get; set; }

        public RequestDetails(HttpRequest request)
        {
            RequestDate = DateTime.UtcNow;
            Method = request.Method;
            QueryStringItems = request.Query;
            FullPath = request.GetEncodedPathAndQuery();
            Id = Guid.NewGuid();
        }
    }

}
