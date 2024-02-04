using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using global::TheKiwiCoder;

public class Shoot : ActionNode {

    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        discreteActionsOut[4] = 1;

        return State.Success;
    }
}

