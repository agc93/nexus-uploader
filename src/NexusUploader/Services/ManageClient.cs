using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;

namespace NexusUploader.Nexus.Services
{
    public class ManageClient
    {
        private readonly HttpClient _httpClient;
        private readonly CookieService _cookies;

        public ManageClient(HttpClient httpClient, CookieService cookieService)
        {
            _httpClient = httpClient;
            _cookies = cookieService;
        }

        public async Task<bool> CheckValidSession() {
            var uri = "/Core/Libs/Common/Widgets/MyModerationHistoryTab";
            using (var req = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                var resp = await _httpClient.SendAsync(req);
                if (!resp.IsSuccessStatusCode) {
                    return false;
                }
                var str = await resp.Content.ReadAsStreamAsync();
                var reader = new StreamReader(str);
                string line;
                int lineOffset = 0;
                while ((line = reader.ReadLine()) != null && lineOffset < 10)
                {
                    if (line.Contains("og:") || line.Contains("Error"))
                    {
                        return false;
                    }
                    lineOffset++;
                }
                return true;
                // return resp.IsSuccessStatusCode && (resp.Content.Headers.ContentLength.HasValue && resp.Content.Headers.ContentLength < 100000) && (resp.Headers.Where(c => c.Key == "Set-Cookie").Count() < 2);
            }
        }

        public async Task<bool> AddChangelog(GameRef game, int modId, string version, string changeMessage)
        {
            changeMessage = HttpUtility.HtmlEncode(changeMessage).Replace(@"\n", "\n");
            var uri = "/Core/Libs/Common/Managers/Mods?SaveDocumentation";
            var message = new HttpRequestMessage(HttpMethod.Post, uri);
            message.Headers.Add("Referer", $"https://www.nexusmods.com/{game.Name}/mods/edit/?step=docs&id={modId}");
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(game.Id.ToString()), "game_id");
            content.Add(new StringContent(string.Empty), "new_version[]");
            content.Add(new StringContent(string.Empty), "new_change[]");
            foreach (var change in changeMessage.Split('\n'))
            {
                content.Add(new StringContent(version), "new_version[]");
                content.Add(new StringContent(change), "new_change[]");
            }
            content.Add(new StringContent(modId.ToString()), "id");
            message.Content = content;
            var resp = await _httpClient.SendAsync(message);
            if (resp.IsSuccessStatusCode) {
                var strResponse = await resp.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(strResponse);
                return data.ContainsKey("status") && data["status"].ToString() == true.ToString();
            }
            return false;
        }

        public async Task<HttpResponseMessage> AddFile(GameRef game, int modId, UploadedFile upload, FileOptions options)
        {
            var uri = "/Core/Libs/Common/Managers/Mods?AddFile";
            var message = new HttpRequestMessage(HttpMethod.Post, uri);
            message.Headers.Add("Referer", $"https://www.nexusmods.com/{game.Name}/mods/edit/?step=docs&id={modId}");
            using (var content = new MultipartFormDataContent())
            {
                content.Add(game.Id.ToContent(), "game_id");
                content.Add(options.Name.ToContent(), "name");
                content.Add(options.Version.ToContent(), "file-version");
                content.Add(options.UpdateMainVersion ? 1.ToContent() : 0.ToContent(), "update-version");
                content.Add(1.ToContent(), "category");
                if (options.PreviousFileId.HasValue) {
                    content.Add(1.ToContent(), "new-existing");
                    content.Add(options.PreviousFileId.Value.ToContent(), "old_file_id");
                }
                content.Add(options.Description.ToContent(), "brief-overview");
                content.Add(upload.Id.ToContent(), "file_uuid");
                content.Add(upload.FileSize.ToContent(), "file_size");
                content.Add(modId.ToContent(), "mod_id");
                content.Add(modId.ToContent(), "id");
                content.Add("add".ToContent(), "action");
                content.Add(upload.FileName.ToContent(), "uploaded_file");
                content.Add(upload.OriginalFile.ToContent(), "original_file");
                message.Content = content;
                var resp = await _httpClient.SendAsync(message);
                return resp;
            }
        }
    }
}