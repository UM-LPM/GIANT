using TheKiwiCoder;
using UnityEngine;



public class ReleaseThreatPheromone : ActionNode
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
        discreteActionsOut[10] = 1;
        return State.Success;

    }
}
