using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class NewActionNode : ActionNode
{
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        return State.Success;
    }
}
