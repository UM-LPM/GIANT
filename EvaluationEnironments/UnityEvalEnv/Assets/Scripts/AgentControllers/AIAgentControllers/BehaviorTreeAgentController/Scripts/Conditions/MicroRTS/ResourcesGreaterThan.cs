using Base;
using Problems.MicroRTS;
using System.Linq;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.MicroRTS
{
    public class ResourcesGreaterThan : ConditionNode
    {
        public int threshold = 0;

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

            int playerID = GetPlayerID();
            if (playerID < 0) return false;

            var player = env.GetPlayer(playerID);
            if (player == null) return false;

            return player.Resources > threshold;
        }

        private MicroRTSEnvironmentController GetEnvironment()
        {
            return context.gameObject.GetComponentInParent<MicroRTSEnvironmentController>();
        }

        private int GetPlayerID()
        {
            var teamID = context.gameObject.GetComponent<TeamIdentifier>();
            return teamID != null ? teamID.TeamID : -1;
        }
    }
}
