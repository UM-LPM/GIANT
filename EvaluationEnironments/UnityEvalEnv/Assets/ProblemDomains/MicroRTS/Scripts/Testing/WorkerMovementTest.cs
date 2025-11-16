using System.Collections;
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
                DebugSystem.LogError("[WorkerMovementTest] MicroRTSEnvironmentController not found!");
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
                DebugSystem.LogSuccess($"[WorkerMovementTest] All units spawned correctly! Resources: {resourceCount}/2, Team 0: {team0WorkerCount}W/{team0BaseCount}B, Team 1: {team1WorkerCount}W/{team1BaseCount}B");
            }
            else
            {
                DebugSystem.LogError($"[WorkerMovementTest] Unit spawn verification FAILED! Resources: {resourceCount}/2, Team 0: {team0WorkerCount}W/{team0BaseCount}B, Team 1: {team1WorkerCount}W/{team1BaseCount}B");
            }

            testRunning = false;
        }
    }
}
