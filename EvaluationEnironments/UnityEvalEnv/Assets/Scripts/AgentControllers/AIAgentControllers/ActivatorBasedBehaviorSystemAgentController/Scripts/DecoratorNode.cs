using AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public abstract class DecoratorNode : ABiSNode
    {
        [HideInInspector] public ABiSNode Child;

        public override ABiSNode Clone()
        {
            DecoratorNode node = Instantiate(this);
            node.Child = Child.Clone();
            return node;
        }
    }
}