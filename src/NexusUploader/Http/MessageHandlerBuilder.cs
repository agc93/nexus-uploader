using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using NexusUploader.Nexus.Services;

namespace NexusUploader.Nexus.Http
{
    public class NexusMessageHandlerBuilder : HttpMessageHandlerBuilder
    {
        private readonly CookieService _cookies;
        private readonly ILogger<NexusMessageHandlerBuilder> _logger;

        public NexusMessageHandlerBuilder(CookieService cookieService, ILogger<NexusMessageHandlerBuilder> logger)
        {
            _cookies = cookieService;
            _logger = logger;
        }
        public override string Name { get; set; }
        public override HttpMessageHandler PrimaryHandler { get; set; }
        public override IList<DelegatingHandler> AdditionalHandlers => new List<DelegatingHandler>();
        // Our custom builder doesn't care about any of the above.
        public override HttpMessageHandler Build()
        {
            var container = GetCookies();
            return new HttpClientHandler
            {
                CookieContainer = container
                // Our custom settings
            };
        }

        private CookieContainer GetCookies()
        {
            var c = new CookieContainer();
            try {
            foreach (var cookie in _cookies.GetCookies())
            {
                if (!string.IsNullOrWhiteSpace(cookie.Key) && !string.IsNullOrWhiteSpace(cookie.Value)) {
                    c.Add(new Cookie(cookie.Key, cookie.Value) { Domain = "nexusmods.com"});
                }
            }
            } catch {
                _logger.LogError("Error encountered while loading cookies! [bold] This probably won't work![/]");
            }
            return c;
        }
    }

    public class NexusCookieHandler : HttpClientHandler
    {
        private readonly CookieService _cookies;
        private readonly ILogger<NexusCookieHandler> _logger;

        public NexusCookieHandler(CookieService cookieService, ILogger<NexusCookieHandler> logger)
        {
            _cookies = cookieService;
            _logger = logger;
            base.CookieContainer = GetCookies();
        }

        private CookieContainer GetCookies()
        {
            var c = new CookieContainer();
            try {
            foreach (var cookie in _cookies.GetCookies())
            {
                if (!string.IsNullOrWhiteSpace(cookie.Key) && !string.IsNullOrWhiteSpace(cookie.Value)) {
                    c.Add(new Cookie(cookie.Key, cookie.Value) { Domain = "nexusmods.com"});
                }
            }
            } catch {
                _logger.LogError("Error encountered while loading cookies! [bold] This probably won't work![/]");
            }
            return c;
        }
    }
}