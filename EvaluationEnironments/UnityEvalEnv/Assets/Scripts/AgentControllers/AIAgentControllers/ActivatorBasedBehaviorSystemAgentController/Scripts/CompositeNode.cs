using System.Collections.Generic;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public abstract class CompositeNode : BehaviorNode
    {
        [HideInInspector] public List<ABiSNode> Children = new List<ABiSNode>();

        public override ABiSNode Clone()
        {
            CompositeNode node = Instantiate(this);
            node.Children = Children.ConvertAll(c => c.Clone());
            return node;
        }
    }
}