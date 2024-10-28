using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using global::AITechniques.BehaviorTrees;

public enum MoveSideDirection {
    Left = 1,
    Right = 2,
    NoAction = 0,
    Random = 3
}

public class MoveSide : ActionNode {

    public MoveSideDirection moveSideDirection = MoveSideDirection.Random;

    private Util Util;
    protected override void OnStart() {
        Util = context.gameObject.GetComponentInParent<Util>();
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        blackboard.actionsOut.AddDiscreteAction("moveSideDirection", moveSideDirection == MoveSideDirection.Random ? Util.NextIntBt(3) : (int)moveSideDirection);

        return State.Success;
    }

    public static Node CreateNodeFromBehaviourTreeNodeDef(BehaviourTreeNodeDef behaviourTreeNodeDef, List<BehaviourTreeNodeDef> behaviourTreeNodeDefs, BehaviourTree tree) {
        // Create node
        MoveSide moveSideNode = new MoveSide();

        // Set node properties
        moveSideNode.moveSideDirection = (MoveSideDirection)int.Parse(behaviourTreeNodeDef.node_properties["moveSideDirection"]);

        tree.nodes.Add(moveSideNode);
        return moveSideNode;
    }
}

