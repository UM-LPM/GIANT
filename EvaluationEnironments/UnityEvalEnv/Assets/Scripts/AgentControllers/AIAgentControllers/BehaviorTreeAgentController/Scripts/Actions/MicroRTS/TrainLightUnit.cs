using Base;
using Problems.MicroRTS;
using Problems.MicroRTS.Core;
using System.Linq;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.MicroRTS
{
    public class TrainLightUnit : ActionNode
    {
        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            var env = GetEnvironment();
            if (env == null) return State.Failure;

            var executor = GetExecutor();
            if (executor == null) return State.Failure;

            int playerID = GetPlayerID();
            if (playerID < 0) return State.Failure;

            var player = env.GetPlayer(playerID);
            if (player == null) return State.Failure;

            var barracks = env.GetAllUnits()
                .Where(u => u.Player == playerID && u.Type.name == "Barracks" && u.HitPoints > 0)
                .ToList();

            if (barracks.Count == 0) return State.Failure;

            foreach (var barrack in barracks)
            {
                if (executor.HasPendingAction(barrack)) continue;

                if (barrack.Type.produces != null && barrack.Type.produces.Count > 0)
                {
                    var lightType = barrack.Type.produces.FirstOrDefault(pt => pt.name == "Light");
                    if (lightType != null && player.Resources >= lightType.cost)
                    {
                        blackboard.actionsOut.AddDiscreteAction($"produce_Light_unit{barrack.ID}", 1);
                        return State.Success;
                    }
                }
            }

            return State.Failure;
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

        private MicroRTSActionExecutor GetExecutor()
        {
            var env = GetEnvironment();
            return env != null ? env.GetComponent<MicroRTSActionExecutor>() : null;
        }
    }
}
