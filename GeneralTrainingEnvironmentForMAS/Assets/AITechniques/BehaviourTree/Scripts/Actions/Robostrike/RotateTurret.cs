using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using global::AITechniques.BehaviorTrees;

public class RotateTurret : ActionNode {

    public RotateDirection rotateDirection = RotateDirection.Random;

    private Util Util;
    protected override void OnStart() {
        Util = context.gameObject.GetComponentInParent<Util>();
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        discreteActionsOut[3] = rotateDirection == RotateDirection.Random? Util.NextIntBt(3) : (int)rotateDirection;

        return State.Success;
    }

    public static Node CreateNodeFromBehaviourTreeNodeDef(BehaviourTreeNodeDef behaviourTreeNodeDef, List<BehaviourTreeNodeDef> behaviourTreeNodeDefs, BehaviourTree tree) {
        // Create node
        RotateTurret rotateTurretNode = new RotateTurret();

        // Set node properties
        rotateTurretNode.rotateDirection = (RotateDirection)int.Parse(behaviourTreeNodeDef.node_properties["rotateDirection"]);

        tree.nodes.Add(rotateTurretNode);
        return rotateTurretNode;
    }
}

