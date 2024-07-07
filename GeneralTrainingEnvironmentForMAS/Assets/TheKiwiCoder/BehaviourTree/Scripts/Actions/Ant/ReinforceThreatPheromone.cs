using TheKiwiCoder;
using UnityEngine;

public class ReinforceThreatPheromone : ActionNode
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
        discreteActionsOut[17] = 1;
        return State.Success;

    }
}
