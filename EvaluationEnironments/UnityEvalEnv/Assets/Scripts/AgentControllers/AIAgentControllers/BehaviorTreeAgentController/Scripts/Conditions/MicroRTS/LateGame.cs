using Base;
using Problems.MicroRTS;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.MicroRTS
{
    public class LateGame : ConditionNode
    {
        public int stepThreshold = 1000;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override bool CheckConditions()
        {
            var env = GetEnvironment();
            if (env == null) return false;

            return env.CurrentSimulationSteps >= stepThreshold;
        }

        private MicroRTSEnvironmentController GetEnvironment()
        {
            return context.gameObject.GetComponentInParent<MicroRTSEnvironmentController>();
        }
    }
}
