using Base;
using Problems.MicroRTS;
using Problems.MicroRTS.Core;
using System.Linq;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.MicroRTS
{
    public class BuildBarracks : ActionNode
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

            var workers = env.GetAllUnits()
                .Where(u => u.Player == playerID && u.Type.name == "Worker" && u.Resources > 0)
                .ToList();

            if (workers.Count == 0) return State.Failure;

            var barracksType = env.UnitTypeTable.GetUnitType("Barracks");
            if (barracksType == null) return State.Failure;

            foreach (var worker in workers)
            {
                if (executor.HasPendingAction(worker)) continue;

                if (FindFreeAdjacentSpace(env, worker, out int targetX, out int targetY))
                {
                    if (executor.ScheduleBuildAtPosition(worker, targetX, targetY, barracksType))
                    {
                        return State.Success;
                    }
                }
            }

            return State.Failure;
        }

        private bool FindFreeAdjacentSpace(MicroRTSEnvironmentController env, Unit worker, out int targetX, out int targetY)
        {
            targetX = worker.X;
            targetY = worker.Y;

            int[] directions = { MicroRTSUtils.DIRECTION_UP, MicroRTSUtils.DIRECTION_RIGHT, MicroRTSUtils.DIRECTION_DOWN, MicroRTSUtils.DIRECTION_LEFT };
            int[] dxs = { 0, 1, 0, -1 };
            int[] dys = { -1, 0, 1, 0 };

            for (int i = 0; i < directions.Length; i++)
            {
                int nx = worker.X + dxs[i];
                int ny = worker.Y + dys[i];

                if (nx < 0 || ny < 0 || nx >= env.MapWidth || ny >= env.MapHeight) continue;
                if (!env.IsWalkable(nx, ny)) continue;

                Unit existingUnit = env.GetUnitAt(nx, ny);
                if (existingUnit != null) continue;

                targetX = nx;
                targetY = ny;
                return true;
            }

            return false;
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
