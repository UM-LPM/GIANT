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
        antAgent = context.gameObject.GetComponent<AntAgentComponent>();
        float boundaryTreshold= context.gameObject.GetComponent<AntEnvironmentController1>().boundaryTreshold;
        float distance=Vector2.Distance(antAgent.transform.position,antAgent.hive.transform.position);
        return distance > boundaryTreshold;

    }
}
