namespace NexusUploader
{
    public class GameRef
    {
        public GameRef()
        {
            
        }

        public GameRef(string name, int id)
        {
            Name = name;
            Id = id.ToString();
        }
        public string Name {get;set;}
        public string Id {get;set;}
    }
}