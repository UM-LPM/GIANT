using Fitnesses;

namespace WebAPI.Models
{
    public class RequestBodyParams
    {
        public string? CoordinatorURI { get; set; }
        public string[]? EvalEnvInstanceURIs { get; set; }
        public string? ApiRequestDataSourceFilePath { get; set; }
        public string? DestinationFilePath { get; set; }
        public IndividualFitness[]? LastEvalIndividualFitnesses { get; set; }
    }
}
