using System.Collections.Generic;
using TheKiwiCoder;

public class Shoot : ActionNode {

    public int shoot = 1;
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        discreteActionsOut[4] = shoot;

        return State.Success;
    }

    public static Node CreateNodeFromBehaviourTreeNodeDef(BehaviourTreeNodeDef behaviourTreeNodeDef, List<BehaviourTreeNodeDef> behaviourTreeNodeDefs, BehaviourTree tree) {
        // Create node
        Shoot shootNode = new Shoot();

        // Set node properties
        shootNode.shoot = int.Parse(behaviourTreeNodeDef.node_properties["shoot"]);

        tree.nodes.Add(shootNode);
        return shootNode;
    }
}
