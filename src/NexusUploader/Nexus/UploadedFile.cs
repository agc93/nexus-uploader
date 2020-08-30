using System.Text.Json.Serialization;

namespace NexusUploader.Nexus
{
    public class UploadedFile {
        [JsonPropertyName("uuid")]
        public string Id {get;set;}
        [JsonIgnore]
        public int FileSize {get;set;}
        [JsonPropertyName("filename")]
        public string FileName {get;set;}
        [JsonIgnore]
        public string OriginalFile {get;set;}
    }
}