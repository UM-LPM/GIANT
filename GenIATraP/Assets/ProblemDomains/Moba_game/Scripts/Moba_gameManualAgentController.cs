using AgentControllers;
using UnityEngine;

namespace Problems.Moba_game
{
    [CreateAssetMenu(fileName = "Moba_gameManualAgentController", menuName = "AgentControllers/AIAgentControllers/ManualAgentControllers/Moba_gameManualAgentController")]
    public class Moba_gameManualAgentController : ManualAgentController
    {
        public override void GetActions(in ActionBuffer actionsOut)
        {
            if (Input.GetKey(KeyCode.W)){
                actionsOut.AddDiscreteAction("moveForwardDirection", 1);
                Debug.Log("moba");}
            else if (Input.GetKey(KeyCode.S))
                actionsOut.AddDiscreteAction("moveForwardDirection", 2);

            if (Input.GetKey(KeyCode.D))
                actionsOut.AddDiscreteAction("rotateDirection", 2);
            else if (Input.GetKey(KeyCode.A))
                actionsOut.AddDiscreteAction("rotateDirection", 1);

            if (Input.GetKey(KeyCode.Q))
                actionsOut.AddDiscreteAction("rotateTurretDirection", 1);
            else if (Input.GetKey(KeyCode.E))
                actionsOut.AddDiscreteAction("rotateTurretDirection", 2);

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