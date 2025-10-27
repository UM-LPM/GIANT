using AgentControllers;
using UnityEngine;

namespace Problems.MicroRTS
{
    [CreateAssetMenu(fileName = "MicroRTSManualAgentController", menuName = "AgentControllers/ManualAgentControllers/MicroRTSManualAgentController")]
    public class MicroRTSManualAgentController : ManualAgentController
    {
        public override void GetActions(in ActionBuffer actionsOut)
        {
            // TODO
        }

        public override AgentController Clone()
        {
            return this;
        }

        public override void AddAgentControllerToSO(ScriptableObject parent)
        {
            return;
        }
    }
}