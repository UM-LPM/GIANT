using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;

public class PlaceBomb : ActionNode {

    public int placeBomb = 1;

    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        var discreteActionsOut = blackboard.actionsOut.DiscreteActions;
        blackboard.actionsOut.AddDiscreteAction("placeBomb", placeBomb);

        return State.Success;
    }

    public static Node CreateNodeFromBehaviourTreeNodeDef(BehaviourTreeNodeDef behaviourTreeNodeDef, List<BehaviourTreeNodeDef> behaviourTreeNodeDefs, BehaviorTreeAgentController tree) {
        // Create node
        PlaceBomb placeBombNode = new PlaceBomb();

        // Set node properties
        placeBombNode.placeBomb = int.Parse(behaviourTreeNodeDef.node_properties["placeBomb"]);

        tree.Nodes.Add(placeBombNode);
        return placeBombNode;
    }
}
