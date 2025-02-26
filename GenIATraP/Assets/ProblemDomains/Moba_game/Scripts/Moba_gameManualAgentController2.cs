using AgentControllers;
using UnityEngine;

namespace Problems.Moba_game
{
    [CreateAssetMenu(fileName = "Moba_gameManualAgentController2", menuName = "AgentControllers/AIAgentControllers/ManualAgentControllers/Moba_gameManualAgentController2")]
    public class Moba_gameManualAgentController2 : ManualAgentController
    {
        public override void GetActions(in ActionBuffer actionsOut)
        {
            if (Input.GetKey(KeyCode.UpArrow))
                actionsOut.AddDiscreteAction("moveForwardDirection", 1);

            if (Input.GetKey(KeyCode.RightArrow))
                actionsOut.AddDiscreteAction("rotateDirection", 2);
            else if (Input.GetKey(KeyCode.LeftArrow))
                actionsOut.AddDiscreteAction("rotateDirection", 1);

            if (Input.GetKey(KeyCode.Space))
                actionsOut.AddDiscreteAction("shootMissile", 1);
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