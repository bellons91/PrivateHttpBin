using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text.Json;

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
        public string Method { get; }
        public IQueryCollection QueryStringItems { get; }
        public string FullPath { get; }
        public Guid Id { get; }
        public ImmutableDictionary<string, StringValues> Headers { get; }
        public string Body { get; }
        public bool IsJson { get; }

        public RequestDetails(HttpRequest request)
        {
            RequestDate = DateTime.Now.ToLocalTime();
            Method = request.Method;
            QueryStringItems = request.Query;
            FullPath = request.GetEncodedPathAndQuery();
            Id = Guid.NewGuid();
            Headers = request.Headers.ToImmutableDictionary();
            var rawBody = ReadFromStreamAsync(request.Body, request.ContentType).GetAwaiter().GetResult();
            (Body, IsJson) = TryFormatAsJson(rawBody);
        }

        private static (string Body, bool IsJson) TryFormatAsJson(string rawBody)
        {
            if (string.IsNullOrEmpty(rawBody))
            {
                return (rawBody, false);
            }

            try
            {
                using var doc = JsonDocument.Parse(rawBody);
                var formatted = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
                return (formatted, true);
            }
            catch (JsonException)
            {
                return (rawBody, false);
            }
        }

        private async Task<string> ReadFromStreamAsync(Stream body, string contentType)
        {
            if (body == null || !body.CanRead)
            {
                return string.Empty;
            }

            var streamReader = new StreamReader(body);
            string stringContent = await streamReader.ReadToEndAsync();

            return stringContent;
        }
    }

}
