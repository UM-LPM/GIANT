using TheKiwiCoder;
using UnityEngine;



public class DropWater : ActionNode
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
        discreteActionsOut[19] = 1;
        return State.Success;

    }
}
