using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using global::TheKiwiCoder;

public enum RotateDirection {
    Left = 1,
    Right = 2,
    NoAction = 0,
    Random = 3
}

public class Rotate : ActionNode {

    public RotateDirection rotateDirection = RotateDirection.Random;

    private Util Util;
    protected override void OnStart() {
        Util = this.context.gameObject.GetComponentInParent<Util>();
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        discreteActionsOut[2] = rotateDirection == RotateDirection.Random? this.Util.Rnd.Next(3) : (int)rotateDirection;

        return State.Success;
    }
}

