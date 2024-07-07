using TheKiwiCoder;
using UnityEngine;



public class ReleaseFoodPheromone : ActionNode
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
        discreteActionsOut[9] = 1; 
        return State.Success;

    }
}
