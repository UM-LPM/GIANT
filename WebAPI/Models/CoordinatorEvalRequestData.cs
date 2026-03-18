using Fitnesses;

public class CoordinatorEvalRequestData
{
    public string[]? EvalEnvInstances { get; set; }
    public EvalRange[]? EvalRanges { get; set; }
    public int[][]? EvalIndividuals { get; set; }
    public IndividualFitness[]? LastEvalIndividualFitnesses { get; set; } // TODO implement
}
