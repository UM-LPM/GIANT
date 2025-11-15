using AgentControllers;
using Base;
using UnityEngine;
using Problems.MicroRTS.Core;
using System.Linq;

namespace Problems.MicroRTS
{
    public class MicroRTSActionExecutor : ActionExecutor
    {
        // This changed to match project imp
        // Original MicroRTS: DIRECTION_NONE=-1, UP=0, RIGHT=1, DOWN=2, LEFT=3
        // Now matches project: DIRECTION_NONE=0, UP=1, RIGHT=2, DOWN=3, LEFT=4 (same as action values)
        private const int DIRECTION_NONE = 0;
        private const int DIRECTION_UP = 1;
        private const int DIRECTION_RIGHT = 2;
        private const int DIRECTION_DOWN = 3;
        private const int DIRECTION_LEFT = 4;

        private Vector2Int GetDirectionOffset(int direction)
        {
            switch (direction)
            {
                case DIRECTION_UP:
                    return new Vector2Int(0, -1); // moving up -> decrease Y
                case DIRECTION_RIGHT:
                    return new Vector2Int(1, 0); // moving right -> increase X
                case DIRECTION_DOWN:
                    return new Vector2Int(0, 1); // moving down -> increase Y
                case DIRECTION_LEFT:
                    return new Vector2Int(-1, 0); // moving left -> decrease X
                default:
                    return Vector2Int.zero;
            }
        }

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

                if (actionValue == DIRECTION_NONE) continue;

                int direction = actionValue;

                // Get direction offset
                Vector2Int offset = GetDirectionOffset(direction);
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