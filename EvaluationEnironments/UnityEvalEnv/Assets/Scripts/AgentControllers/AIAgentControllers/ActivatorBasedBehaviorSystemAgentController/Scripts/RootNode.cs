using UnityEngine;
using System.Collections.Generic;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public class RootNode : ABiSNode
    {
        [HideInInspector] public List<ABiSNode> Children;
        protected override void OnStart()
        {
        }
        protected override void OnStop()
        {
            // Cleanup logic if needed
        }
        protected override State OnUpdate()
        {
            // TODO: Implement
            throw new System.NotImplementedException("RootNode OnUpdate not implemented");
        }
        public override ABiSNode Clone()
        {
            RootNode node = Instantiate(this);
            node.Children = new List<ABiSNode>();
            foreach (var child in Children)
            {
                node.Children.Add(child.Clone());
            }
            return node;
        }
    }
}