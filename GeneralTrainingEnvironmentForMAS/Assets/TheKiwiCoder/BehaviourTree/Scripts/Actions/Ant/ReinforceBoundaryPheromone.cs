using TheKiwiCoder;
using UnityEngine;

public class ReinforceBoundaryPheromone : ActionNode
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
        discreteActionsOut[16] = 1;
        return State.Success;

    }
}
