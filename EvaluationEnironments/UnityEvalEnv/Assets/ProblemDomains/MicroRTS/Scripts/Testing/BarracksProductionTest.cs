using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Problems.MicroRTS.Core;
using Problems.MicroRTS;
using Utils;
using AgentControllers;
using Base;

namespace Problems.MicroRTS.Testing
{
    public class BarracksProductionTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool autoStartTest = true;
        [SerializeField] private float testStartDelay = 0.5f;
        [SerializeField] private float barracksCheckInterval = 3.0f;
        [SerializeField] private float maxWaitTime = 60f;
        [SerializeField] private int targetTeam = 0;

        private MicroRTSEnvironmentController environmentController;
        private MicroRTSActionExecutor actionExecutor;
        private bool testRunning = false;
        private bool hasStarted = false;
        private Coroutine testCoroutine = null;

        void Awake()
        {
            DebugSystem.Log("BarracksProductionTest: Awake() called");
        }

        void OnEnable()
        {
            DebugSystem.Log($"BarracksProductionTest: OnEnable() called. autoStartTest={autoStartTest}, hasStarted={hasStarted}");
        }

        void Start()
        {
            DebugSystem.Log($"BarracksProductionTest: Start() called. autoStartTest={autoStartTest}, hasStarted={hasStarted}, testRunning={testRunning}");
            if (autoStartTest && !testRunning)
            {
                if (testCoroutine != null)
                {
                    StopCoroutine(testCoroutine);
                }
                DebugSystem.Log("BarracksProductionTest: Starting test from Start()");
                testCoroutine = StartCoroutine(StartTestAfterDelay());
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && !testRunning)
            {
                StartCoroutine(TestBarracksProduction());
            }
        }

        public void StartTestManually()
        {
            if (!testRunning)
            {
                if (testCoroutine != null)
                {
                    StopCoroutine(testCoroutine);
                }
                testCoroutine = StartCoroutine(TestBarracksProduction());
            }
        }

        private IEnumerator StartTestAfterDelay()
        {
            DebugSystem.Log($"BarracksProductionTest: StartTestAfterDelay() - waiting {testStartDelay} seconds before starting test");
            yield return new WaitForSeconds(testStartDelay);
            DebugSystem.Log("BarracksProductionTest: Delay complete, starting TestBarracksProduction()");
            testCoroutine = StartCoroutine(TestBarracksProduction());
            yield return testCoroutine;
        }

        private IEnumerator TestBarracksProduction()
        {
            if (testRunning)
            {
                DebugSystem.LogWarning("Barracks production test is already running");
                yield break;
            }
            testRunning = true;

            DebugSystem.Log("BarracksProductionTest: Starting test...");

            environmentController = FindFirstObjectByType<MicroRTSEnvironmentController>();
            if (environmentController == null)
            {
                DebugSystem.LogError("BarracksProductionTest: Environment controller not found");
                testRunning = false;
                yield break;
            }

            actionExecutor = environmentController.GetComponent<MicroRTSActionExecutor>();
            if (actionExecutor == null)
            {
                DebugSystem.LogError("BarracksProductionTest: Action executor not found");
                testRunning = false;
                yield break;
            }

            DebugSystem.Log($"BarracksProductionTest: Starting barracks production test. Waiting for barracks to spawn for team {targetTeam}...");

            Unit barracksUnit = null;
            float elapsedTime = 0f;

            while (barracksUnit == null && elapsedTime < maxWaitTime)
            {
                var allUnits = environmentController.GetAllUnits();
                DebugSystem.Log($"BarracksProductionTest: Checking for barracks. Total units: {allUnits.Count}");

                foreach (var unit in allUnits)
                {
                    DebugSystem.Log($"  Unit: {unit.Type.name} at ({unit.X}, {unit.Y}), Player: {unit.Player}");
                }

                barracksUnit = allUnits.FirstOrDefault(u => u.Type.name == "Barracks" && u.Player == targetTeam);

                if (barracksUnit == null)
                {
                    DebugSystem.Log($"BarracksProductionTest: No barracks found yet. Checking again in {barracksCheckInterval} seconds... (elapsed: {elapsedTime}s)");
                    yield return new WaitForSeconds(barracksCheckInterval);
                    elapsedTime += barracksCheckInterval;
                }
            }

            if (barracksUnit == null)
            {
                DebugSystem.LogError($"No barracks found for team {targetTeam} after {maxWaitTime} seconds");
                testRunning = false;
                yield break;
            }

            DebugSystem.LogSuccess($"Barracks found at ({barracksUnit.X}, {barracksUnit.Y}) for team {barracksUnit.Player}");

            var player = environmentController.GetPlayer(barracksUnit.Player);
            if (player == null)
            {
                DebugSystem.LogError($"Player {barracksUnit.Player} not found");
                testRunning = false;
                yield break;
            }

            DebugSystem.Log($"Player has {player.Resources} resources");

            if (environmentController.Agents == null || environmentController.Agents.Length == 0)
            {
                DebugSystem.LogError("No agents found in environment controller");
                testRunning = false;
                yield break;
            }

            var agentComponent = environmentController.Agents.FirstOrDefault(a => a != null && a.TeamIdentifier != null && a.TeamIdentifier.TeamID == barracksUnit.Player);
            if (agentComponent == null)
            {
                DebugSystem.LogError($"No agent component found for team {barracksUnit.Player}");
                testRunning = false;
                yield break;
            }

            if (agentComponent.ActionBuffer == null)
            {
                agentComponent.ActionBuffer = new ActionBuffer();
            }

            var lightType = environmentController.UnitTypeTable.GetUnitType("Light");
            var heavyType = environmentController.UnitTypeTable.GetUnitType("Heavy");
            var rangedType = environmentController.UnitTypeTable.GetUnitType("Ranged");

            if (lightType == null || heavyType == null || rangedType == null)
            {
                DebugSystem.LogError("Unit types not found");
                testRunning = false;
                yield break;
            }

            int totalCost = lightType.cost + heavyType.cost + rangedType.cost;
            if (player.Resources < totalCost)
            {
                DebugSystem.LogWarning($"Insufficient resources. Need {totalCost}, have {player.Resources}");
                testRunning = false;
                yield break;
            }

            int bottomRow = environmentController.MapHeight - 1;
            int startX = 0;
            int lightX = startX;      // Position 0
            int rangedX = startX + 1; // Position 1
            int heavyX = startX + 2;  // Position 2

            DebugSystem.Log($"Will spawn units in bottom row: Light at ({lightX}, {bottomRow}), Ranged at ({rangedX}, {bottomRow}), Heavy at ({heavyX}, {bottomRow})");
            DebugSystem.Log($"Barracks is at ({barracksUnit.X}, {barracksUnit.Y})");

            var initialLightUnits = environmentController.GetAllUnits().Where(u => u.Type.name == "Light" && u.Player == barracksUnit.Player).Select(u => u.ID).ToHashSet();
            var initialHeavyUnits = environmentController.GetAllUnits().Where(u => u.Type.name == "Heavy" && u.Player == barracksUnit.Player).Select(u => u.ID).ToHashSet();
            var initialRangedUnits = environmentController.GetAllUnits().Where(u => u.Type.name == "Ranged" && u.Player == barracksUnit.Player).Select(u => u.ID).ToHashSet();

            yield return StartCoroutine(SpawnAndMoveUnit(barracksUnit, lightType, lightX, bottomRow, agentComponent, "Light", initialLightUnits));
            yield return StartCoroutine(SpawnAndMoveUnit(barracksUnit, rangedType, rangedX, bottomRow, agentComponent, "Ranged", initialRangedUnits));
            yield return StartCoroutine(SpawnAndMoveUnit(barracksUnit, heavyType, heavyX, bottomRow, agentComponent, "Heavy", initialHeavyUnits));

            var finalLightUnits = environmentController.GetAllUnits().Where(u => u.Type.name == "Light" && u.Player == barracksUnit.Player).Select(u => u.ID).ToHashSet();
            var finalHeavyUnits = environmentController.GetAllUnits().Where(u => u.Type.name == "Heavy" && u.Player == barracksUnit.Player).Select(u => u.ID).ToHashSet();
            var finalRangedUnits = environmentController.GetAllUnits().Where(u => u.Type.name == "Ranged" && u.Player == barracksUnit.Player).Select(u => u.ID).ToHashSet();

            bool lightSpawned = finalLightUnits.Except(initialLightUnits).Any();
            bool heavySpawned = finalHeavyUnits.Except(initialHeavyUnits).Any();
            bool rangedSpawned = finalRangedUnits.Except(initialRangedUnits).Any();

            int finalLightCount = finalLightUnits.Count;
            int finalHeavyCount = finalHeavyUnits.Count;
            int finalRangedCount = finalRangedUnits.Count;

            if (lightSpawned && heavySpawned && rangedSpawned)
            {
                DebugSystem.LogSuccess($"All units spawned successfully! Light: {finalLightCount}, Heavy: {finalHeavyCount}, Ranged: {finalRangedCount}");
            }
            else
            {
                DebugSystem.LogWarning($"Some units failed to spawn. Light: {lightSpawned} ({finalLightCount}), Heavy: {heavySpawned} ({finalHeavyCount}), Ranged: {rangedSpawned} ({finalRangedCount})");
            }

            testRunning = false;
        }

        private IEnumerator SpawnAndMoveUnit(Unit producer, UnitType unitType, int targetX, int targetY, AgentComponent agentComponent, string unitTypeName, HashSet<long> initialUnitIds)
        {
            DebugSystem.Log($"Starting to spawn {unitTypeName}, will move to position ({targetX}, {targetY})");

            float spawnStartTime = Time.time;
            float maxSpawnTime = 30f;
            Unit spawnedUnit = null;

            while (Time.time - spawnStartTime < maxSpawnTime)
            {
                if (actionExecutor.HasPendingAction(producer))
                {
                    actionExecutor.ProcessActions();
                    yield return new WaitForFixedUpdate();
                    continue;
                }

                var newUnits = environmentController.GetAllUnits()
                    .Where(u => u.Type.name == unitTypeName && u.Player == producer.Player && !initialUnitIds.Contains(u.ID))
                    .ToList();

                if (newUnits.Count > 0)
                {
                    spawnedUnit = newUnits.First();

                    if (spawnedUnit != null)
                    {
                        DebugSystem.LogSuccess($"{unitTypeName} spawned at ({spawnedUnit.X}, {spawnedUnit.Y}), ID={spawnedUnit.ID}, canMove={spawnedUnit.Type.canMove}, moving to ({targetX}, {targetY})");

                        if (!spawnedUnit.Type.canMove)
                        {
                            DebugSystem.LogError($"{unitTypeName} cannot move! Unit type canMove is false.");
                            yield break;
                        }

                        yield return StartCoroutine(MoveUnitToPosition(spawnedUnit, targetX, targetY, agentComponent));
                        yield break;
                    }
                }

                string produceActionName = $"produce_{unitTypeName}_unit{producer.ID}";
                agentComponent.ActionBuffer.AddDiscreteAction(produceActionName, 1);

                actionExecutor.ExecuteActions(agentComponent);
                actionExecutor.ProcessActions();

                yield return new WaitForFixedUpdate();
            }

            DebugSystem.LogWarning($"{unitTypeName} spawn timed out after {maxSpawnTime} seconds");
        }

        private IEnumerator MoveUnitToPosition(Unit unit, int targetX, int targetY, AgentComponent agentComponent)
        {
            if (unit == null)
            {
                DebugSystem.LogError("MoveUnitToPosition: unit is null");
                yield break;
            }

            if (!unit.Type.canMove)
            {
                DebugSystem.LogError($"MoveUnitToPosition: {unit.Type.name} cannot move (canMove=false)");
                yield break;
            }

            DebugSystem.Log($"Moving {unit.Type.name} (ID: {unit.ID}) from ({unit.X}, {unit.Y}) to ({targetX}, {targetY})");

            float moveStartTime = Time.time;
            float maxMoveTime = 30f;
            int moveAttempts = 0;

            while ((unit.X != targetX || unit.Y != targetY) && Time.time - moveStartTime < maxMoveTime)
            {
                if (actionExecutor.HasPendingAction(unit))
                {
                    actionExecutor.ProcessActions();
                    yield return new WaitForFixedUpdate();
                    continue;
                }

                if (actionExecutor.GetNextStepTowardTarget(unit, targetX, targetY, out int stepX, out int stepY))
                {
                    int direction = GetDirectionToTarget(unit.X, unit.Y, stepX, stepY);
                    if (direction != MicroRTSUtils.DIRECTION_NONE && CanMoveTo(unit, stepX, stepY))
                    {
                        string moveActionName = $"moveDirection_unit{unit.ID}";
                        agentComponent.ActionBuffer.AddDiscreteAction(moveActionName, direction);
                        moveAttempts++;

                        if (moveAttempts % 10 == 0)
                        {
                            DebugSystem.Log($"{unit.Type.name} at ({unit.X}, {unit.Y}) attempting move {moveAttempts}, direction={direction}, step=({stepX}, {stepY})");
                        }
                    }
                    else
                    {
                        if (moveAttempts % 20 == 0)
                        {
                            DebugSystem.Log($"{unit.Type.name} at ({unit.X}, {unit.Y}) cannot move: direction={direction}, canMoveTo={CanMoveTo(unit, stepX, stepY)}");
                        }
                    }
                }
                else
                {
                    if (moveAttempts == 0 || moveAttempts % 20 == 0)
                    {
                        DebugSystem.Log($"{unit.Type.name} at ({unit.X}, {unit.Y}) GetNextStepTowardTarget returned false. Target: ({targetX}, {targetY}), Map size: {environmentController.MapWidth}x{environmentController.MapHeight}");
                        DebugSystem.Log($"Checking adjacent positions: UP({targetX}, {targetY - 1}), RIGHT({targetX + 1}, {targetY}), DOWN({targetX}, {targetY + 1}), LEFT({targetX - 1}, {targetY})");

                        for (int i = 0; i < 4; i++)
                        {
                            int[] offsets = { 0, 1, 0, -1 };
                            int[] offsetsY = { -1, 0, 1, 0 };
                            int ax = targetX + offsets[i];
                            int ay = targetY + offsetsY[i];
                            bool inBounds = ax >= 0 && ax < environmentController.MapWidth && ay >= 0 && ay < environmentController.MapHeight;
                            bool walkable = inBounds && environmentController.IsWalkable(ax, ay);
                            Unit unitAtPos = inBounds ? environmentController.GetUnitAt(ax, ay) : null;
                            DebugSystem.Log($"  Adjacent {i}: ({ax}, {ay}) - inBounds={inBounds}, walkable={walkable}, unit={unitAtPos?.Type.name ?? "null"}");
                        }
                    }
                }

                actionExecutor.ExecuteActions(agentComponent);
                actionExecutor.ProcessActions();

                yield return new WaitForFixedUpdate();
            }

            if (unit.X == targetX && unit.Y == targetY)
            {
                DebugSystem.LogSuccess($"{unit.Type.name} reached target position ({targetX}, {targetY}) after {moveAttempts} move attempts");
            }
            else
            {
                DebugSystem.LogWarning($"{unit.Type.name} move timed out. Current position: ({unit.X}, {unit.Y}), Target: ({targetX}, {targetY}), Attempts: {moveAttempts}");
            }
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

        private bool CanMoveTo(Unit unit, int x, int y)
        {
            if (environmentController == null) return false;
            if (x < 0 || x >= environmentController.MapWidth || y < 0 || y >= environmentController.MapHeight) return false;
            if (!environmentController.IsWalkable(x, y)) return false;

            Unit unitAtPos = environmentController.GetUnitAt(x, y);
            return unitAtPos == null || unitAtPos.ID == unit.ID;
        }

        private bool IsAdjacent(int x1, int y1, int x2, int y2)
        {
            int dx = Mathf.Abs(x1 - x2);
            int dy = Mathf.Abs(y1 - y2);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }
    }
}

