using AgentControllers;
using Base;
using UnityEngine;
using Problems.MicroRTS.Core;
using System.Linq;

namespace Problems.MicroRTS
{
    public class MicroRTSActionExecutor : ActionExecutor
    {

        private MicroRTSEnvironmentController environmentController;

        private void Awake()
        {
            environmentController = GetComponentInParent<MicroRTSEnvironmentController>();
            if (environmentController == null)
            {
                Debug.LogError("MicroRTSActionExecutor: MicroRTSEnvironmentController not found in parent!");
            }
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            if (environmentController == null) return;
            if (agent.ActionBuffer == null) return;
            if (agent.TeamIdentifier == null) return;

            int teamID = agent.TeamIdentifier.TeamID;

            // Get all alive units for this player (TeamID = Player)
            var playerUnits = environmentController.GetAllUnits()
                .Where(u => u.Player == teamID && u.HitPoints > 0)
                .ToList();

            foreach (Unit unit in playerUnits)
            {
                if (!unit.Type.canMove) continue;

                // Movement action for this unit
                string actionName = $"moveDirection_unit{unit.ID}";
                int actionValue = agent.ActionBuffer.GetDiscreteAction(actionName);

                if (actionValue == MicroRTSUtils.DIRECTION_NONE) continue;

                int direction = actionValue;

                // Get direction offset
                Vector2Int offset = MicroRTSUtils.GetDirectionOffset(direction);
                int newX = unit.X + offset.x;
                int newY = unit.Y + offset.y;

                // Check if movement is valid
                if (CanMoveTo(unit, newX, newY))
                {
                    // Update logical position
                    unit.SetX(newX);
                    unit.SetY(newY);

                    // Sync visual position
                    SyncUnitVisualPosition(unit);
                }
            }
        }

        private bool CanMoveTo(Unit unit, int newX, int newY)
        {
            if (environmentController == null) return false;
            if (unit == null) return false;

            // Check bounds
            if (newX < 0 || newX >= environmentController.MapWidth || newY < 0 || newY >= environmentController.MapHeight)
            {
                return false;
            }

            // Check if walkable
            if (!environmentController.IsWalkable(newX, newY))
            {
                return false;
            }

            // Check unit collision
            Unit unitAtPosition = environmentController.GetUnitAt(newX, newY);
            if (unitAtPosition != null && unitAtPosition.ID != unit.ID)
            {
                return false;
            }

            // Check if unit can move
            if (!unit.Type.canMove)
            {
                return false;
            }

            return true;
        }

        private void SyncUnitVisualPosition(Unit unit)
        {
            if (environmentController == null || unit == null) return;

            GameObject unitObj = environmentController.GetUnitGameObject(unit.ID);
            // Return if unit GameObject was destroyed
            if (unitObj == null) return;

            MicroRTSUnitComponent unitComponent = environmentController.GetUnitComponent(unitObj);
            if (unitComponent == null) return;

            Vector3 worldPosition = environmentController.GridToWorldPosition(unit.X, unit.Y);
            unitComponent.SyncPositionFromGrid(worldPosition);
        }
    }
}