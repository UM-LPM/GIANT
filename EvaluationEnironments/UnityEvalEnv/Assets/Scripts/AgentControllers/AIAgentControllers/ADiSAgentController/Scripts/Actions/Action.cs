using System;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public abstract class Action: ADiSComponent
    {
        public Action(Action other) : base(other)
        {
        }

        public abstract void Execute(ActionBuffer actionsOut);
    }
}