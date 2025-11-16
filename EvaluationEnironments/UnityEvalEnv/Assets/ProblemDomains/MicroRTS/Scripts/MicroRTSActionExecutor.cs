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
        private Dictionary<Unit, (int targetX, int targetY, bool toResource)> harvestTargets = new Dictionary<Unit, (int, int, bool)>();
        private Dictionary<Unit, (int resourceX, int resourceY)> resourceTargets = new Dictionary<Unit, (int, int)>();
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
            ProcessHarvestNavigation();
        }

        public bool HasPendingAction(Unit unit)
        {
            return unit != null && pendingActions.ContainsKey(unit);
        }

        public bool GetNextStepTowardTarget(Unit unit, int targetX, int targetY, out int stepX, out int stepY)
        {
            return GetNextStepTowardTargetInternal(unit, targetX, targetY, out stepX, out stepY);
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
            ProcessHarvestNavigation();

            int teamID = agent.TeamIdentifier.TeamID;

            var playerUnits = environmentController.GetAllUnits()
                .Where(u => u.Player == teamID && u.HitPoints > 0)
                .ToList();

            foreach (Unit unit in playerUnits)
            {
                if (pendingActions.ContainsKey(unit)) continue;

                if (unit.Type.canHarvest && unit.Resources == 0)
                {
                    string harvestActionName = $"harvestDirection_unit{unit.ID}";
                    int harvestDirection = agent.ActionBuffer.GetDiscreteAction(harvestActionName);

                    if (harvestDirection != MicroRTSUtils.DIRECTION_NONE)
                    {
                        Vector2Int offset = MicroRTSUtils.GetDirectionOffset(harvestDirection);
                        int targetX = unit.X + offset.x;
                        int targetY = unit.Y + offset.y;
                        Unit targetUnit = environmentController.GetUnitAt(targetX, targetY);

                        if (targetUnit != null && targetUnit.Type.isResource && CanHarvestAt(unit, targetX, targetY))
                        {
                            int currentCycle = GetCurrentCycle();
                            var assignment = new MicroRTSActionAssignment(unit, MicroRTSActionAssignment.ACTION_TYPE_HARVEST, harvestDirection, currentCycle);
                            pendingActions[unit] = assignment;
                            continue;
                        }
                    }

                    string harvestTargetXName = $"harvestTargetX_unit{unit.ID}";
                    string harvestTargetYName = $"harvestTargetY_unit{unit.ID}";
                    int targetXCoord = agent.ActionBuffer.GetDiscreteAction(harvestTargetXName);
                    int targetYCoord = agent.ActionBuffer.GetDiscreteAction(harvestTargetYName);

                    if (targetXCoord >= 0 && targetYCoord >= 0)
                    {
                        Unit targetResource = environmentController.GetUnitAt(targetXCoord, targetYCoord);
                        if (targetResource != null && targetResource.Type.isResource && targetResource.Resources > 0)
                        {
                            harvestTargets[unit] = (targetXCoord, targetYCoord, true);
                            resourceTargets[unit] = (targetXCoord, targetYCoord);
                            continue;
                        }
                    }
                }

                if (unit.Type.canMove)
                {
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
                harvestTargets.Remove(unit);
                resourceTargets.Remove(unit);
            }

            foreach (var unit in readyActions)
            {
                if (!pendingActions.TryGetValue(unit, out var assignment)) continue;

                if (assignment.actionType == MicroRTSActionAssignment.ACTION_TYPE_MOVE)
                {
                    ExecuteMovement(unit, assignment.direction);
                }
                else if (assignment.actionType == MicroRTSActionAssignment.ACTION_TYPE_HARVEST)
                {
                    ExecuteHarvest(unit, assignment.direction);
                }
                // TODO: Implement timing for ATTACK actions (unit.AttackTime)
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

        private void ProcessHarvestNavigation()
        {
            if (environmentController == null) return;

            var toClear = new List<Unit>();
            var toUpdate = new Dictionary<Unit, (int x, int y, bool toResource)>();

            var ids = new List<Unit>(harvestTargets.Keys);
            foreach (var unit in ids)
            {
                if (unit == null || unit.HitPoints <= 0)
                {
                    toClear.Add(unit);
                    continue;
                }

                if (pendingActions.ContainsKey(unit))
                {
                    continue;
                }

                var target = harvestTargets[unit];

                if (target.toResource && unit.Resources > 0)
                {
                    if (FindNearestBase(unit, out int bx, out int by))
                    {
                        toUpdate[unit] = (bx, by, false);
                        target = (bx, by, false);
                    }
                    else
                    {
                        toClear.Add(unit);
                        continue;
                    }
                }

                if (!target.toResource && unit.Resources == 0)
                {
                    if (resourceTargets.TryGetValue(unit, out var rpos))
                    {
                        Unit resource = environmentController.GetUnitAt(rpos.resourceX, rpos.resourceY);
                        if (resource != null && resource.Type.isResource && resource.Resources > 0)
                        {
                            toUpdate[unit] = (rpos.resourceX, rpos.resourceY, true);
                            target = (rpos.resourceX, rpos.resourceY, true);
                        }
                        else
                        {
                            toClear.Add(unit);
                            continue;
                        }
                    }
                    else
                    {
                        toClear.Add(unit);
                        continue;
                    }
                }

                Unit targetUnit = environmentController.GetUnitAt(target.targetX, target.targetY);
                if (targetUnit == null)
                {
                    toClear.Add(unit);
                    continue;
                }

                if (target.toResource && !targetUnit.Type.isResource)
                {
                    toClear.Add(unit);
                    continue;
                }

                if (!target.toResource && (!targetUnit.Type.isStockpile || targetUnit.Player != unit.Player))
                {
                    toClear.Add(unit);
                    continue;
                }

                if (IsAdjacent(unit.X, unit.Y, target.targetX, target.targetY))
                {
                    if (target.toResource)
                    {
                        if (CanHarvestAt(unit, target.targetX, target.targetY))
                        {
                            int direction = GetDirectionToTarget(unit.X, unit.Y, target.targetX, target.targetY);
                            if (direction != MicroRTSUtils.DIRECTION_NONE)
                            {
                                int currentCycle = GetCurrentCycle();
                                var assignment = new MicroRTSActionAssignment(unit, MicroRTSActionAssignment.ACTION_TYPE_HARVEST, direction, currentCycle);
                                pendingActions[unit] = assignment;
                            }
                        }
                    }
                    else
                    {
                        if (unit.Resources > 0 && targetUnit.Player == unit.Player)
                        {
                            int direction = GetDirectionToTarget(unit.X, unit.Y, target.targetX, target.targetY);
                            if (direction != MicroRTSUtils.DIRECTION_NONE)
                            {
                                ExecuteReturn(unit, direction);
                            }
                        }
                    }
                }
                else
                {
                    if (GetNextStepTowardTargetInternal(unit, target.targetX, target.targetY, out int stepX, out int stepY))
                    {
                        int direction = GetDirectionToTarget(unit.X, unit.Y, stepX, stepY);
                        if (direction != MicroRTSUtils.DIRECTION_NONE && CanMoveTo(unit, stepX, stepY))
                        {
                            int currentCycle = GetCurrentCycle();
                            var assignment = new MicroRTSActionAssignment(unit, MicroRTSActionAssignment.ACTION_TYPE_MOVE, direction, currentCycle);
                            pendingActions[unit] = assignment;
                        }
                    }
                }
            }

            foreach (var unit in toClear)
            {
                harvestTargets.Remove(unit);
                resourceTargets.Remove(unit);
            }

            foreach (var kvp in toUpdate)
            {
                harvestTargets[kvp.Key] = kvp.Value;
            }
        }

        private bool GetNextStepTowardTargetInternal(Unit unit, int targetX, int targetY, out int stepX, out int stepY)
        {
            stepX = unit.X;
            stepY = unit.Y;

            if (environmentController == null) return false;

            int w = environmentController.MapWidth;
            int h = environmentController.MapHeight;

            int dx = targetX - unit.X;
            int dy = targetY - unit.Y;
            if (Mathf.Abs(dx) + Mathf.Abs(dy) == 1)
            {
                if (CanMoveTo(unit, targetX, targetY))
                {
                    stepX = targetX;
                    stepY = targetY;
                    return true;
                }
            }

            var goals = new HashSet<int>();

            int[][] preferredOrder = new int[][]
            {
                new int[] {targetX, targetY - 1},
                new int[] {targetX + 1, targetY},
                new int[] {targetX, targetY + 1},
                new int[] {targetX - 1, targetY}
            };

            void TryAddGoal(int gx, int gy)
            {
                if (gx < 0 || gy < 0 || gx >= w || gy >= h) return;
                if (!environmentController.IsWalkable(gx, gy)) return;
                Unit unitAtPos = environmentController.GetUnitAt(gx, gy);
                if (unitAtPos != null && unitAtPos.ID != unit.ID) return;
                goals.Add(gx + gy * w);
            }

            foreach (var pos in preferredOrder)
            {
                TryAddGoal(pos[0], pos[1]);
            }

            if (goals.Count == 0) return false;

            int start = unit.X + unit.Y * w;
            var queue = new Queue<int>();
            var visited = new bool[w * h];
            var cameFrom = new int[w * h];
            for (int i = 0; i < cameFrom.Length; i++) cameFrom[i] = -1;
            queue.Enqueue(start);
            visited[start] = true;

            int found = -1;
            int[] dxs = { 0, 1, 0, -1 };
            int[] dys = { -1, 0, 1, 0 };
            while (queue.Count > 0)
            {
                int cur = queue.Dequeue();
                if (goals.Contains(cur))
                {
                    found = cur;
                    break;
                }
                int cx = cur % w;
                int cy = cur / w;
                for (int dir = 0; dir < 4; dir++)
                {
                    int nx = cx + dxs[dir];
                    int ny = cy + dys[dir];
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                    int ni = nx + ny * w;
                    if (visited[ni]) continue;
                    if (!environmentController.IsWalkable(nx, ny)) continue;
                    Unit occ = environmentController.GetUnitAt(nx, ny);
                    if (occ != null && occ.ID != unit.ID) continue;
                    visited[ni] = true;
                    cameFrom[ni] = cur;
                    queue.Enqueue(ni);
                }
            }

            if (found == -1) return false;

            int curIdx = found;
            int prev = cameFrom[curIdx];
            while (prev != -1 && prev != start)
            {
                curIdx = prev;
                prev = cameFrom[curIdx];
            }

            int sx = curIdx % w;
            int sy = curIdx / w;

            stepX = sx;
            stepY = sy;

            return !(sx == unit.X && sy == unit.Y);
        }

        private void ExecuteHarvest(Unit unit, int direction)
        {
            if (unit == null || environmentController == null) return;
            if (!unit.Type.canHarvest || unit.Resources > 0) return;

            Vector2Int offset = MicroRTSUtils.GetDirectionOffset(direction);
            int targetX = unit.X + offset.x;
            int targetY = unit.Y + offset.y;

            Unit resource = environmentController.GetUnitAt(targetX, targetY);
            if (resource == null || !resource.Type.isResource) return;

            resource.SetResources(resource.Resources - unit.HarvestAmount);
            if (resource.Resources <= 0)
            {
                environmentController.RemoveUnit(resource);
            }

            unit.SetResources(unit.HarvestAmount);
        }

        private void ExecuteReturn(Unit unit, int direction)
        {
            if (unit == null || environmentController == null) return;
            if (unit.Resources == 0) return;

            Vector2Int offset = MicroRTSUtils.GetDirectionOffset(direction);
            int targetX = unit.X + offset.x;
            int targetY = unit.Y + offset.y;

            Unit baseUnit = environmentController.GetUnitAt(targetX, targetY);
            if (baseUnit == null || !baseUnit.Type.isStockpile || baseUnit.Player != unit.Player) return;

            var player = environmentController.GetPlayer(unit.Player);
            if (player != null)
            {
                player.SetResources(player.Resources + unit.Resources);
                unit.SetResources(0);
            }
        }

        private bool CanHarvestAt(Unit unit, int targetX, int targetY)
        {
            if (unit == null || !unit.Type.canHarvest || unit.Resources > 0) return false;
            if (!IsAdjacent(unit.X, unit.Y, targetX, targetY)) return false;

            Unit target = environmentController.GetUnitAt(targetX, targetY);
            return target != null && target.Type.isResource && target.Resources > 0;
        }

        private bool IsAdjacent(int x1, int y1, int x2, int y2)
        {
            int dx = Mathf.Abs(x1 - x2);
            int dy = Mathf.Abs(y1 - y2);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        private int GetDirectionToTarget(int fromX, int fromY, int toX, int toY)
        {
            int dx = toX - fromX;
            int dy = toY - fromY;

            if (dx == 0 && dy == -1) return MicroRTSUtils.DIRECTION_UP;
            if (dx == 1 && dy == 0) return MicroRTSUtils.DIRECTION_RIGHT;
            if (dx == 0 && dy == 1) return MicroRTSUtils.DIRECTION_DOWN;
            if (dx == -1 && dy == 0) return MicroRTSUtils.DIRECTION_LEFT;

            return MicroRTSUtils.DIRECTION_NONE;
        }

        private Unit FindNearestResource(Unit unit)
        {
            if (unit == null || environmentController == null) return null;

            Unit nearest = null;
            int minDist = int.MaxValue;

            foreach (var u in environmentController.GetAllUnits())
            {
                if (u.Type.isResource && u.Resources > 0)
                {
                    int dist = Mathf.Abs(u.X - unit.X) + Mathf.Abs(u.Y - unit.Y);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = u;
                    }
                }
            }

            return nearest;
        }

        private bool FindNearestBase(Unit unit, out int bx, out int by)
        {
            bx = 0;
            by = 0;

            if (unit == null || environmentController == null) return false;

            int best = int.MaxValue;
            bool found = false;

            foreach (var u in environmentController.GetAllUnits())
            {
                if (u.Player == unit.Player && u.Type.isStockpile)
                {
                    int d = Mathf.Abs(u.X - unit.X) + Mathf.Abs(u.Y - unit.Y);
                    if (d < best)
                    {
                        best = d;
                        bx = u.X;
                        by = u.Y;
                        found = true;
                    }
                }
            }

            return found;
        }
    }
}
