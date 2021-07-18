using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using NexusUploader.Nexus.Services;

namespace NexusUploader.Nexus.Http
{
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
                foreach (var (name, value) in _cookies.GetCookies().Where(cookie =>
                    !string.IsNullOrWhiteSpace(cookie.Key) && !string.IsNullOrWhiteSpace(cookie.Value))) {
                    c.Add(new Cookie(name, value) {Domain = "nexusmods.com"});
                }
            } catch {
                _logger.LogError("Error encountered while loading cookies! [bold] This probably won't work![/]");
            }
            return c;
        }
    }
}