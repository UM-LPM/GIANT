using Base;
using Problems.MicroRTS;
using Problems.MicroRTS.Core;
using System.Linq;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController.MicroRTS
{
    public class AttackClosestEnemy : ActionNode
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

            var attackers = env.GetAllUnits()
                .Where(u => u.Player == playerID && u.Type.canAttack && u.HitPoints > 0)
                .ToList();

            if (attackers.Count == 0) return State.Failure;

            var enemies = env.GetAllUnits()
                .Where(u => u.Player != playerID && u.Player >= 0 && u.HitPoints > 0 && !u.Type.isResource)
                .ToList();

            if (enemies.Count == 0) return State.Failure;

            bool actionWritten = false;

            foreach (var attacker in attackers)
            {
                var closestEnemy = enemies
                    .OrderBy(e => Mathf.Abs(e.X - attacker.X) + Mathf.Abs(e.Y - attacker.Y))
                    .FirstOrDefault();

                if (closestEnemy != null)
                {
                    blackboard.actionsOut.AddDiscreteAction($"attackTargetX_unit{attacker.ID}", closestEnemy.X);
                    blackboard.actionsOut.AddDiscreteAction($"attackTargetY_unit{attacker.ID}", closestEnemy.Y);
                    actionWritten = true;
                }
            }

            return actionWritten ? State.Success : State.Failure;
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
