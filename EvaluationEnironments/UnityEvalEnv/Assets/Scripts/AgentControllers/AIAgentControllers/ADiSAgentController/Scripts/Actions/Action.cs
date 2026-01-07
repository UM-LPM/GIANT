using System;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public abstract class Action: ADiSComponent
    {
        public abstract void Execute(ActionBuffer actionsOut);
    }
}