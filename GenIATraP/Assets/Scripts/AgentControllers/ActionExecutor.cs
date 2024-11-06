
using UnityEngine;

namespace AgentControllers
{
    [DisallowMultipleComponent]
    public abstract class ActionExecutor: MonoBehaviour
    {
        public abstract void ExecuteActions(AgentComponent agent);
    }
}