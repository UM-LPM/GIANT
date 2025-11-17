using System.Collections;
using System.Linq;
using UnityEngine;
using Problems.MicroRTS.Core;
using Problems.MicroRTS;
using Utils;
using AgentControllers;

namespace Problems.MicroRTS.Testing
{
    public class BaseProductionTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool autoStartTest = true;
        [SerializeField] private float testStartDelay = 0.5f;
        [SerializeField] private float maxWaitTime = 5.0f;
        [SerializeField] private float checkInterval = 0.5f;

        [Header("Production Test Configuration")]
        [SerializeField] private bool enableProduction = false;
        [SerializeField] private int workersToSpawn = 1;
        [SerializeField] private int targetTeam = 0;

        private MicroRTSEnvironmentController environmentController;
        private MicroRTSActionExecutor actionExecutor;
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
            if (Input.GetKeyDown(KeyCode.B) && !testRunning)
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

            if (enableProduction)
            {
                yield return StartCoroutine(TestBaseProduction());
            }
            else
            {
                DebugSystem.LogWarning("Base production test is disabled. Enable 'Enable Production' to test worker spawning.");
            }

            testRunning = false;
        }

        private IEnumerator TestBaseProduction()
        {
            DebugSystem.Log($"Starting base production test. Target: {workersToSpawn} workers for team {targetTeam}");

            Unit baseUnit = null;
            foreach (var unit in environmentController.GetAllUnits())
            {
                if (unit.Type.isStockpile && unit.Player == targetTeam)
                {
                    baseUnit = unit;
                    break;
                }
            }

            if (baseUnit == null)
            {
                DebugSystem.LogError($"No base found for team {targetTeam}");
                testRunning = false;
                yield break;
            }

            var player = environmentController.GetPlayer(baseUnit.Player);
            if (player == null)
            {
                DebugSystem.LogError($"Player {baseUnit.Player} not found");
                testRunning = false;
                yield break;
            }

            DebugSystem.Log($"Found base at ({baseUnit.X}, {baseUnit.Y}) for team {baseUnit.Player}. Player has {player.Resources} resources");

            if (environmentController.Agents == null || environmentController.Agents.Length == 0)
            {
                DebugSystem.LogError("No agents found in environment controller");
                testRunning = false;
                yield break;
            }

            var agentComponent = environmentController.Agents.FirstOrDefault(a => a != null && a.TeamIdentifier != null && a.TeamIdentifier.TeamID == baseUnit.Player);
            if (agentComponent == null)
            {
                DebugSystem.LogError($"No agent component found for team {baseUnit.Player}");
                testRunning = false;
                yield break;
            }

            int initialWorkerCount = environmentController.GetAllUnits().Count(u => u.Type.name == "Worker" && u.Player == baseUnit.Player);
            DebugSystem.Log($"Initial worker count for team {baseUnit.Player}: {initialWorkerCount}");

            int workersSpawned = 0;
            float spawnStartTime = Time.time;
            float maxSpawnTime = 60f;

            while (workersSpawned < workersToSpawn && Time.time - spawnStartTime < maxSpawnTime)
            {
                if (actionExecutor.HasPendingAction(baseUnit))
                {
                    yield return new WaitForFixedUpdate();
                    continue;
                }

                var currentWorkerCount = environmentController.GetAllUnits().Count(u => u.Type.name == "Worker" && u.Player == baseUnit.Player);
                if (currentWorkerCount > initialWorkerCount + workersSpawned)
                {
                    workersSpawned++;
                    DebugSystem.LogSuccess($"Worker {workersSpawned}/{workersToSpawn} spawned! Total workers: {currentWorkerCount}");
                    yield return new WaitForFixedUpdate();
                    continue;
                }

                if (agentComponent.ActionBuffer == null)
                {
                    agentComponent.ActionBuffer = new ActionBuffer();
                }

                string produceActionName = $"produce_Worker_unit{baseUnit.ID}";
                agentComponent.ActionBuffer.AddDiscreteAction(produceActionName, 1);

                actionExecutor.ExecuteActions(agentComponent);

                yield return new WaitForFixedUpdate();
            }

            int finalWorkerCount = environmentController.GetAllUnits().Count(u => u.Type.name == "Worker" && u.Player == baseUnit.Player);
            int actualSpawned = finalWorkerCount - initialWorkerCount;

            if (actualSpawned >= workersToSpawn)
            {
                DebugSystem.LogSuccess($"Production test completed! Spawned {actualSpawned} workers (target was {workersToSpawn}). Final worker count: {finalWorkerCount}");
            }
            else if (Time.time - spawnStartTime >= maxSpawnTime)
            {
                DebugSystem.LogWarning($"Production test timed out. Spawned {actualSpawned}/{workersToSpawn} workers in {maxSpawnTime} seconds. Final worker count: {finalWorkerCount}");
            }
            else
            {
                DebugSystem.LogWarning($"Production test incomplete. Spawned {actualSpawned}/{workersToSpawn} workers. Final worker count: {finalWorkerCount}");
            }
        }
    }
}



