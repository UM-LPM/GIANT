using TheKiwiCoder;
using UnityEngine;



public class MaintainHive : ActionNode
{
    protected override void OnStart()
    {

    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {

        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        discreteActionsOut[5] = 1;
        return State.Success;

    }
}
