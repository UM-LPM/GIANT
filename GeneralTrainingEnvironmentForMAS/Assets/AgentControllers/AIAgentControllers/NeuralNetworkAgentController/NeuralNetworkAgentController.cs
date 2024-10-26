using UnityEngine;

namespace AgentControllers.AIAgentControllers
{
    [CreateAssetMenu(menuName = "AgentControllers/AIAgentControllers/NeuralNetworkAgentController")]
    public class NeuralNetworkAgentController : AIAgentController
    {
        public NeuralNetworkAgentController()
            : base()
        {
        }

        public override void GetActions(in ActionBuffer actionsOut)
        {
            throw new System.NotImplementedException();
        }
    }
}