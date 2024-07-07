using TheKiwiCoder;
using UnityEngine;

public class ReinforceFoodPheromone : ActionNode
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
        discreteActionsOut[15] = 1;
        return State.Success;

    }
}
