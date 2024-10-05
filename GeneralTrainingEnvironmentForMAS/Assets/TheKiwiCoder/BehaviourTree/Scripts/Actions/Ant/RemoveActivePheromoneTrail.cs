using TheKiwiCoder;
using UnityEngine;



public class RemoveActivePheromoneTrail : ActionNode
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
        discreteActionsOut[21] = 1;
        return State.Success;

    }
}
