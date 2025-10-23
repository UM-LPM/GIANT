using AgentControllers;
using UnityEngine;

namespace Problems.BombClash
{
    [CreateAssetMenu(fileName = "BombermanManualAgentController", menuName = "AgentControllers/ManualAgentControllers/BombermanManualAgentController")]
    public class BombermanManualAgentController : ManualAgentController
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

            if (Input.GetKeyDown(KeyCode.Space))
                actionsOut.AddDiscreteAction("placeBomb", 1);
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