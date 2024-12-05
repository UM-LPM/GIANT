using Fitnesses;

public class CoordinatorEvalRequestData
{
    public string[]? EvalEnvInstances { get; set; }
    public int? EvalRangeStart { get; set; }
    public int? EvalRangeEnd { get; set; }
    public IndividualFitness[]? LastEvalIndividualFitnesses { get; set; }
}
