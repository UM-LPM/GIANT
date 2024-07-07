using TheKiwiCoder;
using UnityEngine;

public class RestAndRecover : ActionNode
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
        discreteActionsOut[12] = 1;
        return State.Success;

    }
}
