using TheKiwiCoder;
using UnityEngine;

public class PickUpFood : ActionNode
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
    discreteActionsOut[3] = 1;
    return State.Success;
          
    }
}
