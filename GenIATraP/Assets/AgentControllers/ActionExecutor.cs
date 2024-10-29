
using UnityEngine;

namespace AgentControllers
{
    public abstract class ActionExecutor: MonoBehaviour
    {
        public abstract void ExecuteActions(ActionBuffer actionBuffer);
    }
}