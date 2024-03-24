using TheKiwiCoder;
using UnityEngine;

public class PickUpFood : ActionNode
{
    private AntEnvironmentController environmentController;

    protected override void OnStart()
    {

    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
          
                    var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
                    discreteActionsOut[5] = 1;
                    return State.Success;
          
    }
}
