using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NexusUploader.Nexus
{
    public class NexusFile
    {
        [JsonPropertyName("id")]
        public List<int> Ids {get;set;}

        [JsonPropertyName("file_id")]
        public int FileId {get;set;}

        [JsonPropertyName("category_name")]
        public string CategoryName {get;set;}

        [JsonPropertyName("version")]
        public string FileVersion {get;set;}
    }
}