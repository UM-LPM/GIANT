using TheKiwiCoder;
using UnityEngine;



public class Attack : ActionNode
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
        discreteActionsOut[7] = 1;
        return State.Success;

    }
}
