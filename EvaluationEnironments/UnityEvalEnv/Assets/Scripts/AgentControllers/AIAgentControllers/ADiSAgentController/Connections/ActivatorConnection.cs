using System;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    [Serializable]
    public class ActivatorConnection : ScriptableObject
    {
        public Activator Activator;
        public bool IsNegated;
    }
}
