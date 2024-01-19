using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using global::TheKiwiCoder;

public enum MoveForwardDirection {
    Forward = 1,
    Backward = 2,
    NoAction = 0,
    Random = 3
}

public class MoveForward : ActionNode {

    public MoveForwardDirection moveForwardDirection = MoveForwardDirection.Random;
    
    private Util util;
    protected override void OnStart() {
        util = this.context.gameObject.GetComponentInParent<Util>();
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        discreteActionsOut[0] = moveForwardDirection == MoveForwardDirection.Random ? this.util.rnd.Next(3) : (int)moveForwardDirection;

        return State.Success;
    }
}

