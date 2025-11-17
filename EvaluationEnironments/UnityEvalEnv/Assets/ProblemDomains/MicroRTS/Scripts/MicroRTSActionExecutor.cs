using AgentControllers;
using Base;
using UnityEngine;
using Problems.MicroRTS.Core;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace Problems.MicroRTS
{
    public class MicroRTSActionExecutor : ActionExecutor
    {
        private MicroRTSEnvironmentController environmentController;
        private Dictionary<Unit, MicroRTSActionAssignment> pendingActions = new Dictionary<Unit, MicroRTSActionAssignment>();
        private Dictionary<Unit, (int targetX, int targetY, bool toResource)> harvestTargets = new Dictionary<Unit, (int, int, bool)>();
        private Dictionary<Unit, (int resourceX, int resourceY)> resourceTargets = new Dictionary<Unit, (int, int)>();
        private Dictionary<Unit, (int spawnX, int spawnY, UnitType unitType)> producingUnits = new Dictionary<Unit, (int, int, UnitType)>();
        private Dictionary<Unit, (int targetX, int targetY)> attackTargets = new Dictionary<Unit, (int, int)>();
        [SerializeField] private bool useDeterministicDamage = false;
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
            ProcessAttackNavigation();
        }

        public bool HasPendingAction(Unit unit)
        {
            return unit != null && pendingActions.ContainsKey(unit);
        }

        public void SetAttackTarget(Unit attacker, int targetX, int targetY)
        {
            if (attacker != null)
            {
                attackTargets[attacker] = (targetX, targetY);
            }
        }

        public bool ScheduleBuildAtPosition(Unit builder, int targetX, int targetY, UnitType unitType)
        {
            if (builder == null || environmentController == null || unitType == null) return false;

            int dx = targetX - builder.X;
            int dy = targetY - builder.Y;

            if (Mathf.Abs(dx) + Mathf.Abs(dy) != 1)
            {
                DebugSystem.LogWarning($"Target position ({targetX}, {targetY}) is not adjacent to builder at ({builder.X}, {builder.Y})");
                return false;
            }

            int direction = GetDirectionToTarget(builder.X, builder.Y, targetX, targetY);
            if (direction == MicroRTSUtils.DIRECTION_NONE) return false;

            if (targetX < 0 || targetX >= environmentController.MapWidth ||
                targetY < 0 || targetY >= environmentController.MapHeight)
            {
                return false;
            }

            if (!environmentController.IsWalkable(targetX, targetY))
            {
                return false;
            }

            Unit existingUnit = environmentController.GetUnitAt(targetX, targetY);
            if (existingUnit != null)
            {
                return false;
            }

            bool spaceReserved = false;
            foreach (var producing in producingUnits.Values)
            {
                if (producing.spawnX == targetX && producing.spawnY == targetY)
                {
                    spaceReserved = true;
                    break;
                }
            }
            if (spaceReserved)
            {
                return false;
            }

            var player = environmentController.GetPlayer(builder.Player);
            if (player == null || player.Resources < unitType.cost)
            {
                return false;
            }

            if (pendingActions.ContainsKey(builder))
            {
                return false;
            }

            int currentCycle = GetCurrentCycle();
            var assignment = new MicroRTSActionAssignment(builder, MicroRTSActionAssignment.ACTION_TYPE_PRODUCE, currentCycle, direction, null, null, unitType);
            pendingActions[builder] = assignment;
            producingUnits[builder] = (targetX, targetY, unitType);
            DebugSystem.Log($"{builder.Type.name} at ({builder.X}, {builder.Y}) scheduled to build {unitType.name} at ({targetX}, {targetY}). Production will take {unitType.produceTime} cycles");
            return true;
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
            ProcessAttackNavigation();

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
                            var assignment = new MicroRTSActionAssignment(unit, MicroRTSActionAssignment.ACTION_TYPE_HARVEST, currentCycle, harvestDirection);
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
                        var assignment = new MicroRTSActionAssignment(unit, MicroRTSActionAssignment.ACTION_TYPE_MOVE, currentCycle, direction);
                        pendingActions[unit] = assignment;
                    }
                }

                if (unit.Type.produces != null && unit.Type.produces.Count > 0)
                {
                    foreach (UnitType producibleType in unit.Type.produces)
                    {
                        string produceActionName = $"produce_{producibleType.name}_unit{unit.ID}";
                        int produceRequested = agent.ActionBuffer.GetDiscreteAction(produceActionName);

                        if (produceRequested != 0)
                        {
                            if (FindFreeAdjacentSpace(unit, out int spawnX, out int spawnY, out int foundDirection))
                            {
                                var player = environmentController.GetPlayer(unit.Player);
                                if (player != null && player.Resources >= producibleType.cost)
                                {
                                    int currentCycle = GetCurrentCycle();
                                    var assignment = new MicroRTSActionAssignment(unit, MicroRTSActionAssignment.ACTION_TYPE_PRODUCE, currentCycle, foundDirection, null, null, producibleType);
                                    pendingActions[unit] = assignment;
                                    producingUnits[unit] = (spawnX, spawnY, producibleType);
                                    string producerType = unit.Type.name;
                                    DebugSystem.Log($"{producerType} at ({unit.X}, {unit.Y}) started producing {producibleType.name} at ({spawnX}, {spawnY}). Production will take {producibleType.produceTime} cycles");
                                    break;
                                }
                                else if (player != null)
                                {
                                    string producerType = unit.Type.name;
                                    DebugSystem.Log($"{producerType} at ({unit.X}, {unit.Y}) cannot produce {producibleType.name} - insufficient resources (have {player.Resources}, need {producibleType.cost})");
                                }
                            }
                            else
                            {
                                string producerType = unit.Type.name;
                                DebugSystem.LogWarning($"{producerType} at ({unit.X}, {unit.Y}) cannot produce {producibleType.name} - no free adjacent space available");
                            }
                            break;
                        }
                    }
                }

                if (unit.Type.canAttack)
                {
                    string attackTargetXName = $"attackTargetX_unit{unit.ID}";
                    string attackTargetYName = $"attackTargetY_unit{unit.ID}";
                    int targetXCoord = agent.ActionBuffer.GetDiscreteAction(attackTargetXName);
                    int targetYCoord = agent.ActionBuffer.GetDiscreteAction(attackTargetYName);

                    if (targetXCoord >= 0 && targetYCoord >= 0)
                    {
                        Unit targetUnit = environmentController.GetUnitAt(targetXCoord, targetYCoord);
                        if (targetUnit != null && ValidateAttackTarget(unit, targetUnit))
                        {
                            attackTargets[unit] = (targetXCoord, targetYCoord);
                            continue;
                        }
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
                producingUnits.Remove(unit);
                attackTargets.Remove(unit);
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
                else if (assignment.actionType == MicroRTSActionAssignment.ACTION_TYPE_PRODUCE)
                {
                    ExecuteProduce(unit, assignment.direction, assignment.unitType);
                }
                else if (assignment.actionType == MicroRTSActionAssignment.ACTION_TYPE_ATTACK)
                {
                    ExecuteAttack(unit, assignment.targetX, assignment.targetY);
                }
                // TODO: Implement timing for RETURN actions (unit.MoveTime)

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
                            var assignment = new MicroRTSActionAssignment(unit, MicroRTSActionAssignment.ACTION_TYPE_MOVE, currentCycle, direction);
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

        private void ProcessAttackNavigation()
        {
            if (environmentController == null) return;

            var toClear = new List<Unit>();

            var ids = new List<Unit>(attackTargets.Keys);
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

                var target = attackTargets[unit];
                Unit targetUnit = environmentController.GetUnitAt(target.targetX, target.targetY);

                if (targetUnit == null || targetUnit.HitPoints <= 0)
                {
                    toClear.Add(unit);
                    continue;
                }

                if (!ValidateAttackTarget(unit, targetUnit))
                {
                    toClear.Add(unit);
                    continue;
                }

                if (IsInAttackRange(unit, target.targetX, target.targetY))
                {
                    int currentCycle = GetCurrentCycle();
                    var assignment = new MicroRTSActionAssignment(unit, MicroRTSActionAssignment.ACTION_TYPE_ATTACK, currentCycle, null, target.targetX, target.targetY);
                    pendingActions[unit] = assignment;
                }
                else
                {
                    if (GetNextStepTowardTargetInternal(unit, target.targetX, target.targetY, out int stepX, out int stepY))
                    {
                        int direction = GetDirectionToTarget(unit.X, unit.Y, stepX, stepY);
                        if (direction != MicroRTSUtils.DIRECTION_NONE && CanMoveTo(unit, stepX, stepY))
                        {
                            int currentCycle = GetCurrentCycle();
                            var assignment = new MicroRTSActionAssignment(unit, MicroRTSActionAssignment.ACTION_TYPE_MOVE, currentCycle, direction);
                            pendingActions[unit] = assignment;
                        }
                    }
                    else
                    {
                        toClear.Add(unit);
                    }
                }
            }

            foreach (var unit in toClear)
            {
                attackTargets.Remove(unit);
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

        private bool FindFreeAdjacentSpace(Unit producer, out int spawnX, out int spawnY, out int direction)
        {
            spawnX = producer.X;
            spawnY = producer.Y;
            direction = MicroRTSUtils.DIRECTION_NONE;

            if (producer == null || environmentController == null) return false;

            int[] directions = { MicroRTSUtils.DIRECTION_UP, MicroRTSUtils.DIRECTION_RIGHT, MicroRTSUtils.DIRECTION_DOWN, MicroRTSUtils.DIRECTION_LEFT };
            int[] dxs = { 0, 1, 0, -1 };
            int[] dys = { -1, 0, 1, 0 };

            for (int i = 0; i < directions.Length; i++)
            {
                int nx = producer.X + dxs[i];
                int ny = producer.Y + dys[i];

                if (nx < 0 || ny < 0 || nx >= environmentController.MapWidth || ny >= environmentController.MapHeight) continue;
                if (!environmentController.IsWalkable(nx, ny)) continue;

                Unit existingUnit = environmentController.GetUnitAt(nx, ny);
                if (existingUnit != null) continue;

                bool spaceReserved = false;
                foreach (var producing in producingUnits.Values)
                {
                    if (producing.spawnX == nx && producing.spawnY == ny)
                    {
                        spaceReserved = true;
                        break;
                    }
                }
                if (spaceReserved) continue;

                spawnX = nx;
                spawnY = ny;
                direction = directions[i];
                return true;
            }

            return false;
        }

        private void ExecuteProduce(Unit producer, int direction, UnitType unitType)
        {
            if (producer == null || environmentController == null || unitType == null) return;

            int targetX = producer.X;
            int targetY = producer.Y;

            Vector2Int offset = MicroRTSUtils.GetDirectionOffset(direction);
            targetX += offset.x;
            targetY += offset.y;

            string producerType = producer.Type.name;

            if (targetX < 0 || targetX >= environmentController.MapWidth ||
                targetY < 0 || targetY >= environmentController.MapHeight)
            {
                DebugSystem.LogWarning($"{producerType} at ({producer.X}, {producer.Y}) cannot spawn {unitType.name} - target position ({targetX}, {targetY}) is out of bounds");
                return;
            }

            if (!environmentController.IsWalkable(targetX, targetY))
            {
                DebugSystem.LogWarning($"{producerType} at ({producer.X}, {producer.Y}) cannot spawn {unitType.name} - position ({targetX}, {targetY}) is not walkable");
                return;
            }

            Unit existingUnit = environmentController.GetUnitAt(targetX, targetY);
            if (existingUnit != null)
            {
                DebugSystem.LogWarning($"{producerType} at ({producer.X}, {producer.Y}) cannot spawn {unitType.name} - position ({targetX}, {targetY}) is occupied");
                return;
            }

            bool spaceReserved = false;
            foreach (var kvp in producingUnits)
            {
                if (kvp.Key != producer && kvp.Value.spawnX == targetX && kvp.Value.spawnY == targetY)
                {
                    spaceReserved = true;
                    break;
                }
            }
            if (spaceReserved)
            {
                DebugSystem.LogWarning($"{producerType} at ({producer.X}, {producer.Y}) cannot spawn {unitType.name} - position ({targetX}, {targetY}) is reserved for production by another unit");
                return;
            }

            var player = environmentController.GetPlayer(producer.Player);
            if (player == null)
            {
                DebugSystem.LogWarning($"{producerType} at ({producer.X}, {producer.Y}) cannot spawn {unitType.name} - player {producer.Player} not found");
                return;
            }

            int unitCost = unitType.cost;
            if (player.Resources < unitCost)
            {
                DebugSystem.Log($"{producerType} at ({producer.X}, {producer.Y}) cannot spawn {unitType.name} - insufficient resources (have {player.Resources}, need {unitCost})");
                return;
            }

            Unit newUnit = new Unit(producer.Player, unitType, targetX, targetY, 0);
            player.SetResources(player.Resources - unitCost);

            Vector3 worldPosition = environmentController.GridToWorldPosition(targetX, targetY);
            GameObject unitPrefab = GetUnitPrefab(unitType.name);
            if (unitPrefab == null)
            {
                DebugSystem.LogError($"Cannot create {unitType.name} - prefab not found");
                environmentController.RemoveUnit(newUnit);
                player.SetResources(player.Resources + unitCost);
                return;
            }

            GameObject newUnitObj = Instantiate(unitPrefab, worldPosition, Quaternion.identity, environmentController.transform);
            newUnitObj.name = $"{unitType.name}_Player{producer.Player}_{newUnit.ID}";

            MicroRTSUnitComponent unitComponent = newUnitObj.GetComponent<MicroRTSUnitComponent>();
            if (unitComponent == null)
            {
                unitComponent = newUnitObj.AddComponent<MicroRTSUnitComponent>();
            }
            unitComponent.Initialize(newUnit);

            environmentController.RegisterUnit(newUnitObj, unitComponent, newUnit);

            producingUnits.Remove(producer);
            DebugSystem.LogSuccess($"{producerType} at ({producer.X}, {producer.Y}) spawned {unitType.name} at ({targetX}, {targetY}). Player {producer.Player} now has {player.Resources} resources");
        }

        private GameObject GetUnitPrefab(string unitTypeName)
        {
            if (environmentController == null) return null;

            var spawnPositions = environmentController.GetComponentInChildren<UnitSpawnPositions>();
            if (spawnPositions == null) return null;

            switch (unitTypeName)
            {
                case "Worker":
                    return spawnPositions.WorkerPrefab;
                case "Base":
                    return spawnPositions.BasePrefab;
                case "Barracks":
                    return spawnPositions.BarracksPrefab;
                case "Resource":
                    return spawnPositions.ResourcePrefab;
                default:
                    return null;
            }
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

        private bool ValidateAttackTarget(Unit attacker, Unit target)
        {
            if (attacker == null || target == null) return false;
            if (!attacker.Type.canAttack) return false;
            if (target.Player == attacker.Player || target.Player < 0) return false;
            return true;
        }

        private bool IsInAttackRange(Unit attacker, int targetX, int targetY)
        {
            if (attacker == null) return false;

            int dx = Mathf.Abs(targetX - attacker.X);
            int dy = Mathf.Abs(targetY - attacker.Y);
            int attackRange = attacker.AttackRange;

            if (attackRange == 1)
            {
                return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
            }
            else
            {
                int sqDistance = dx * dx + dy * dy;
                int sqRange = attackRange * attackRange;
                return sqDistance <= sqRange;
            }
        }

        private int CalculateDamage(Unit attacker)
        {
            if (attacker == null) return 0;

            if (useDeterministicDamage || attacker.MinDamage == attacker.MaxDamage)
            {
                return attacker.MinDamage;
            }
            else
            {
                return Random.Range(attacker.MinDamage, attacker.MaxDamage + 1);
            }
        }

        private void ExecuteAttack(Unit attacker, int targetX, int targetY)
        {
            if (attacker == null || environmentController == null) return;
            if (!attacker.Type.canAttack) return;

            Unit target = environmentController.GetUnitAt(targetX, targetY);
            if (target == null)
            {
                attackTargets.Remove(attacker);
                return;
            }

            if (!ValidateAttackTarget(attacker, target))
            {
                attackTargets.Remove(attacker);
                return;
            }

            if (!IsInAttackRange(attacker, targetX, targetY))
            {
                return;
            }

            int damage = CalculateDamage(attacker);
            int newHP = target.HitPoints - damage;
            target.SetHitPoints(newHP);

            DebugSystem.Log($"{attacker.Type.name} at ({attacker.X}, {attacker.Y}) attacked {target.Type.name} at ({targetX}, {targetY}) for {damage} damage. Target HP: {newHP}/{target.MaxHitPoints}");

            if (target.HitPoints <= 0)
            {
                environmentController.RemoveUnit(target);
                attackTargets.Remove(attacker);
                DebugSystem.LogSuccess($"{target.Type.name} at ({targetX}, {targetY}) was destroyed");
            }
            else
            {
                int currentCycle = GetCurrentCycle();
                var assignment = new MicroRTSActionAssignment(attacker, MicroRTSActionAssignment.ACTION_TYPE_ATTACK, currentCycle, null, targetX, targetY);
                pendingActions[attacker] = assignment;
            }
        }
    }
}
