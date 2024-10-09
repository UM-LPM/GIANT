using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class DetectPheromoneOfType : ConditionNode
{
    public PheromoneType pheromoneType;
    private AntAgentComponent agent;

    protected override void OnStart()
    {
        agent = context.gameObject.GetComponentInParent<AntAgentComponent>();
    }

    protected override bool CheckConditions()
    {
        return DetectPheromone(agent.transform.position, agent.detectionRadius, pheromoneType);
    }

    protected override void OnStop()
    {
    }

    private bool DetectPheromone(Vector3 position, float detectionRadius, PheromoneType targetType)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, detectionRadius);

        foreach (Collider2D collider in hitColliders)
        {
            PheromoneNodeComponent pheromoneNode = collider.GetComponent<PheromoneNodeComponent>();
            if (pheromoneNode != null && pheromoneNode.type == targetType)
            {
                return true;
            }
        }

        return false;
    }
}
