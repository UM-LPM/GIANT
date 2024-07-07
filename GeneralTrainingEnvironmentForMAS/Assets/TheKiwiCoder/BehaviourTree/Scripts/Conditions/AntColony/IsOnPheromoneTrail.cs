using TheKiwiCoder;
using UnityEngine;

public class IsOnPheromoneTrail : ConditionNode
{
    public AntAgentComponent antAgent;

    protected override void OnStart()
    {

    }

    protected override void OnStop()
    {
    }

    protected override bool CheckConditions()
    {
        antAgent = context.gameObject.GetComponent<AntAgentComponent>();
        return antAgent != null && (antAgent.foodPheromoneTrailComponent || antAgent.threatPheromoneTrailComponent|| antAgent.waterPheromoneTrailComponent|| antAgent.boundaryPheromoneTrailComponent);
    }
}
