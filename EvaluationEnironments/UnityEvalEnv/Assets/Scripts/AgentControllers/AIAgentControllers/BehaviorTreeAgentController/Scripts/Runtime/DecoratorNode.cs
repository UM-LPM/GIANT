using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public abstract class DecoratorNode : BTNode {
        [HideInInspector] public BTNode child;

        public override BTNode Clone() {
            DecoratorNode node = Instantiate(this);
            node.child = child.Clone();
            return node;
        }
    }
}
