using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using global::TheKiwiCoder;

public class RotateTurrent : ActionNode {

    public RotateDirection rotateDirection = RotateDirection.Random;

    private Util util;
    protected override void OnStart() {
        util = this.context.gameObject.GetComponentInParent<Util>();
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        discreteActionsOut[3] = rotateDirection == RotateDirection.Random? this.util.rnd.Next(3) : (int)rotateDirection;

        return State.Success;
    }
}

