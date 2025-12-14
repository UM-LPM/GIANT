using Base;
using Problems.MicroRTS;
using System.Linq;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.MicroRTS
{
    public class HaveBarracks : ConditionNode
    {
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

            return env.GetAllUnits()
                .Any(u => u.Player == playerID && u.Type.name == "Barracks" && u.HitPoints > 0);
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
