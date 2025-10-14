using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public abstract class CompositeNode : BehaviorNode
    {
        [HideInInspector] public List<BehaviorNode> Children = new List<BehaviorNode>();

        public override ABiSNode Clone()
        {
            CompositeNode node = Instantiate(this);
            node.Children = Children.ConvertAll(c => c.Clone() as BehaviorNode);
            return node;
        }

        public override void RemoveChild(ABiSNode child)
        {
            if (Children.Contains(child as BehaviorNode))
            {
                Children.Remove(child as BehaviorNode);
            }
            else
            {
                DebugSystem.LogWarning("Child not found in CompositeNode's children list.");
            }
        }
    }
}