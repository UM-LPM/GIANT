using AgentControllers;
using UnityEngine;

namespace Problems.PlanetConquest
{
    [CreateAssetMenu(fileName = "PlanetConquestManualAgentController", menuName = "AgentControllers/AIAgentControllers/ManualAgentControllers/PlanetConquestManualAgentController")]
    public class PlanetConquestManualAgentController : ManualAgentController
    {
        public override void GetActions(in ActionBuffer actionsOut)
        {
            if (Input.GetKey(KeyCode.W))
                actionsOut.AddDiscreteAction("moveForwardDirection", 1);

            if (Input.GetKey(KeyCode.D))
                actionsOut.AddDiscreteAction("rotateDirection", 2);
            else if (Input.GetKey(KeyCode.A))
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