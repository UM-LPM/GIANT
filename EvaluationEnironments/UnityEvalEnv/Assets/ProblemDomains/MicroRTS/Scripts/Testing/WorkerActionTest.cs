using System.Collections;
using System.Linq;
using UnityEngine;
using Problems.MicroRTS.Core;
using Problems.MicroRTS;
using Utils;
using AgentControllers;
using System.Collections.Generic;

namespace Problems.MicroRTS.Testing
{
    public enum TestType
    {
        Movement,
        Harvest,
        Attack,
        BuildBarracks,
        BuildBase
    }

    public class WorkerActionTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private TestType testType = TestType.Movement;
        [SerializeField] private bool autoStartTest = true;
        [SerializeField] private float testStartDelay = 0.5f;
        [SerializeField] private float maxWaitTime = 5.0f;
        [SerializeField] private float checkInterval = 0.5f;

        private MicroRTSEnvironmentController environmentController;
        private MicroRTSActionExecutor actionExecutor;
        private Util util;
        private bool testRunning = false;

        void Start()
        {
            if (autoStartTest)
            {
                StartCoroutine(StartTestAfterDelay());
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.T) && !testRunning)
            {
                StartCoroutine(VerifyUnitSpawn());
            }
        }

        private IEnumerator StartTestAfterDelay()
        {
            yield return new WaitForSeconds(testStartDelay);
            StartCoroutine(VerifyUnitSpawn());
        }

        private IEnumerator VerifyUnitSpawn()
        {
            if (testRunning) yield break;
            testRunning = true;

            environmentController = FindFirstObjectByType<MicroRTSEnvironmentController>();
            if (environmentController == null)
            {
                DebugSystem.LogError("Environment controller not found");
                testRunning = false;
                yield break;
            }

            util = environmentController.GetComponent<Util>();
            if (util == null)
            {
                DebugSystem.LogError("Util component not found");
                testRunning = false;
                yield break;
            }

            actionExecutor = environmentController.GetComponent<MicroRTSActionExecutor>();
            if (actionExecutor == null)
            {
                DebugSystem.LogError("Action executor not found");
                testRunning = false;
                yield break;
            }

            float elapsedTime = 0f;
            while (elapsedTime < maxWaitTime)
            {
                var allUnits = environmentController.GetAllUnits();
                if (allUnits.Count >= 6)
                {
                    break;
                }

                yield return new WaitForSeconds(checkInterval);
                elapsedTime += checkInterval;
            }

            var units = environmentController.GetAllUnits();
            int resourceCount = 0;
            int team0WorkerCount = 0;
            int team0BaseCount = 0;
            int team1WorkerCount = 0;
            int team1BaseCount = 0;

            foreach (Unit unit in units)
            {
                string unitType = unit.Type?.name ?? "NULL";
                int team = unit.Player;

                if (team == -1)
                {
                    resourceCount++;
                }
                else if (team == 0)
                {
                    if (unitType == "Worker") team0WorkerCount++;
                    else if (unitType == "Base") team0BaseCount++;
                }
                else if (team == 1)
                {
                    if (unitType == "Worker") team1WorkerCount++;
                    else if (unitType == "Base") team1BaseCount++;
                }
            }

            bool allCorrect = resourceCount == 2 &&
                             team0WorkerCount == 1 && team0BaseCount == 1 &&
                             team1WorkerCount == 1 && team1BaseCount == 1;

            if (allCorrect)
            {
                DebugSystem.LogSuccess($"Units spawned: {resourceCount} resources, Team 0: {team0WorkerCount} worker/{team0BaseCount} base, Team 1: {team1WorkerCount} worker/{team1BaseCount} base");

                if (testType == TestType.Movement)
                {
                    yield return StartCoroutine(TestWorkerMovement());
                }
                else if (testType == TestType.Harvest)
                {
                    yield return StartCoroutine(TestWorkerHarvest());
                }
                else if (testType == TestType.Attack)
                {
                    yield return StartCoroutine(TestWorkerAttack());
                }
                else if (testType == TestType.BuildBarracks)
                {
                    yield return StartCoroutine(TestWorkerBuildBarracks());
                }
                else if (testType == TestType.BuildBase)
                {
                    yield return StartCoroutine(TestWorkerBuildBase());
                }
            }
            else
            {
                DebugSystem.LogWarning($"Unit spawn failed: {resourceCount} resources, Team 0: {team0WorkerCount} worker/{team0BaseCount} base, Team 1: {team1WorkerCount} worker/{team1BaseCount} base");
            }

            testRunning = false;
        }

        private Unit FindTeam0Worker()
        {
            if (environmentController == null) return null;

            var units = environmentController.GetAllUnits();
            return units.FirstOrDefault(u =>
                u.Player == 0 &&
                u.Type != null &&
                u.Type.name == "Worker" &&
                u.HitPoints > 0);
        }

        private Unit FindNearestResource(Unit worker)
        {
            if (environmentController == null || worker == null) return null;

            Unit nearest = null;
            int minDist = int.MaxValue;

            foreach (var unit in environmentController.GetAllUnits())
            {
                if (unit.Type.isResource && unit.Resources > 0)
                {
                    int dist = Mathf.Abs(unit.X - worker.X) + Mathf.Abs(unit.Y - worker.Y);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = unit;
                    }
                }
            }

            return nearest;
        }

        private Unit FindNearestBase(Unit worker)
        {
            if (environmentController == null || worker == null) return null;

            Unit nearest = null;
            int minDist = int.MaxValue;

            foreach (var unit in environmentController.GetAllUnits())
            {
                if (unit.Player == worker.Player && unit.Type.isStockpile)
                {
                    int dist = Mathf.Abs(unit.X - worker.X) + Mathf.Abs(unit.Y - worker.Y);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = unit;
                    }
                }
            }

            return nearest;
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

        private IEnumerator TestWorkerMovement()
        {
            Unit worker = FindTeam0Worker();
            if (worker == null)
            {
                DebugSystem.LogError("Team 0 worker not found");
                yield break;
            }

            if (util == null || util.Rnd == null)
            {
                DebugSystem.LogError("Seeded random not available");
                yield break;
            }

            if (actionExecutor == null)
            {
                DebugSystem.LogError("Action executor not found");
                yield break;
            }

            int initialX = worker.X;
            int initialY = worker.Y;
            int moveTime = worker.MoveTime;
            float cyclesPerSec = actionExecutor.GetCyclesPerSecond();
            float expectedMoveDuration = moveTime / cyclesPerSec;

            DebugSystem.Log($"Worker at ({initialX}, {initialY}), moveTime: {moveTime} cycles ({expectedMoveDuration:F2}s at {cyclesPerSec:F1} cycles/s), starting movement timing test");

            int[] allDirections = { MicroRTSUtils.DIRECTION_UP, MicroRTSUtils.DIRECTION_RIGHT, MicroRTSUtils.DIRECTION_DOWN, MicroRTSUtils.DIRECTION_LEFT };
            int successfulMoves = 0;
            int lastX = worker.X;
            int lastY = worker.Y;
            int actionScheduledCycle = 0;
            bool actionPending = false;

            while (true)
            {
                int currentX = worker.X;
                int currentY = worker.Y;
                int cycleBeforeProcess = actionExecutor.GetCurrentCycle();

                actionExecutor.ProcessActions();

                int newX = worker.X;
                int newY = worker.Y;

                if (newX != currentX || newY != currentY)
                {
                    int cyclesElapsed = cycleBeforeProcess - actionScheduledCycle;
                    successfulMoves++;

                    if (cyclesElapsed != moveTime)
                    {
                        DebugSystem.LogError($"Move {successfulMoves} wrong timing! Took {cyclesElapsed} cycles, expected exactly {moveTime} cycles (scheduled at {actionScheduledCycle}, executed at {cycleBeforeProcess})");
                    }
                    else
                    {
                        float actualDuration = environmentController.CurrentSimulationTime - (actionScheduledCycle / cyclesPerSec);
                        DebugSystem.Log($"Move {successfulMoves}: ({currentX}, {currentY}) -> ({newX}, {newY}) in {cyclesElapsed} cycles ({actualDuration:F3}s)");
                    }

                    lastX = newX;
                    lastY = newY;
                    actionPending = false;
                }

                if (!actionPending)
                {
                    int randomDirection = allDirections[util.Rnd.Next(allDirections.Length)];
                    Vector2Int offset = MicroRTSUtils.GetDirectionOffset(randomDirection);
                    int targetX = newX + offset.x;
                    int targetY = newY + offset.y;

                    if (CanMoveTo(worker, targetX, targetY))
                    {
                        actionScheduledCycle = actionExecutor.GetCurrentCycle();
                        ScheduleMovement(worker, randomDirection);
                        actionPending = true;
                    }
                }

                yield return new WaitForFixedUpdate();
            }
        }

        private IEnumerator TestWorkerHarvest()
        {
            Unit worker = FindTeam0Worker();
            if (worker == null)
            {
                DebugSystem.LogError("Team 0 worker not found");
                yield break;
            }

            if (actionExecutor == null)
            {
                DebugSystem.LogError("Action executor not found");
                yield break;
            }

            int harvestTime = worker.HarvestTime;
            float cyclesPerSec = actionExecutor.GetCyclesPerSecond();
            float expectedHarvestDuration = harvestTime / cyclesPerSec;

            DebugSystem.Log($"Worker at ({worker.X}, {worker.Y}), harvestTime: {harvestTime} cycles ({expectedHarvestDuration:F2}s at {cyclesPerSec:F1} cycles/s), starting harvest test");

            Unit targetResource = FindNearestResource(worker);
            if (targetResource == null)
            {
                DebugSystem.LogError("No harvestable resource found");
                yield break;
            }

            DebugSystem.Log($"Target resource at ({targetResource.X}, {targetResource.Y}) with {targetResource.Resources} resources");

            var allResources = environmentController.GetAllUnits().Where(u => u.Type.isResource && u.Resources > 0).ToList();
            DebugSystem.Log($"Total harvestable resources on map: {allResources.Count}");
            foreach (var res in allResources)
            {
                DebugSystem.Log($"  - Resource at ({res.X}, {res.Y}) with {res.Resources} resources");
            }

            int initialResourceAmount = targetResource.Resources;
            int previousWorkerResources = worker.Resources;
            int successfulHarvests = 0;
            int actionScheduledCycle = 0;
            bool harvestPending = false;
            bool navigatingToResource = true;
            Unit currentResource = targetResource;
            Unit currentBase = null;
            int previousX = worker.X;
            int previousY = worker.Y;

            while (true)
            {
                int cycleBeforeProcess = actionExecutor.GetCurrentCycle();
                actionExecutor.ProcessActions();

                int currentX = worker.X;
                int currentY = worker.Y;
                int currentWorkerResources = worker.Resources;
                int currentResourceAmount = currentResource != null ? currentResource.Resources : 0;
                bool hasPendingAction = actionExecutor.HasPendingAction(worker);

                if (currentX != previousX || currentY != previousY)
                {
                    DebugSystem.Log($"Worker moved: ({previousX}, {previousY}) -> ({currentX}, {currentY})");
                    previousX = currentX;
                    previousY = currentY;
                }

                if (navigatingToResource)
                {
                    DebugSystem.Log($"Navigating to resource at ({currentResource.X}, {currentResource.Y}), worker at ({worker.X}, {worker.Y}), resources: {worker.Resources}");
                    if (IsAdjacent(worker.X, worker.Y, currentResource.X, currentResource.Y))
                    {
                        DebugSystem.Log($"Adjacent to resource, harvesting...");
                        if (!harvestPending && !hasPendingAction && worker.Resources == 0)
                        {
                            int direction = GetDirectionToTarget(worker.X, worker.Y, currentResource.X, currentResource.Y);
                            if (direction != MicroRTSUtils.DIRECTION_NONE)
                            {
                                actionScheduledCycle = actionExecutor.GetCurrentCycle();
                                ScheduleHarvest(worker, direction);
                                harvestPending = true;
                                previousWorkerResources = worker.Resources;
                                DebugSystem.Log($"Scheduled harvest action at cycle {actionScheduledCycle}");
                            }
                        }
                    }
                    else
                    {
                        if (!harvestPending && !hasPendingAction)
                        {
                            DebugSystem.Log($"Not adjacent, scheduling move to ({currentResource.X}, {currentResource.Y})");
                            ScheduleMoveToTarget(worker, currentResource.X, currentResource.Y);
                        }
                        else
                        {
                            DebugSystem.Log($"Cannot move: harvestPending={harvestPending}, hasPendingAction={hasPendingAction}");
                        }
                    }

                    if (harvestPending && currentWorkerResources > previousWorkerResources)
                    {
                        int cyclesElapsed = cycleBeforeProcess - actionScheduledCycle;
                        successfulHarvests++;

                        if (cyclesElapsed != harvestTime)
                        {
                            DebugSystem.LogError($"Harvest {successfulHarvests} wrong timing! Took {cyclesElapsed} cycles, expected exactly {harvestTime} cycles");
                        }
                        else
                        {
                            DebugSystem.LogSuccess($"Harvest {successfulHarvests}: Worker now has {worker.Resources} resources (took {cyclesElapsed} cycles)");
                        }

                        harvestPending = false;
                        navigatingToResource = false;

                        if (currentBase == null)
                        {
                            currentBase = FindNearestBase(worker);
                            if (currentBase == null)
                            {
                                DebugSystem.LogError("No base found to return resources");
                                yield break;
                            }
                            DebugSystem.Log($"Navigating to own team base at ({currentBase.X}, {currentBase.Y}) (Team {currentBase.Player})");

                            if (currentBase.Player != worker.Player)
                            {
                                DebugSystem.LogError($"ERROR: Found base belongs to team {currentBase.Player}, but worker is team {worker.Player}!");
                            }
                        }
                    }

                    previousWorkerResources = currentWorkerResources;
                }
                else
                {
                    DebugSystem.Log($"Navigating to base at ({currentBase.X}, {currentBase.Y}), worker at ({worker.X}, {worker.Y}), resources: {worker.Resources}");
                    if (IsAdjacent(worker.X, worker.Y, currentBase.X, currentBase.Y))
                    {
                        DebugSystem.Log($"Adjacent to base, depositing...");
                        if (worker.Resources > 0 && !hasPendingAction)
                        {
                            var player = environmentController.GetPlayer(worker.Player);
                            int playerResourcesBefore = player != null ? player.Resources : 0;

                            int direction = GetDirectionToTarget(worker.X, worker.Y, currentBase.X, currentBase.Y);
                            if (direction != MicroRTSUtils.DIRECTION_NONE)
                            {
                                ExecuteReturn(worker, direction);

                                if (player != null && player.Resources > playerResourcesBefore)
                                {
                                    int returnedAmount = player.Resources - playerResourcesBefore;
                                    DebugSystem.LogSuccess($"Returned {returnedAmount} resources to base. Player now has {player.Resources} resources");
                                }

                                if (worker.Resources == 0)
                                {
                                    navigatingToResource = true;
                                    harvestPending = false;
                                    currentResource = FindNearestResource(worker);
                                    if (currentResource == null)
                                    {
                                        DebugSystem.Log("No more resources to harvest");
                                        yield break;
                                    }
                                    DebugSystem.Log($"Returning to resource at ({currentResource.X}, {currentResource.Y})");
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!hasPendingAction)
                        {
                            DebugSystem.Log($"Not adjacent to base, scheduling move to ({currentBase.X}, {currentBase.Y})");
                            ScheduleMoveToTarget(worker, currentBase.X, currentBase.Y);
                        }
                        else
                        {
                            DebugSystem.Log($"Cannot move to base: hasPendingAction={hasPendingAction}");
                        }
                    }
                }

                if (currentResource != null)
                {
                    Unit actualResource = environmentController.GetUnitAt(currentResource.X, currentResource.Y);
                    if (actualResource == null || actualResource.Resources <= 0 || !actualResource.Type.isResource)
                    {
                        DebugSystem.Log($"Resource at ({currentResource.X}, {currentResource.Y}) depleted or removed.");

                        if (worker.Resources > 0)
                        {
                            DebugSystem.Log($"Worker is carrying {worker.Resources} resources. Must return to base first.");
                            if (currentBase == null)
                            {
                                currentBase = FindNearestBase(worker);
                                if (currentBase == null)
                                {
                                    DebugSystem.LogError("No base found to return resources");
                                    yield break;
                                }
                                DebugSystem.Log($"Navigating to own team base at ({currentBase.X}, {currentBase.Y}) (Team {currentBase.Player})");
                            }
                            navigatingToResource = false;
                            harvestPending = false;
                        }
                        else
                        {
                            DebugSystem.Log($"Worker has no resources. Finding new resource...");
                            currentResource = FindNearestResource(worker);
                            if (currentResource == null)
                            {
                                DebugSystem.Log("No more resources to harvest");
                                yield break;
                            }
                            navigatingToResource = true;
                            harvestPending = false;
                            DebugSystem.Log($"New target resource at ({currentResource.X}, {currentResource.Y}) with {currentResource.Resources} resources");
                        }
                    }
                    else
                    {
                        currentResource = actualResource;
                    }
                }

                yield return new WaitForFixedUpdate();
            }
        }

        private void ScheduleMovement(Unit unit, int direction)
        {
            if (actionExecutor == null) return;

            var assignment = new MicroRTSActionAssignment(unit, MicroRTSActionAssignment.ACTION_TYPE_MOVE, GetCurrentGameTime(), direction);
            actionExecutor.ScheduleAction(assignment);
        }

        private void ScheduleHarvest(Unit unit, int direction)
        {
            if (actionExecutor == null) return;

            var assignment = new MicroRTSActionAssignment(unit, MicroRTSActionAssignment.ACTION_TYPE_HARVEST, GetCurrentGameTime(), direction);
            actionExecutor.ScheduleAction(assignment);
        }

        private void ScheduleMoveToTarget(Unit unit, int targetX, int targetY)
        {
            if (actionExecutor == null || environmentController == null) return;
            if (actionExecutor.HasPendingAction(unit)) return;

            if (actionExecutor.GetNextStepTowardTarget(unit, targetX, targetY, out int stepX, out int stepY))
            {
                int direction = GetDirectionToTarget(unit.X, unit.Y, stepX, stepY);
                if (direction != MicroRTSUtils.DIRECTION_NONE && CanMoveTo(unit, stepX, stepY))
                {
                    ScheduleMovement(unit, direction);
                }
            }
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

        private int GetCurrentGameTime()
        {
            return actionExecutor.GetCurrentCycle();
        }

        private Unit FindEnemyWorker(Unit attacker)
        {
            if (environmentController == null || attacker == null) return null;

            var units = environmentController.GetAllUnits();
            return units.FirstOrDefault(u =>
                u.Player != attacker.Player &&
                u.Player >= 0 &&
                u.Type != null &&
                u.Type.name == "Worker" &&
                u.HitPoints > 0);
        }

        private Unit FindEnemyBase(Unit attacker)
        {
            if (environmentController == null || attacker == null) return null;

            var units = environmentController.GetAllUnits();
            return units.FirstOrDefault(u =>
                u.Player != attacker.Player &&
                u.Player >= 0 &&
                u.Type != null &&
                u.Type.name == "Base" &&
                u.HitPoints > 0);
        }

        private IEnumerator TestWorkerAttack()
        {
            Unit attacker = FindTeam0Worker();
            if (attacker == null)
            {
                DebugSystem.LogError("Team 0 worker not found");
                yield break;
            }

            if (actionExecutor == null)
            {
                DebugSystem.LogError("Action executor not found");
                yield break;
            }

            if (!attacker.Type.canAttack)
            {
                DebugSystem.LogError("Worker cannot attack");
                yield break;
            }

            int attackTime = attacker.AttackTime;
            float cyclesPerSec = actionExecutor.GetCyclesPerSecond();
            float expectedAttackDuration = attackTime / cyclesPerSec;

            DebugSystem.Log($"Worker at ({attacker.X}, {attacker.Y}), attackTime: {attackTime} cycles ({expectedAttackDuration:F2}s at {cyclesPerSec:F1} cycles/s), starting attack test");

            Unit enemyWorker = FindEnemyWorker(attacker);
            Unit enemyBase = FindEnemyBase(attacker);

            if (enemyWorker == null && enemyBase == null)
            {
                DebugSystem.LogError("No enemy units found to attack");
                yield break;
            }

            bool attackingWorker = true;
            Unit currentTarget = enemyWorker ?? enemyBase;
            int previousX = attacker.X;
            int previousY = attacker.Y;
            int previousTargetHP = currentTarget != null ? currentTarget.HitPoints : 0;
            int successfulAttacks = 0;
            bool targetDestroyed = false;

            if (enemyWorker != null)
            {
                DebugSystem.Log($"Target enemy worker at ({enemyWorker.X}, {enemyWorker.Y}) with {enemyWorker.HitPoints} HP");
                actionExecutor.SetAttackTarget(attacker, enemyWorker.X, enemyWorker.Y);
            }
            else if (enemyBase != null)
            {
                DebugSystem.Log($"Target enemy base at ({enemyBase.X}, {enemyBase.Y}) with {enemyBase.HitPoints} HP");
                actionExecutor.SetAttackTarget(attacker, enemyBase.X, enemyBase.Y);
                attackingWorker = false;
            }

            while (true)
            {
                actionExecutor.ProcessActions();

                int currentX = attacker.X;
                int currentY = attacker.Y;
                bool hasPendingAction = actionExecutor.HasPendingAction(attacker);

                Unit actualTarget = environmentController.GetUnitAt(currentTarget.X, currentTarget.Y);

                if (actualTarget == null)
                {
                    if (!targetDestroyed)
                    {
                        DebugSystem.LogWarning($"Target {currentTarget.Type.name} at ({currentTarget.X}, {currentTarget.Y}) moved away or was destroyed");

                        if (attackingWorker && enemyBase != null)
                        {
                            DebugSystem.Log($"Switching target to enemy base at ({enemyBase.X}, {enemyBase.Y}) with {enemyBase.HitPoints} HP");
                            currentTarget = enemyBase;
                            attackingWorker = false;
                            actionExecutor.SetAttackTarget(attacker, enemyBase.X, enemyBase.Y);
                            previousTargetHP = enemyBase.HitPoints;
                        }
                        else
                        {
                            DebugSystem.LogSuccess("Target destroyed or moved away. Test complete.");
                            yield break;
                        }
                    }
                }
                else if (actualTarget.HitPoints <= 0)
                {
                    if (!targetDestroyed)
                    {
                        targetDestroyed = true;
                        DebugSystem.LogSuccess($"{currentTarget.Type.name} at ({currentTarget.X}, {currentTarget.Y}) was destroyed after {successfulAttacks} attacks");

                        if (attackingWorker && enemyBase != null)
                        {
                            DebugSystem.Log($"Switching target to enemy base at ({enemyBase.X}, {enemyBase.Y}) with {enemyBase.HitPoints} HP");
                            currentTarget = enemyBase;
                            attackingWorker = false;
                            actionExecutor.SetAttackTarget(attacker, enemyBase.X, enemyBase.Y);
                            targetDestroyed = false;
                            previousTargetHP = enemyBase.HitPoints;
                        }
                        else
                        {
                            DebugSystem.LogSuccess("All enemy targets destroyed. Test complete.");
                            yield break;
                        }
                    }
                }
                else
                {
                    int currentTargetHP = actualTarget.HitPoints;

                    if (currentX != previousX || currentY != previousY)
                    {
                        DebugSystem.Log($"Attacker moved: ({previousX}, {previousY}) -> ({currentX}, {currentY})");
                        previousX = currentX;
                        previousY = currentY;
                    }

                    if (currentTargetHP < previousTargetHP)
                    {
                        int damage = previousTargetHP - currentTargetHP;
                        successfulAttacks++;
                        DebugSystem.LogSuccess($"Attack {successfulAttacks}: Dealt {damage} damage to {actualTarget.Type.name} at ({actualTarget.X}, {actualTarget.Y}). HP: {previousTargetHP} -> {currentTargetHP}");
                        previousTargetHP = currentTargetHP;
                    }
                }

                yield return new WaitForFixedUpdate();
            }
        }

        private bool FindRandomWalkablePosition(out int x, out int y)
        {
            x = 0;
            y = 0;

            if (environmentController == null) return false;
            if (util == null || util.Rnd == null)
            {
                DebugSystem.LogError("Util component or seeded random not available");
                return false;
            }

            List<(int x, int y)> walkablePositions = new List<(int x, int y)>();

            for (int tx = 0; tx < environmentController.MapWidth; tx++)
            {
                for (int ty = 0; ty < environmentController.MapHeight; ty++)
                {
                    if (environmentController.IsWalkable(tx, ty))
                    {
                        Unit unitAtPos = environmentController.GetUnitAt(tx, ty);
                        if (unitAtPos == null)
                        {
                            walkablePositions.Add((tx, ty));
                        }
                    }
                }
            }

            if (walkablePositions.Count == 0) return false;

            var selected = walkablePositions[util.Rnd.Next(walkablePositions.Count)];
            x = selected.x;
            y = selected.y;
            return true;
        }

        private IEnumerator TestWorkerBuildBarracks()
        {
            Unit worker = FindTeam0Worker();
            if (worker == null)
            {
                DebugSystem.LogError("Team 0 worker not found");
                yield break;
            }

            if (actionExecutor == null)
            {
                DebugSystem.LogError("Action executor not found");
                yield break;
            }

            if (environmentController == null)
            {
                DebugSystem.LogError("Environment controller not found");
                yield break;
            }

            var player = environmentController.GetPlayer(worker.Player);
            if (player == null)
            {
                DebugSystem.LogError("Player not found");
                yield break;
            }

            var barracksType = environmentController.UnitTypeTable.GetUnitType("Barracks");
            if (barracksType == null)
            {
                DebugSystem.LogError("Barracks unit type not found");
                yield break;
            }

            if (util == null || util.Rnd == null)
            {
                DebugSystem.LogError("Util component or seeded random not available");
                yield break;
            }

            DebugSystem.Log($"Worker at ({worker.X}, {worker.Y}), starting Barracks build test");
            DebugSystem.Log($"Player has {player.Resources} resources, Barracks costs {barracksType.cost}");

            if (player.Resources < barracksType.cost)
            {
                DebugSystem.LogWarning($"Insufficient resources to build Barracks. Need {barracksType.cost}, have {player.Resources}");
                yield break;
            }

            if (!FindRandomWalkablePosition(out int targetX, out int targetY))
            {
                DebugSystem.LogError("Could not find random walkable position");
                yield break;
            }

            DebugSystem.Log($"Target build position: ({targetX}, {targetY})");

            int adjacentX = targetX;
            int adjacentY = targetY;
            bool foundAdjacent = false;

            int[] dxs = { 0, 1, 0, -1 };
            int[] dys = { -1, 0, 1, 0 };

            for (int i = 0; i < 4; i++)
            {
                int ax = targetX + dxs[i];
                int ay = targetY + dys[i];
                if (ax >= 0 && ax < environmentController.MapWidth && ay >= 0 && ay < environmentController.MapHeight)
                {
                    if (environmentController.IsWalkable(ax, ay))
                    {
                        Unit unitAtPos = environmentController.GetUnitAt(ax, ay);
                        if (unitAtPos == null || unitAtPos.ID == worker.ID)
                        {
                            adjacentX = ax;
                            adjacentY = ay;
                            foundAdjacent = true;
                            break;
                        }
                    }
                }
            }

            if (!foundAdjacent)
            {
                DebugSystem.LogWarning($"Could not find adjacent position to ({targetX}, {targetY})");
                yield break;
            }

            DebugSystem.Log($"Moving worker to adjacent position ({adjacentX}, {adjacentY})");

            while (worker.X != adjacentX || worker.Y != adjacentY)
            {
                if (actionExecutor.HasPendingAction(worker))
                {
                    actionExecutor.ProcessActions();
                    yield return new WaitForFixedUpdate();
                    continue;
                }

                if (actionExecutor.GetNextStepTowardTarget(worker, adjacentX, adjacentY, out int stepX, out int stepY))
                {
                    int direction = GetDirectionToTarget(worker.X, worker.Y, stepX, stepY);
                    if (direction != MicroRTSUtils.DIRECTION_NONE && CanMoveTo(worker, stepX, stepY))
                    {
                        ScheduleMovement(worker, direction);
                    }
                }

                actionExecutor.ProcessActions();
                yield return new WaitForFixedUpdate();
            }

            DebugSystem.LogSuccess($"Worker reached position ({worker.X}, {worker.Y}), starting Barracks build");
            DebugSystem.Log($"Worker at ({worker.X}, {worker.Y}), target build position: ({targetX}, {targetY}), adjacent: {IsAdjacent(worker.X, worker.Y, targetX, targetY)}");

            if (!IsAdjacent(worker.X, worker.Y, targetX, targetY))
            {
                DebugSystem.LogError($"Worker is not adjacent to target position! Worker: ({worker.X}, {worker.Y}), Target: ({targetX}, {targetY})");
                yield break;
            }

            Unit unitAtTarget = environmentController.GetUnitAt(targetX, targetY);
            if (unitAtTarget != null)
            {
                DebugSystem.LogWarning($"Target position ({targetX}, {targetY}) is occupied by {unitAtTarget.Type.name}");
                yield break;
            }

            if (!environmentController.IsWalkable(targetX, targetY))
            {
                DebugSystem.LogWarning($"Target position ({targetX}, {targetY}) is not walkable");
                yield break;
            }

            int initialBarracksCount = environmentController.GetAllUnits().Count(u => u.Type.name == "Barracks" && u.Player == worker.Player);
            int buildStartCycle = 0;
            float buildStartTime = Time.time;
            float maxBuildTime = 30f;

            if (!actionExecutor.ScheduleBuildAtPosition(worker, targetX, targetY, barracksType))
            {
                DebugSystem.LogError($"Failed to schedule Barracks build at ({targetX}, {targetY})");
                yield break;
            }

            buildStartCycle = actionExecutor.GetCurrentCycle();
            DebugSystem.Log($"Barracks production scheduled! Will take {barracksType.produceTime} cycles");

            while (Time.time - buildStartTime < maxBuildTime)
            {
                if (actionExecutor.HasPendingAction(worker))
                {
                    actionExecutor.ProcessActions();
                    yield return new WaitForFixedUpdate();
                    continue;
                }

                int currentBarracksCount = environmentController.GetAllUnits().Count(u => u.Type.name == "Barracks" && u.Player == worker.Player);
                if (currentBarracksCount > initialBarracksCount)
                {
                    Unit newBarracks = environmentController.GetAllUnits().FirstOrDefault(u => u.Type.name == "Barracks" && u.Player == worker.Player && u.X == targetX && u.Y == targetY);
                    if (newBarracks == null)
                    {
                        newBarracks = environmentController.GetAllUnits().FirstOrDefault(u => u.Type.name == "Barracks" && u.Player == worker.Player);
                    }

                    if (newBarracks != null)
                    {
                        int cyclesElapsed = actionExecutor.GetCurrentCycle() - buildStartCycle;
                        DebugSystem.LogSuccess($"Barracks built at ({newBarracks.X}, {newBarracks.Y}) after {cyclesElapsed} cycles! Player now has {player.Resources} resources");
                        yield break;
                    }
                }

                actionExecutor.ProcessActions();
                yield return new WaitForFixedUpdate();
            }

            DebugSystem.LogWarning($"Barracks build test timed out after {maxBuildTime} seconds");
        }

        private IEnumerator TestWorkerBuildBase()
        {
            Unit worker = FindTeam0Worker();
            if (worker == null)
            {
                DebugSystem.LogError("Team 0 worker not found");
                yield break;
            }

            if (actionExecutor == null)
            {
                DebugSystem.LogError("Action executor not found");
                yield break;
            }

            if (environmentController == null)
            {
                DebugSystem.LogError("Environment controller not found");
                yield break;
            }

            var player = environmentController.GetPlayer(worker.Player);
            if (player == null)
            {
                DebugSystem.LogError("Player not found");
                yield break;
            }

            var baseType = environmentController.UnitTypeTable.GetUnitType("Base");
            if (baseType == null)
            {
                DebugSystem.LogError("Base unit type not found");
                yield break;
            }

            if (util == null || util.Rnd == null)
            {
                DebugSystem.LogError("Util component or seeded random not available");
                yield break;
            }

            DebugSystem.Log($"Worker at ({worker.X}, {worker.Y}), starting Base build test");
            DebugSystem.Log($"Player has {player.Resources} resources, Base costs {baseType.cost}");

            if (player.Resources < baseType.cost)
            {
                DebugSystem.LogWarning($"Insufficient resources to build Base. Need {baseType.cost}, have {player.Resources}");
                yield break;
            }

            if (!FindRandomWalkablePosition(out int targetX, out int targetY))
            {
                DebugSystem.LogError("Could not find random walkable position");
                yield break;
            }

            DebugSystem.Log($"Target build position: ({targetX}, {targetY})");

            int adjacentX = targetX;
            int adjacentY = targetY;
            bool foundAdjacent = false;

            int[] dxs = { 0, 1, 0, -1 };
            int[] dys = { -1, 0, 1, 0 };

            for (int i = 0; i < 4; i++)
            {
                int ax = targetX + dxs[i];
                int ay = targetY + dys[i];
                if (ax >= 0 && ax < environmentController.MapWidth && ay >= 0 && ay < environmentController.MapHeight)
                {
                    if (environmentController.IsWalkable(ax, ay))
                    {
                        Unit unitAtPos = environmentController.GetUnitAt(ax, ay);
                        if (unitAtPos == null || unitAtPos.ID == worker.ID)
                        {
                            adjacentX = ax;
                            adjacentY = ay;
                            foundAdjacent = true;
                            break;
                        }
                    }
                }
            }

            if (!foundAdjacent)
            {
                DebugSystem.LogWarning($"Could not find adjacent position to ({targetX}, {targetY})");
                yield break;
            }

            DebugSystem.Log($"Moving worker to adjacent position ({adjacentX}, {adjacentY})");

            while (worker.X != adjacentX || worker.Y != adjacentY)
            {
                if (actionExecutor.HasPendingAction(worker))
                {
                    actionExecutor.ProcessActions();
                    yield return new WaitForFixedUpdate();
                    continue;
                }

                if (actionExecutor.GetNextStepTowardTarget(worker, adjacentX, adjacentY, out int stepX, out int stepY))
                {
                    int direction = GetDirectionToTarget(worker.X, worker.Y, stepX, stepY);
                    if (direction != MicroRTSUtils.DIRECTION_NONE && CanMoveTo(worker, stepX, stepY))
                    {
                        ScheduleMovement(worker, direction);
                    }
                }

                actionExecutor.ProcessActions();
                yield return new WaitForFixedUpdate();
            }

            DebugSystem.LogSuccess($"Worker reached position ({worker.X}, {worker.Y}), starting Base build");
            DebugSystem.Log($"Worker at ({worker.X}, {worker.Y}), target build position: ({targetX}, {targetY}), adjacent: {IsAdjacent(worker.X, worker.Y, targetX, targetY)}");

            if (!IsAdjacent(worker.X, worker.Y, targetX, targetY))
            {
                DebugSystem.LogError($"Worker is not adjacent to target position! Worker: ({worker.X}, {worker.Y}), Target: ({targetX}, {targetY})");
                yield break;
            }

            Unit unitAtTarget = environmentController.GetUnitAt(targetX, targetY);
            if (unitAtTarget != null)
            {
                DebugSystem.LogWarning($"Target position ({targetX}, {targetY}) is occupied by {unitAtTarget.Type.name}");
                yield break;
            }

            if (!environmentController.IsWalkable(targetX, targetY))
            {
                DebugSystem.LogWarning($"Target position ({targetX}, {targetY}) is not walkable");
                yield break;
            }

            int initialBaseCount = environmentController.GetAllUnits().Count(u => u.Type.name == "Base" && u.Player == worker.Player);
            int buildStartCycle = 0;
            float buildStartTime = Time.time;
            float maxBuildTime = 30f;

            if (!actionExecutor.ScheduleBuildAtPosition(worker, targetX, targetY, baseType))
            {
                DebugSystem.LogError($"Failed to schedule Base build at ({targetX}, {targetY})");
                yield break;
            }

            buildStartCycle = actionExecutor.GetCurrentCycle();
            DebugSystem.Log($"Base production scheduled! Will take {baseType.produceTime} cycles");

            while (Time.time - buildStartTime < maxBuildTime)
            {
                if (actionExecutor.HasPendingAction(worker))
                {
                    actionExecutor.ProcessActions();
                    yield return new WaitForFixedUpdate();
                    continue;
                }

                int currentBaseCount = environmentController.GetAllUnits().Count(u => u.Type.name == "Base" && u.Player == worker.Player);
                if (currentBaseCount > initialBaseCount)
                {
                    Unit newBase = environmentController.GetAllUnits().FirstOrDefault(u => u.Type.name == "Base" && u.Player == worker.Player && u.X == targetX && u.Y == targetY);
                    if (newBase == null)
                    {
                        newBase = environmentController.GetAllUnits().FirstOrDefault(u => u.Type.name == "Base" && u.Player == worker.Player);
                    }

                    if (newBase != null)
                    {
                        int cyclesElapsed = actionExecutor.GetCurrentCycle() - buildStartCycle;
                        DebugSystem.LogSuccess($"Base built at ({newBase.X}, {newBase.Y}) after {cyclesElapsed} cycles! Player now has {player.Resources} resources");
                        yield break;
                    }
                }

                actionExecutor.ProcessActions();
                yield return new WaitForFixedUpdate();
            }

            DebugSystem.LogWarning($"Base build test timed out after {maxBuildTime} seconds");
        }
    }
}

