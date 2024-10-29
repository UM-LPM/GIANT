using UnityEngine;

namespace AgentControllers.AIAgentControllers
{
    [CreateAssetMenu(menuName = "AgentControllers/AIAgentControllers/FiniteStateMachineAgentController")]
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
    }
}