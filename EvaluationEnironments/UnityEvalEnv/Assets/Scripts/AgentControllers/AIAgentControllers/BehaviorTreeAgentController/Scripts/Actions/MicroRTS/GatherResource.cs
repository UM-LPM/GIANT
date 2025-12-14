using Base;
using Problems.MicroRTS;
using Problems.MicroRTS.Core;
using System.Linq;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.MicroRTS
{
    public class GatherResource : ActionNode
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

            int playerID = GetPlayerID();
            if (playerID < 0) return State.Failure;

            var workers = env.GetAllUnits()
                .Where(u => u.Player == playerID && u.Type.name == "Worker" && u.HitPoints > 0 && u.Resources == 0)
                .ToList();

            if (workers.Count == 0) return State.Failure;

            var resources = env.GetAllUnits()
                .Where(u => u.Type.isResource && u.Resources > 0)
                .ToList();

            if (resources.Count == 0) return State.Failure;

            var worker = workers.FirstOrDefault();
            if (worker == null) return State.Failure;

            var nearestResource = resources
                .OrderBy(r => Mathf.Abs(r.X - worker.X) + Mathf.Abs(r.Y - worker.Y))
                .FirstOrDefault();

            if (nearestResource == null) return State.Failure;

            blackboard.actionsOut.AddDiscreteAction($"harvestTargetX_unit{worker.ID}", nearestResource.X);
            blackboard.actionsOut.AddDiscreteAction($"harvestTargetY_unit{worker.ID}", nearestResource.Y);

            return State.Success;
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
