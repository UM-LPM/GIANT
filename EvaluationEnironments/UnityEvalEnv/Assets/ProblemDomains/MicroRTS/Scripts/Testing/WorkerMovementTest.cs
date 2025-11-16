using System.Collections;
using System.Linq;
using UnityEngine;
using Problems.MicroRTS.Core;
using Problems.MicroRTS;
using Utils;

namespace Problems.MicroRTS.Testing
{
    public class WorkerMovementTest : MonoBehaviour
    {
        [Header("Test Configuration")]
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
                yield return StartCoroutine(TestWorkerMovement());
            }
            else
            {
                DebugSystem.LogError($"Unit spawn failed: {resourceCount} resources, Team 0: {team0WorkerCount} worker/{team0BaseCount} base, Team 1: {team1WorkerCount} worker/{team1BaseCount} base");
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

        private void ScheduleMovement(Unit unit, int direction)
        {
            if (actionExecutor == null || environmentController == null) return;

            var assignment = new MicroRTSActionAssignment(unit, MicroRTSActionAssignment.ACTION_TYPE_MOVE, direction, GetCurrentGameTime());
            actionExecutor.ScheduleAction(assignment);
        }

        private int GetCurrentGameTime()
        {
            return actionExecutor.GetCurrentCycle();
        }
    }
}
