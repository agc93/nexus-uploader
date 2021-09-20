namespace NexusUploader
{
    public class FileOptions
    {
        public FileOptions(string name, string version, string category = "Main Files")
        {
            Name = name;
            Version = version;
        }
        public string Name {get;}
        public string Version {get;}
        public string Description {get;set;} = string.Empty;
        // public string Category {get;set;}
        public bool RemoveDownloadWithManager { get; set; }
        public bool UpdateMainVersion {get;set;} = true;
        public bool? SetAsMainVortex {get;set;}
        public int? PreviousFileId {get;set;}

        public override string ToString() {
            return $"{Name}/{Version}; UpdateVer:{UpdateMainVersion}; Previous:{(PreviousFileId.HasValue ? PreviousFileId.Value.ToString() : "unset")}; Description:{(Description.IsSet() ? "set" : "unset")}";
        }
    }
}