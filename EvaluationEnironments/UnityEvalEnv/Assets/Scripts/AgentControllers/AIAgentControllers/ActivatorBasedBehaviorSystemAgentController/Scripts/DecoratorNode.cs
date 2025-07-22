using AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public abstract class DecoratorNode : ABiSNode
    {
        [HideInInspector] public ConnectionNode Child;

        public override ABiSNode Clone()
        {
            DecoratorNode node = Instantiate(this);
            node.Child = Child.Clone() as ConnectionNode;
            return node;
        }

        public override void RemoveChild(ABiSNode child = null)
        {
            child = null;
        }
    }
}