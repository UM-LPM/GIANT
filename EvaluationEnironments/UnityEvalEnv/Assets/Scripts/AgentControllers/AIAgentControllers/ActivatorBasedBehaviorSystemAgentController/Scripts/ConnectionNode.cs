using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public class ConnectionNode : ABiSNode
    {
        [HideInInspector] public ABiSNode Child; // TODO: Should this be BehaviorNode?
        public float Weight;

        protected override void OnStart()
        {
            throw new NotImplementedException();
        }

        protected override void OnStop()
        {
            throw new NotImplementedException();
        }

        protected override State OnUpdate()
        {
            throw new NotImplementedException();
        }

        public override void RemoveChild(ABiSNode child = null)
        {
            child = null;
        }
    }
}
