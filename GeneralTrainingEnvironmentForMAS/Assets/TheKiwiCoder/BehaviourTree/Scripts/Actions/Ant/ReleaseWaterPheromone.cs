using TheKiwiCoder;
using UnityEngine;



public class ReleaseWaterPheromone : ActionNode
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
        discreteActionsOut[11] = 1;
        return State.Success;

    }
}
