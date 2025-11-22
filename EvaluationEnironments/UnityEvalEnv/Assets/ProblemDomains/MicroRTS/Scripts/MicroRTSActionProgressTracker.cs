using UnityEngine;
using System.Collections.Generic;
using Problems.MicroRTS.Core;

namespace Problems.MicroRTS
{
    public class MicroRTSActionProgressTracker : MonoBehaviour
    {
        private MicroRTSActionExecutor actionExecutor;
        private MicroRTSEnvironmentController environmentController;

        private Dictionary<Unit, MicroRTSProgressBar> unitProgressBars = new Dictionary<Unit, MicroRTSProgressBar>();
        private Dictionary<(int x, int y), MicroRTSProgressBar> spawnLocationBars = new Dictionary<(int, int), MicroRTSProgressBar>();

        void Awake()
        {
            actionExecutor = GetComponent<MicroRTSActionExecutor>();
            environmentController = GetComponentInParent<MicroRTSEnvironmentController>();
        }

        void Update()
        {
            UpdateUnitActionProgress();
            UpdateProductionProgress();
        }

        private void UpdateUnitActionProgress()
        {
            if (actionExecutor == null || environmentController == null) return;

            var toRemove = new List<Unit>();

            foreach (var kvp in unitProgressBars)
            {
                Unit unit = kvp.Key;
                MicroRTSProgressBar progressBar = kvp.Value;

                if (unit == null || unit.HitPoints <= 0 || progressBar == null)
                {
                    toRemove.Add(unit);
                    continue;
                }

                if (actionExecutor.HasPendingAction(unit))
                {
                    float progress = CalculateActionProgress(unit);
                    if (progress >= 0f && progress <= 1f)
                    {
                        GameObject unitObj = environmentController.GetUnitGameObject(unit.ID);
                        if (unitObj != null)
                        {
                            progressBar.SetPosition(unitObj.transform.position);
                            progressBar.SetProgress(progress);
                            progressBar.SetVisible(true);
                        }
                    }
                }
                else
                {
                    toRemove.Add(unit);
                }
            }

            foreach (var unit in toRemove)
            {
                RemoveUnitProgressBar(unit);
            }
        }

        private void UpdateProductionProgress()
        {
            if (actionExecutor == null || environmentController == null) return;

            var producingUnits = actionExecutor.GetProducingUnits();
            var activeSpawnLocations = new HashSet<(int x, int y)>();

            foreach (var kvp in producingUnits)
            {
                Unit producer = kvp.Key;
                (int spawnX, int spawnY, UnitType unitType) = kvp.Value;

                if (producer == null || producer.HitPoints <= 0)
                    continue;

                activeSpawnLocations.Add((spawnX, spawnY));

                if (!spawnLocationBars.ContainsKey((spawnX, spawnY)))
                {
                    CreateSpawnLocationBar(spawnX, spawnY);
                }

                if (spawnLocationBars.TryGetValue((spawnX, spawnY), out MicroRTSProgressBar bar))
                {
                    float progress = CalculateProductionProgress(producer);
                    if (progress >= 0f && progress <= 1f)
                    {
                        Vector3 worldPos = environmentController.GridToWorldPosition(spawnX, spawnY);
                        bar.SetPosition(worldPos);
                        bar.SetProgress(progress);
                        bar.SetVisible(true);
                    }
                }
            }

            var toRemove = new List<(int x, int y)>();
            foreach (var kvp in spawnLocationBars)
            {
                if (!activeSpawnLocations.Contains(kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var location in toRemove)
            {
                RemoveSpawnLocationBar(location);
            }
        }

        private float CalculateActionProgress(Unit unit)
        {
            if (actionExecutor == null) return 0f;

            var pendingActions = actionExecutor.GetPendingActions();
            if (!pendingActions.TryGetValue(unit, out MicroRTSActionAssignment assignment))
                return 0f;

            if (assignment == null) return 0f;

            int currentCycle = actionExecutor.GetCurrentCycle();
            int assignmentTime = assignment.assignmentTime;
            int eta = assignment.GetETA();

            if (eta <= 0) return 0f;

            float progress = (float)(currentCycle - assignmentTime) / eta;
            return Mathf.Clamp01(progress);
        }

        private float CalculateProductionProgress(Unit producer)
        {
            if (actionExecutor == null) return 0f;

            var pendingActions = actionExecutor.GetPendingActions();
            if (!pendingActions.TryGetValue(producer, out MicroRTSActionAssignment assignment))
                return 0f;

            if (assignment == null || assignment.actionType != MicroRTSActionAssignment.ACTION_TYPE_PRODUCE)
                return 0f;

            int currentCycle = actionExecutor.GetCurrentCycle();
            int assignmentTime = assignment.assignmentTime;
            int eta = assignment.GetETA();

            if (eta <= 0) return 0f;

            float progress = (float)(currentCycle - assignmentTime) / eta;
            return Mathf.Clamp01(progress);
        }

        public void OnActionScheduled(Unit unit, MicroRTSActionAssignment assignment)
        {
            if (unit == null || assignment == null || environmentController == null) return;

            if (assignment.actionType == MicroRTSActionAssignment.ACTION_TYPE_PRODUCE)
            {
                return;
            }

            if (!unitProgressBars.ContainsKey(unit))
            {
                CreateUnitProgressBar(unit);
            }
        }

        public void OnActionCompleted(Unit unit)
        {
            if (unit != null)
            {
                RemoveUnitProgressBar(unit);
            }
        }

        public void OnProductionScheduled(Unit producer, int spawnX, int spawnY)
        {
            if (environmentController == null) return;

            if (!spawnLocationBars.ContainsKey((spawnX, spawnY)))
            {
                CreateSpawnLocationBar(spawnX, spawnY);
            }
        }

        private void CreateUnitProgressBar(Unit unit)
        {
            if (environmentController == null) return;

            GameObject unitObj = environmentController.GetUnitGameObject(unit.ID);
            if (unitObj == null) return;

            GameObject barObj = new GameObject($"ProgressBar_Unit_{unit.ID}");
            barObj.transform.SetParent(unitObj.transform, false);
            barObj.transform.localPosition = Vector3.zero;

            MicroRTSProgressBar progressBar = barObj.AddComponent<MicroRTSProgressBar>();
            unitProgressBars[unit] = progressBar;
        }

        private void CreateSpawnLocationBar(int spawnX, int spawnY)
        {
            if (environmentController == null) return;

            Vector3 worldPos = environmentController.GridToWorldPosition(spawnX, spawnY);

            GameObject barObj = new GameObject($"ProgressBar_Spawn_{spawnX}_{spawnY}");
            barObj.transform.position = worldPos;

            MicroRTSProgressBar progressBar = barObj.AddComponent<MicroRTSProgressBar>();
            spawnLocationBars[(spawnX, spawnY)] = progressBar;
        }

        private void RemoveUnitProgressBar(Unit unit)
        {
            if (unitProgressBars.TryGetValue(unit, out MicroRTSProgressBar bar))
            {
                if (bar != null)
                {
                    bar.SetVisible(false);
                    Destroy(bar.gameObject);
                }
                unitProgressBars.Remove(unit);
            }
        }

        private void RemoveSpawnLocationBar((int x, int y) location)
        {
            if (spawnLocationBars.TryGetValue(location, out MicroRTSProgressBar bar))
            {
                if (bar != null)
                {
                    bar.SetVisible(false);
                    Destroy(bar.gameObject);
                }
                spawnLocationBars.Remove(location);
            }
        }

        void OnDestroy()
        {
            foreach (var bar in unitProgressBars.Values)
            {
                if (bar != null) Destroy(bar.gameObject);
            }
            unitProgressBars.Clear();

            foreach (var bar in spawnLocationBars.Values)
            {
                if (bar != null) Destroy(bar.gameObject);
            }
            spawnLocationBars.Clear();
        }
    }
}

