using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using global::AgentControllers.AIAgentControllers.BehaviorTreeAgentController;

public enum MoveForwardDirection {
    Forward = 1,
    Backward = 2,
    NoAction = 0,
    Random = 3
}

public class MoveForward : ActionNode {

    public MoveForwardDirection moveForwardDirection = MoveForwardDirection.Random;
    
    private Util Util;
    protected override void OnStart() {
        Util = context.gameObject.GetComponentInParent<Util>();
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;

        blackboard.actionsOut.AddDiscreteAction("moveForwardDirection", moveForwardDirection == MoveForwardDirection.Random ? Util.NextIntBt(3) : (int)moveForwardDirection);

        return State.Success;
    }
}

