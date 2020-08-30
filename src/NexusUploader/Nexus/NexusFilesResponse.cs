using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NexusUploader.Nexus
{
    public class NexusFilesResponse
    {
        [JsonPropertyName("files")]
        public List<NexusFile> Files {get;set;} = new List<NexusFile>();

        [JsonPropertyName("file_updated")]
        public List<object> Updates {get;set;} = new List<object>();
    }
}