using TheKiwiCoder;
using UnityEngine;
using UnityEngine.UIElements;

public class HiveNeedsMaintanance : ConditionNode
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
        return antAgent.hive.needsMaintenance;

    }
}
