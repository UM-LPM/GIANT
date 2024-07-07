using TheKiwiCoder;
using UnityEngine;



public class DropFood : ActionNode
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
        discreteActionsOut[6] = 1;
        return State.Success;

    }
}
