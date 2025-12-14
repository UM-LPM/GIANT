using Base;
using Problems.MicroRTS;
using Problems.MicroRTS.Core;
using System.Linq;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.MicroRTS
{
    public class TrainWorker : ActionNode
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

            var baseUnits = env.GetAllUnits()
                .Where(u => u.Player == playerID && u.Type.name == "Base" && u.HitPoints > 0)
                .ToList();

            foreach (var baseUnit in baseUnits)
            {
                if (executor.HasPendingAction(baseUnit)) continue;

                if (baseUnit.Type.produces != null && baseUnit.Type.produces.Count > 0)
                {
                    var workerType = baseUnit.Type.produces.FirstOrDefault(pt => pt.name == "Worker");
                    if (workerType != null)
                    {
                        blackboard.actionsOut.AddDiscreteAction($"produce_Worker_unit{baseUnit.ID}", 1);
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
