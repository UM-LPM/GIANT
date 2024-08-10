using TheKiwiCoder;
using UnityEngine;

public class IsRested : ConditionNode
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
        return antAgent != null && antAgent.restThreshold <= antAgent.stamina;
    }
}
