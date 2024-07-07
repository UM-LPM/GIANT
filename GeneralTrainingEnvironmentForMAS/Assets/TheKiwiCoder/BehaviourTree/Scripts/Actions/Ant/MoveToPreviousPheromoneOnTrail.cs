using TheKiwiCoder;
using UnityEngine;

public class MoveToPreviousPheromoneOnTrail : ActionNode
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
        discreteActionsOut[18] = 1;
        return State.Success;

    }
}
