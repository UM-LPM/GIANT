using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class DetectBoundaryPheromone : ConditionNode
{
    private AntAgentComponent agent;

    protected override void OnStart()
    {
        agent = context.gameObject.GetComponentInParent<AntAgentComponent>();
    }

    protected override bool CheckConditions()
    {
        return DetectPheromone(agent.transform.position, agent.detectionRadius);
    }

    protected override void OnStop()
    {
    }

    private bool DetectPheromone(Vector3 position, float detectionRadius)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, detectionRadius);
        foreach (Collider2D collider in hitColliders)
        {
            PheromoneNodeComponent pheromoneNode = collider.GetComponent<PheromoneNodeComponent>();
            if (pheromoneNode != null && pheromoneNode.type == PheromoneType.Boundary)
            {
                return true;
            }
        }

        return false;
    }
}
