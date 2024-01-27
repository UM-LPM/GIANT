using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheKiwiCoder;

public class PlaceBomb : ActionNode {
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        discreteActionsOut[2] = 1;

        return State.Success;
    }
}
