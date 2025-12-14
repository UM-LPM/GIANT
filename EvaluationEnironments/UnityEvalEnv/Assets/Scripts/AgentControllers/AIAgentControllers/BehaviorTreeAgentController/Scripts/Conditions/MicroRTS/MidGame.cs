using Base;
using Problems.MicroRTS;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.MicroRTS
{
    public class MidGame : ConditionNode
    {
        public int earlyThreshold = 100;
        public int lateThreshold = 1000;

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

            int steps = env.CurrentSimulationSteps;
            return steps >= earlyThreshold && steps < lateThreshold;
        }

        private MicroRTSEnvironmentController GetEnvironment()
        {
            return context.gameObject.GetComponentInParent<MicroRTSEnvironmentController>();
        }
    }
}
