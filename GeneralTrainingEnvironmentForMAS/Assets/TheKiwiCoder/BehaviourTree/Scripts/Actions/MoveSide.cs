using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using global::TheKiwiCoder;

public enum MoveSideDirection {
    Left = 1,
    Right = 2,
    NoAction = 0,
    Random = 3
}

public class MoveSide : ActionNode {

    public MoveSideDirection moveSideDirection = MoveSideDirection.Random;

    private Util util;
    protected override void OnStart() {
        util = this.context.gameObject.GetComponentInParent<Util>();
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        discreteActionsOut[1] = moveSideDirection == MoveSideDirection.Random ? this.util.rnd.Next(3) : (int)moveSideDirection;

        return State.Success;
    }
}

