using UnityEngine;

namespace AgentControllers.AIAgentControllers.ScriptAgentControllers
{
    [CreateAssetMenu(fileName = "DemoScriptAgentController", menuName = "AgentControllers/AIAgentControllers/ScriptAgentControllers/DemoScriptAgentController")]
    public class DemoScriptAgentController : ScriptAgentController
    {
        public DemoScriptAgentController()
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
    }
}