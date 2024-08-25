using TheKiwiCoder;
using UnityEngine;
using UnityEngine.UIElements;

public class IsInHive : ConditionNode
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
        Vector2 positionHive= antAgent.hive.gameObject.transform.position;
        Vector2 positionAgent=  antAgent.transform.position;
        return  Vector3.Distance(positionHive, positionAgent) < 2.0;

    }
}
