namespace Fitnesses
{
    public class IndividualFitness
    {
        public int IndividualID { get; set; }
        public float Value { get; set; }
        public Dictionary<string, float>? IndividualValues { get; set; }
        public Dictionary<string, float>? AdditionalValues { get; set; }
        public Dictionary<string, Object>? IndividualPerformanceData { get; set; }
    }

    public class FinalIndividualFitness
    {
        public int IndividualID { get; set; }
        public float Value { get; set; }
        public List<IndividualMatchResult>? IndividualMatchResults { get; set; }
        public IndividualMatchResult? AvgMatchResult { get; set; }
        public Dictionary<string, float>? AdditionalValues { get; set; }
    }

    public class IndividualMatchResult
    {
        public int[]? OpponentsIDs { get; set; }
        public string? MatchName { get; set; }
        public float Value { get; set; }
        public Dictionary<string, float>? IndividualValues { get; set; }
        public Dictionary<string, int[]>? IndividualPerformanceData { get; set; }
    }
}
