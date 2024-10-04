using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using global::AITechniques.BehaviorTrees;

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
        Util = context.gameObject.GetComponentInParent<Util>();
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        discreteActionsOut[2] = rotateDirection == RotateDirection.Random? Util.NextIntBt(3) : (int)rotateDirection;

        return State.Success;
    }

    public static Node CreateNodeFromBehaviourTreeNodeDef(BehaviourTreeNodeDef behaviourTreeNodeDef, List<BehaviourTreeNodeDef> behaviourTreeNodeDefs, BehaviourTree tree) {
        // Create node
        Rotate rotateNode = new Rotate();

        // Set node properties
        rotateNode.rotateDirection = (RotateDirection)int.Parse(behaviourTreeNodeDef.node_properties["rotateDirection"]);

        tree.nodes.Add(rotateNode);
        return rotateNode;
    }
}

