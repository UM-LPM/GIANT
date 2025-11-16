using AgentControllers;
using Base;
using UnityEngine;
using Problems.MicroRTS.Core;
using System.Collections.Generic;
using System.Linq;

namespace Problems.MicroRTS
{
    public class MicroRTSActionExecutor : ActionExecutor
    {
        private MicroRTSEnvironmentController environmentController;
        private Dictionary<Unit, MicroRTSActionAssignment> pendingActions = new Dictionary<Unit, MicroRTSActionAssignment>();
        private float? cachedCyclesPerSecond = null;

        private float CyclesPerSecond
        {
            get
            {
                if (cachedCyclesPerSecond.HasValue)
                {
                    return cachedCyclesPerSecond.Value;
                }

                float fixedDeltaTime = Time.fixedDeltaTime;
                if (fixedDeltaTime <= 0f)
                {
                    Debug.LogWarning("Time.fixedDeltaTime is invalid, using default 50 cycles/second");
                    cachedCyclesPerSecond = 50f;
                    return cachedCyclesPerSecond.Value;
                }

                cachedCyclesPerSecond = 1f / fixedDeltaTime;
                return cachedCyclesPerSecond.Value;
            }
        }

        public float GetCyclesPerSecond()
        {
            return CyclesPerSecond;
        }

        private void Awake()
        {
            environmentController = GetComponentInParent<MicroRTSEnvironmentController>();
            if (environmentController == null)
            {
                Debug.LogError("Environment controller not found");
            }
        }

        public void ScheduleAction(MicroRTSActionAssignment assignment)
        {
            if (assignment == null || assignment.unit == null) return;
            pendingActions[assignment.unit] = assignment;
        }

        public int GetCurrentCycle()
        {
            if (environmentController == null) return 0;
            return SimulationTimeToCycle(environmentController.CurrentSimulationTime);
        }

        public void ProcessActions()
        {
            ExecuteReadyActions();
        }

        private int SimulationTimeToCycle(float simulationTime)
        {
            return Mathf.FloorToInt(simulationTime * CyclesPerSecond);
        }

        public override void ExecuteActions(AgentComponent agent)
        {
            if (environmentController == null) return;
            if (agent.ActionBuffer == null) return;
            if (agent.TeamIdentifier == null) return;

            ExecuteReadyActions();

            int teamID = agent.TeamIdentifier.TeamID;

            var playerUnits = environmentController.GetAllUnits()
                .Where(u => u.Player == teamID && u.HitPoints > 0)
                .ToList();

            foreach (Unit unit in playerUnits)
            {
                if (!unit.Type.canMove) continue;
                if (pendingActions.ContainsKey(unit)) continue;

                string actionName = $"moveDirection_unit{unit.ID}";
                int actionValue = agent.ActionBuffer.GetDiscreteAction(actionName);

                if (actionValue == MicroRTSUtils.DIRECTION_NONE) continue;

                int direction = actionValue;
                Vector2Int offset = MicroRTSUtils.GetDirectionOffset(direction);
                int newX = unit.X + offset.x;
                int newY = unit.Y + offset.y;

                if (CanMoveTo(unit, newX, newY))
                {
                    int currentCycle = GetCurrentCycle();
                    var assignment = new MicroRTSActionAssignment(unit, MicroRTSActionAssignment.ACTION_TYPE_MOVE, direction, currentCycle);
                    pendingActions[unit] = assignment;
                }
            }
        }

        private void ExecuteReadyActions()
        {
            var readyActions = new List<Unit>();
            var toRemove = new List<Unit>();

            foreach (var kvp in pendingActions)
            {
                var assignment = kvp.Value;
                if (assignment == null || assignment.unit == null)
                {
                    toRemove.Add(kvp.Key);
                    continue;
                }

                if (assignment.unit.HitPoints <= 0)
                {
                    toRemove.Add(assignment.unit);
                    continue;
                }

                int eta = assignment.GetETA();
                int requiredCycle = assignment.assignmentTime + eta;
                int currentCycle = GetCurrentCycle();
                if (currentCycle >= requiredCycle)
                {
                    readyActions.Add(assignment.unit);
                }
            }

            foreach (var unit in toRemove)
            {
                pendingActions.Remove(unit);
            }

            foreach (var unit in readyActions)
            {
                if (!pendingActions.TryGetValue(unit, out var assignment)) continue;

                if (assignment.actionType == MicroRTSActionAssignment.ACTION_TYPE_MOVE)
                {
                    ExecuteMovement(unit, assignment.direction);
                }
                // TODO: Implement timing for ATTACK actions (unit.AttackTime)
                // TODO: Implement timing for HARVEST actions (unit.HarvestTime)
                // TODO: Implement timing for RETURN actions (unit.MoveTime)
                // TODO: Implement timing for PRODUCE actions (unitType.produceTime)

                pendingActions.Remove(unit);
            }
        }

        private void ExecuteMovement(Unit unit, int direction)
        {
            if (unit == null || environmentController == null) return;

            Vector2Int offset = MicroRTSUtils.GetDirectionOffset(direction);
            int newX = unit.X + offset.x;
            int newY = unit.Y + offset.y;

            if (CanMoveTo(unit, newX, newY))
            {
                unit.SetX(newX);
                unit.SetY(newY);
                SyncUnitVisualPosition(unit);
            }
        }

        private bool CanMoveTo(Unit unit, int newX, int newY)
        {
            if (environmentController == null) return false;
            if (unit == null) return false;

            if (newX < 0 || newX >= environmentController.MapWidth || newY < 0 || newY >= environmentController.MapHeight)
            {
                return false;
            }

            if (!environmentController.IsWalkable(newX, newY))
            {
                return false;
            }

            Unit unitAtPosition = environmentController.GetUnitAt(newX, newY);
            if (unitAtPosition != null && unitAtPosition.ID != unit.ID)
            {
                return false;
            }

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
            if (unitObj == null) return;

            MicroRTSUnitComponent unitComponent = environmentController.GetUnitComponent(unitObj);
            if (unitComponent == null) return;

            Vector3 worldPosition = environmentController.GridToWorldPosition(unit.X, unit.Y);
            unitComponent.SyncPositionFromGrid(worldPosition);
        }
    }
}