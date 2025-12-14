using Base;
using Problems.MicroRTS;
using Problems.MicroRTS.Core;
using System.Linq;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.MicroRTS
{
    public class WorkersLessThan : ConditionNode
    {
        public int maxWorkers = 3;

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

            int workerCount = env.GetAllUnits()
                .Where(u => u.Player == playerID && u.Type.name == "Worker" && u.HitPoints > 0)
                .Count();

            return workerCount < maxWorkers;
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
