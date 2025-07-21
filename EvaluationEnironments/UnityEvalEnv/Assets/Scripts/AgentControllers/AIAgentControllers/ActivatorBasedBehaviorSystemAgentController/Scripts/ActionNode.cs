using System.Collections.Generic;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public abstract class ActionNode : BehaviorNode
    {
        public override void RemoveChild(ABiSNode child = null)
        {
            // No action required for ActionNode
        }
    }
}