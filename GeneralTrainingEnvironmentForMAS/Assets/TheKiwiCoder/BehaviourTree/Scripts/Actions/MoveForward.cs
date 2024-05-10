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
    
    private Util Util;
    protected override void OnStart() {
        Util = this.context.gameObject.GetComponentInParent<Util>();
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        discreteActionsOut[0] = moveForwardDirection == MoveForwardDirection.Random ? this.Util.Rnd.Next(3) : (int)moveForwardDirection;

        return State.Success;
    }

    public static Node CreateNodeFromBehaviourTreeNodeDef(BehaviourTreeNodeDef behaviourTreeNodeDef, List<BehaviourTreeNodeDef> behaviourTreeNodeDefs, BehaviourTree tree) {
        // Create node
        MoveForward moveForwardNode = new MoveForward();

        // Set node properties
        moveForwardNode.moveForwardDirection = (MoveForwardDirection)int.Parse(behaviourTreeNodeDef.node_properties["moveForwardDirection"]);

        tree.nodes.Add(moveForwardNode);
        return moveForwardNode;
    }
}

