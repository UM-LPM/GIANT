using UnityEngine;

namespace AgentControllers.AIAgentControllers
{
    [CreateAssetMenu(fileName = "FiniteStateMachineAgentController", menuName = "AgentControllers/AIAgentControllers/FiniteStateMachineAgentController")]
    public class FiniteStateMachineAgentController : AIAgentController
    {
        public FiniteStateMachineAgentController()
            : base()
        {
        }

        public override void GetActions(in ActionBuffer actionsOut)
        {
            throw new System.NotImplementedException();
        }

        public override AgentController Clone()
        {
            throw new System.NotImplementedException();
        }

        public override void AddAgentControllerToSO(ScriptableObject parent)
        {
            throw new System.NotImplementedException();
        }
    }
}