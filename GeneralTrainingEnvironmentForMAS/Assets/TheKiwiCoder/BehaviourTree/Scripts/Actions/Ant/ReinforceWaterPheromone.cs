using TheKiwiCoder;
using UnityEngine;

public class ReinforceWaterPheromone : ActionNode
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
        discreteActionsOut[14] = 1;
        return State.Success;

    }
}
