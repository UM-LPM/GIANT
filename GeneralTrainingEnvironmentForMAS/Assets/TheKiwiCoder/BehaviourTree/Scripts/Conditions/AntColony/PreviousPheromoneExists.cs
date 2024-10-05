using TheKiwiCoder;
using UnityEngine;

public class previousPheromoneExists : ConditionNode
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
        antAgent = context.gameObject.GetComponentInParent<AntAgentComponent>();
        return  antAgent.currentActiveNodePheromone!=null && antAgent.currentActiveNodePheromone.previous !=null;
    }
}
