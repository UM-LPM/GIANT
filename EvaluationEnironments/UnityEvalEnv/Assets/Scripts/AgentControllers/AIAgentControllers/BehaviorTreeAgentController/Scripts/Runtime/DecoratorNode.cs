using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public abstract class DecoratorNode : BTNode {
        [HideInInspector] public BTNode child;

        public DecoratorNode(DecoratorNode other) : base(other)
        {
            if (other == null)
                return;

            if (other.child != null) {
                    this.child = other.child.Clone();
            }
        }
    }
}
