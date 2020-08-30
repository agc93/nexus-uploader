using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NexusUploader.Nexus.Services
{
    public class CookieService
    {
        private readonly ModConfiguration _config;

        public CookieService(ModConfiguration config)
        {
            _config = config;
        }
        /*
        * At a minimum it seems like the following keys are required:
        * - member_id
        * - pass_hash
        * - sfc
        * - sfct
        * - _app_session
        * - sid
        * - mqtids
        * - jwt_fingerprint
        * - session_id
        */

        public Dictionary<string, string> GetCookies() {
            
            if (Path.HasExtension(_config.Cookies) && File.Exists(_config.Cookies)) {
                var ckTxt = File.ReadAllLines(Path.GetFullPath(_config.Cookies));
                var ckSet = ParseCookiesTxt(ckTxt);
                return ckSet;
            } else {
                if (_config.Cookies.Contains('\n')) {
                    var ckSet = ParseCookiesTxt(_config.Cookies.Split('\n'));
                    return ckSet;
                } else {
                    return _config.Cookies.Split(';').Select(s => s.Trim(' ')).ToDictionary(s => s.Split('=').First(), s => s.Split('=').Last());
                }
            }
            
        }

        private Dictionary<string, string> ParseCookiesTxt(IEnumerable<string> ckTxt) {
            var ckSet = ckTxt
                    .Select(t => t.Split('\t', System.StringSplitOptions.RemoveEmptyEntries))
                    .Where(cv => cv.First().TrimStart('.') == "nexusmods.com")
                    .ToDictionary(k => k[5], v => v.Length > 6 ? v[6] : string.Empty);
            return ckSet;
        }
    }
}