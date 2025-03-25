using AgentControllers;
using UnityEngine;

namespace Problems.Soccer
{
    [CreateAssetMenu(fileName = "SoccerManualAgentController", menuName = "AgentControllers/AIAgentControllers/ManualAgentControllers/SoccerManualAgentController")]
    public class SoccerManualAgentController : ManualAgentController
    {
        public override void GetActions(in ActionBuffer actionsOut)
        {
            if (Input.GetKey(KeyCode.W))
                actionsOut.AddDiscreteAction("moveForwardDirection", 1);
            else if (Input.GetKey(KeyCode.S))
                actionsOut.AddDiscreteAction("moveForwardDirection", 2);

            if (Input.GetKey(KeyCode.D))
                actionsOut.AddDiscreteAction("moveSideDirection", 2);
            else if (Input.GetKey(KeyCode.A))
                actionsOut.AddDiscreteAction("moveSideDirection", 1);

            if (Input.GetKey(KeyCode.Q))
                actionsOut.AddDiscreteAction("rotateDirection", 1);
            else if (Input.GetKey(KeyCode.E))
                actionsOut.AddDiscreteAction("rotateDirection", 2);
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