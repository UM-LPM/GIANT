using System.Collections.Generic;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public abstract class CompositeNode : BTNode {
        [HideInInspector] public List<BTNode> children = new List<BTNode>();

        public CompositeNode(CompositeNode other) : base(other)
        {
            if (other == null)
                return;

            this.children = new List<BTNode>();
            foreach (BTNode child in other.children) {
                this.children.Add(child.Clone());
            }
        }
    }
}