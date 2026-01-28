using AgentControllers;
using UnityEngine;

namespace Problems.Mario
{
    [CreateAssetMenu(fileName = "MarioManualAgentController", menuName = "AgentControllers/ManualAgentControllers/MarioManualAgentController")]
    public class MarioManualAgentController : ManualAgentController
    {
        public override void GetActions(in ActionBuffer actionsOut)
        {
            if (Input.GetKey(KeyCode.D))
                actionsOut.AddDiscreteAction("moveSideDirection", 2);
            else if (Input.GetKey(KeyCode.A))
                actionsOut.AddDiscreteAction("moveSideDirection", 1);

            if (Input.GetKey(KeyCode.W))
                actionsOut.AddDiscreteAction("smallJump", 1);

            if (Input.GetKey(KeyCode.E))
                actionsOut.AddDiscreteAction("bigJump", 1);
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