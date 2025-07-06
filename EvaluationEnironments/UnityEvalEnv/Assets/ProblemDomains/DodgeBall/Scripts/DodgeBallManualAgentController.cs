using AgentControllers;
using UnityEngine;

namespace Problems.DodgeBall
{
    [CreateAssetMenu(fileName = "DodgeBallManualAgentController", menuName = "AgentControllers/AIAgentControllers/ManualAgentControllers/DodgeBallManualAgentController")]
    public class DodgeBallManualAgentController : ManualAgentController
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

            if (Input.GetKey(KeyCode.C))
                actionsOut.AddDiscreteAction("pickupBall", 1);

            if (Input.GetKey(KeyCode.Space))
                actionsOut.AddDiscreteAction("throwBall", 1);
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