using TheKiwiCoder;
using UnityEngine;

public class MoveToNextPheromoneOnTrail : ActionNode
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
        discreteActionsOut[13] = 1;
        return State.Success;

    }
}
