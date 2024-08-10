using TheKiwiCoder;
using UnityEngine;

public class ReachedColonyBoundary : ConditionNode
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
        float boundaryTreshold= context.gameObject.GetComponentInParent<AntEnvironmentController1>().boundaryTreshold;
        float distance=Vector2.Distance(antAgent.transform.position,antAgent.hive.transform.position);
        return distance > boundaryTreshold;

    }
}
