using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;

namespace NexusUploader.Nexus.Services
{
    public class UploadClient
    {
        private readonly HttpClient _httpClient;
        private readonly CookieService _cookies;

        private const string _zipContentType = "application/x-zip-compressed";

        public UploadClient(HttpClient httpClient, CookieService cookieService)
        {
            _httpClient = httpClient;
            _cookies = cookieService;
        }

        public async Task<bool> CheckStatus(UploadedFile upload)
        {
            var ready = false;
            var attempt = 1;
            do
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                var json = await _httpClient.GetStringAsync($"/uploads/check_status?id={upload.Id}");
                var resp = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                var assembled = bool.TryParse(resp["file_chunks_reassembled"]?.ToString(), out ready);
            } while (!ready);
            return ready;
        }

        /* public async Task<bool> CheckValidCookie(string cookie) {
            var ready = false;
            var json = await _httpClient.GetStringAsync($"/uploads/check_status?id={Guid.NewGuid().ToString().ToLower()}");
            var resp = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            // var assembled = bool.TryParse(resp["file_chunks_reassembled"]?.ToString(), out ready);
            return ready;
        } */

        public async Task<UploadedFile> UploadFile(GameRef game, int modId, FileInfo file)
        {
            int chunckSize = 5242880;
            int GetChunkSize(int i)
            {
                long position = (i * (long)chunckSize);
                int toRead = (int)Math.Min(file.Length - position + 1, chunckSize);
                return toRead;
            }
            string GetIdentifier()
            {
                return $"{file.Length}{file.Name.Replace(".", "")}";
            }

            int totalChunks = (int)(file.Length / chunckSize);
            if (file.Length % chunckSize != 0)
            {
                totalChunks++;
            }

            using (var str = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                for (int i = 0; i < totalChunks; i++)
                {
                    var toRead = GetChunkSize(i);
                    var path = "/uploads/chunk"
                    .SetQueryParams(new
                    {
                        resumableChunkNumber = totalChunks,
                        resumableChunkSize = chunckSize,
                        resumableCurrentChunkSize = GetChunkSize(i),
                        resumableTotalSize = file.Length,
                        resumableType = _zipContentType,
                        resumableIdentifier = GetIdentifier(),
                        resumableFilename = file.Name,
                        resumableRelativePath = file.Name,
                        resumableTotalChunks = totalChunks
                    });
                    var getResp = await _httpClient.GetAsync(path.ToString());
                    if (getResp.StatusCode != HttpStatusCode.NoContent)
                    {
                        throw new Exception("I don't even know what this means");
                    }
                    byte[] buffer = new byte[toRead];
                    await str.ReadAsync(buffer, 0, buffer.Length);
                    using (MultipartFormDataContent form = new MultipartFormDataContent())
                    {
                        form.Add(new StringContent((i + 1).ToString()), "resumableChunkNumber");
                        form.Add(chunckSize.ToContent(), "resumableChunkSize");
                        form.Add(toRead.ToContent(), "resumableCurrentChunkSize");
                        form.Add(file.Length.ToContent(), "resumableTotalSize");
                        form.Add(_zipContentType.ToContent(), "resumableType");
                        form.Add(GetIdentifier().ToContent(), "resumableIdentifier");
                        form.Add(file.Name.ToContent(), "resumableFilename");
                        form.Add(file.Name.ToContent(), "resumableRelativePath");
                        form.Add(totalChunks.ToContent(), "resumableTotalChunks");
                        form.Add(new ByteArrayContent(buffer), "file", "blob");
                        //  new StreamContent(str, toRead)
                        // form.Add(new ByteArrayContent(buffer), "file", "blob");
                        var response = await _httpClient.PostAsync("/uploads/chunk", form).ConfigureAwait(false);
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception("I don't know what this means either");
                        }
                        if (response.Content.Headers.Contains("Content-Type"))
                        {
                            var resp = System.Text.Json.JsonSerializer.Deserialize<UploadedFile>(await response.Content.ReadAsStringAsync());
                            if (!string.IsNullOrWhiteSpace(resp.Id))
                            {
                                resp.FileSize = (int)file.Length;
                                resp.OriginalFile = file.Name;
                                return resp;
                            }

                        }
                    }
                }
            }

            return null;

        }
    }
}