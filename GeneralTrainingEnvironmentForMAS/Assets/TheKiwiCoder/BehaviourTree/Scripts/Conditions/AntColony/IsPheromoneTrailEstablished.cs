using TheKiwiCoder;
using UnityEngine;

public class IsPheromoneTrailEstablished : ConditionNode
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
        bool activePheromoneTrailEstablished = false;
        if(antAgent.activePheromoneTrail != null)
            {
             float distanceToHive = Vector3.Distance(antAgent.activePheromoneTrail.lastNode.transform.position, antAgent.hive.transform.position);
            activePheromoneTrailEstablished = distanceToHive <= 2.0f;
            }
        
        return activePheromoneTrailEstablished;
    }
}
