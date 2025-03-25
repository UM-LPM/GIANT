using AgentControllers;
using UnityEngine;

namespace Problems.Bomberman
{
    [CreateAssetMenu(fileName = "BombermanManualAgentController", menuName = "AgentControllers/AIAgentControllers/ManualAgentControllers/BombermanManualAgentController")]
    public class BombermanManualAgentController : ManualAgentController
    {
        public override void GetActions(in ActionBuffer actionsOut)
        {
            if (Input.GetKey(KeyCode.W))
                actionsOut.AddDiscreteAction("moveUpDirection", 1);
            else if (Input.GetKey(KeyCode.S))
                actionsOut.AddDiscreteAction("moveUpDirection", 2);

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