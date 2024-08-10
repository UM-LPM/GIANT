using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class IsBoundaryPheromoneWeak : ConditionNode
{
    private AntAgentComponent agent;

    protected override void OnStart()
    {
        agent = context.gameObject.GetComponentInParent<AntAgentComponent>();
    }

    protected override bool CheckConditions()
    {
        PheromoneTrailComponent trailComponent = agent.GetComponent<PheromoneTrailComponent>();
        if (trailComponent != null && trailComponent.pheromoneType == PheromoneType.Boundary)
        {
            return trailComponent.IsTrailWeak();
        }
        return false;
    }

    protected override void OnStop()
    {
    }
}
