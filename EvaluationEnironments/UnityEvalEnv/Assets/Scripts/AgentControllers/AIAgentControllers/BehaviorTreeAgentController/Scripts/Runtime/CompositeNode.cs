using System.Collections.Generic;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public abstract class CompositeNode : BTNode {
        [HideInInspector] public List<BTNode> children = new List<BTNode>();

        public override BTNode Clone() {
            CompositeNode node = Instantiate(this);
            node.children = children.ConvertAll(c => c.Clone());
            return node;
        }
    }
}