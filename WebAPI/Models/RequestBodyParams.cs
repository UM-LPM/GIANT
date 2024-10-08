namespace WebAPI.Models
{
    public class RequestBodyParams
    {
        public string CoordinatorURI { get; set; }
        public string[] EvalEnvInstanceURIs { get; set; }
        public string SourceFilePath { get; set; }
        public string DestinationFilePath { get; set; }
    }
}
