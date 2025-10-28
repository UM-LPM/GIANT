using AgentControllers;
using UnityEngine;

namespace Problems.Pong
{
    [CreateAssetMenu(fileName = "PongManualAgentController", menuName = "AgentControllers/ManualAgentControllers/PongManualAgentController")]
    public class SoccerManualAgentController : ManualAgentController
    {
        public override void GetActions(in ActionBuffer actionsOut)
        {
            if (Input.GetKey(KeyCode.W))
                actionsOut.AddDiscreteAction("moveForwardDirection", 1);
            else if (Input.GetKey(KeyCode.S))
                actionsOut.AddDiscreteAction("moveForwardDirection", 2);
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