using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;

public class IsPheromoneTrailOfType : ConditionNode
{
    public enum PheromoneType { Threat = 0, Boundary = 1 };
    public PheromoneType pheromoneType;
    public AntAgentComponent agent;
    
    protected override void OnStart()
    {
        agent = context.gameObject.GetComponentInParent<AntAgentComponent>();

    }

    protected override void OnStop()
    {
    }

    protected override bool CheckConditions()
    {
        if (!agent.activePheromoneTrail)
        {
            return false;
        }
        if (pheromoneType == PheromoneType.Threat)
        {
            if (agent.activePheromoneTrail.pheromoneType == global::PheromoneType.Threat)
            {
                return true;
            }

            return false;
        }
        else if (pheromoneType == PheromoneType.Boundary)
        {
            if (agent.activePheromoneTrail.pheromoneType == global::PheromoneType.Boundary)
            {
                return true;
            }


            return false;
        }
        return false;
    }
}
