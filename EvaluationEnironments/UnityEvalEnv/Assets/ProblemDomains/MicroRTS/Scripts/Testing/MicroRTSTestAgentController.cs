using AgentControllers;
using Problems.MicroRTS.Core;
using Problems.MicroRTS;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MicroRTSTestAgentController", menuName = "AgentControllers/AIAgentControllers/MicroRTSTestAgentController")]
public class MicroRTSTestAgentController : ManualAgentController
{
    private enum TestPhase
    {
        ProduceWorkers,
        HarvestResources,
        BuildBase,
        BuildBarracks,
        ProduceBattleUnits,
        Attack
    }

    private TestPhase currentPhase = TestPhase.ProduceWorkers;
    private Dictionary<long, (int x, int y)> workerResourceAssignments = new();
    private bool allResourcesDepleted = false;
    private long barracksID = -1;
    private List<string> battleUnitTypes = new() { "Light", "Heavy", "Ranged" };
    private int currentBattleUnitIndex = 0;
    private (int x, int y)? baseBuildPosition = null;
    private (int x, int y)? barracksBuildPosition = null;
    private long builderWorkerID = -1;

    public override void GetActions(in ActionBuffer actionsOut)
    {
        MicroRTSEnvironmentController env = Object.FindObjectOfType<MicroRTSEnvironmentController>();
        if (env == null) return;

        MicroRTSActionExecutor executor = env.GetComponent<MicroRTSActionExecutor>();
        if (executor == null) return;

        var myUnits = env.GetAllUnits()
            .Where(u => u.Player == 0 && u.HitPoints > 0)
            .ToList();

        var myBases = myUnits.Where(u => u.Type.name == "Base").ToList();
        var myWorkers = myUnits.Where(u => u.Type.name == "Worker").ToList();
        var myBarracks = myUnits.Where(u => u.Type.name == "Barracks").ToList();
        var resources = env.GetAllUnits()
            .Where(u => u.Type.isResource && u.Resources > 0)
            .ToList();

        if (currentPhase == TestPhase.ProduceWorkers)
        {
            HandleProduceWorkersPhase(myBases, myWorkers, actionsOut);
        }
        else if (currentPhase == TestPhase.HarvestResources)
        {
            HandleHarvestResourcesPhase(env, executor, myWorkers, resources, actionsOut);
        }
        else if (currentPhase == TestPhase.BuildBase)
        {
            HandleBuildBasePhase(env, executor, myBases, myWorkers, actionsOut);
        }
        else if (currentPhase == TestPhase.BuildBarracks)
        {
            HandleBuildBarracksPhase(env, executor, myBases, myWorkers, myBarracks, actionsOut);
        }
        else if (currentPhase == TestPhase.ProduceBattleUnits)
        {
            HandleProduceBattleUnitsPhase(executor, myUnits, myBarracks, actionsOut);
        }
        else if (currentPhase == TestPhase.Attack)
        {
            HandleAttackPhase(env, myUnits, actionsOut);
        }
    }

    // ============================================================================
    // PHASE 1: PRODUCE WORKERS
    // ============================================================================
    // Produce 3 workers from the starting base
    private void HandleProduceWorkersPhase(List<Unit> myBases, List<Unit> myWorkers, ActionBuffer actionsOut)
    {
        if (myBases.Count > 0 && myWorkers.Count < 3)
        {
            var baseUnit = myBases[0];
            string actionName = $"produce_Worker_unit{baseUnit.ID}";
            actionsOut.AddDiscreteAction(actionName, 1);
        }
        else if (myWorkers.Count >= 3)
        {
            currentPhase = TestPhase.HarvestResources;
        }
    }

    // ============================================================================
    // PHASE 2: HARVEST RESOURCES
    // ============================================================================
    // Assign all workers to harvest resources. When all resources are depleted, move to build phase.
    private void HandleHarvestResourcesPhase(MicroRTSEnvironmentController env, MicroRTSActionExecutor executor, List<Unit> myWorkers, List<Unit> resources, ActionBuffer actionsOut)
    {
        // Clear assignments for depleted resources
        var depletedResources = new List<long>();
        foreach (var assignment in workerResourceAssignments)
        {
            var resource = env.GetUnitAt(assignment.Value.x, assignment.Value.y);
            if (resource == null || !resource.Type.isResource || resource.Resources <= 0)
            {
                depletedResources.Add(assignment.Key);
            }
        }
        foreach (var workerID in depletedResources)
        {
            workerResourceAssignments.Remove(workerID);
        }

        // Assign workers to resources - harvest navigation handles movement automatically
        foreach (var worker in myWorkers)
        {
            if (executor.HasPendingAction(worker) && !workerResourceAssignments.ContainsKey(worker.ID))
            {
                continue; // Worker is busy building
            }

            // If worker has resources, keep them in harvest system to return to base
            if (worker.Resources > 0)
            {
                if (workerResourceAssignments.TryGetValue(worker.ID, out var returnPos))
                {
                    actionsOut.AddDiscreteAction($"harvestTargetX_unit{worker.ID}", returnPos.x);
                    actionsOut.AddDiscreteAction($"harvestTargetY_unit{worker.ID}", returnPos.y);
                }
                continue;
            }

            // Check if already assigned to valid resource
            if (workerResourceAssignments.TryGetValue(worker.ID, out var assignedPos))
            {
                var assignedResource = env.GetUnitAt(assignedPos.x, assignedPos.y);
                if (assignedResource != null && assignedResource.Type.isResource && assignedResource.Resources > 0)
                {
                    actionsOut.AddDiscreteAction($"harvestTargetX_unit{worker.ID}", assignedPos.x);
                    actionsOut.AddDiscreteAction($"harvestTargetY_unit{worker.ID}", assignedPos.y);
                    continue;
                }
                else
                {
                    workerResourceAssignments.Remove(worker.ID);
                }
            }

            // Find nearest available resource
            if (resources.Count > 0)
            {
                var nearestResource = resources
                    .OrderBy(r => Mathf.Abs(r.X - worker.X) + Mathf.Abs(r.Y - worker.Y))
                    .FirstOrDefault();

                if (nearestResource != null)
                {
                    workerResourceAssignments[worker.ID] = (nearestResource.X, nearestResource.Y);
                    actionsOut.AddDiscreteAction($"harvestTargetX_unit{worker.ID}", nearestResource.X);
                    actionsOut.AddDiscreteAction($"harvestTargetY_unit{worker.ID}", nearestResource.Y);
                }
            }
        }

        // Transition when all resources depleted
        if (!allResourcesDepleted && resources.Count == 0)
        {
            allResourcesDepleted = true;
            currentPhase = TestPhase.BuildBase;
        }
    }

    // ============================================================================
    // PHASE 3: BUILD BASE
    // ============================================================================
    // Build a base at fixed position (1, 1) using the first worker
    private void HandleBuildBasePhase(MicroRTSEnvironmentController env, MicroRTSActionExecutor executor, List<Unit> myBases, List<Unit> myWorkers, ActionBuffer actionsOut)
    {
        if (myBases.Count == 1)
        {
            var firstWorker = myWorkers.OrderBy(w => w.ID).FirstOrDefault();

            if (firstWorker != null && builderWorkerID == -1)
            {
                builderWorkerID = firstWorker.ID;
            }

            if (firstWorker != null && firstWorker.ID == builderWorkerID)
            {
                if (firstWorker.Resources > 0)
                {
                    var nearestBase = myBases
                        .OrderBy(b => Mathf.Abs(b.X - firstWorker.X) + Mathf.Abs(b.Y - firstWorker.Y))
                        .FirstOrDefault();

                    if (nearestBase != null)
                    {
                        actionsOut.AddDiscreteAction($"harvestTargetX_unit{firstWorker.ID}", nearestBase.X);
                        actionsOut.AddDiscreteAction($"harvestTargetY_unit{firstWorker.ID}", nearestBase.Y);
                    }
                }
                else if (!baseBuildPosition.HasValue)
                {
                    baseBuildPosition = (1, 1);
                }
                else if (baseBuildPosition.HasValue && !executor.HasPendingAction(firstWorker))
                {
                    int buildX = baseBuildPosition.Value.x;
                    int buildY = baseBuildPosition.Value.y;

                    int dx = Mathf.Abs(firstWorker.X - buildX);
                    int dy = Mathf.Abs(firstWorker.Y - buildY);
                    bool isAdjacent = (dx == 1 && dy == 0) || (dx == 0 && dy == 1);

                    if (isAdjacent)
                    {
                        UnitType baseType = env.UnitTypeTable.GetUnitType("Base");
                        if (baseType != null && executor.ScheduleBuildAtPosition(firstWorker, buildX, buildY, baseType))
                        {
                            workerResourceAssignments.Remove(firstWorker.ID);
                            currentPhase = TestPhase.BuildBarracks;
                        }
                    }
                    else
                    {
                        if (executor.GetNextStepTowardTarget(firstWorker, buildX, buildY, out int stepX, out int stepY))
                        {
                            int direction = GetDirectionToTarget(firstWorker.X, firstWorker.Y, stepX, stepY);
                            if (direction != MicroRTSUtils.DIRECTION_NONE)
                            {
                                actionsOut.AddDiscreteAction($"moveDirection_unit{firstWorker.ID}", direction);
                            }
                        }
                    }
                }
            }
        }
        else if (myBases.Count > 1)
        {
            currentPhase = TestPhase.BuildBarracks;
        }
    }

    // ============================================================================
    // PHASE 4: BUILD BARRACKS
    // ============================================================================
    // Build barracks at fixed position (1, 3) using the same worker
    private void HandleBuildBarracksPhase(MicroRTSEnvironmentController env, MicroRTSActionExecutor executor, List<Unit> myBases, List<Unit> myWorkers, List<Unit> myBarracks, ActionBuffer actionsOut)
    {
        if (myBarracks.Count == 0)
        {
            var firstWorker = myWorkers.OrderBy(w => w.ID).FirstOrDefault();

            if (firstWorker != null && firstWorker.ID == builderWorkerID)
            {
                if (firstWorker.Resources > 0)
                {
                    var nearestBase = myBases
                        .OrderBy(b => Mathf.Abs(b.X - firstWorker.X) + Mathf.Abs(b.Y - firstWorker.Y))
                        .FirstOrDefault();

                    if (nearestBase != null)
                    {
                        actionsOut.AddDiscreteAction($"harvestTargetX_unit{firstWorker.ID}", nearestBase.X);
                        actionsOut.AddDiscreteAction($"harvestTargetY_unit{firstWorker.ID}", nearestBase.Y);
                    }
                }
                else if (!barracksBuildPosition.HasValue)
                {
                    barracksBuildPosition = (1, 3);
                }
                else if (barracksBuildPosition.HasValue && !executor.HasPendingAction(firstWorker))
                {
                    int buildX = barracksBuildPosition.Value.x;
                    int buildY = barracksBuildPosition.Value.y;

                    int dx = Mathf.Abs(firstWorker.X - buildX);
                    int dy = Mathf.Abs(firstWorker.Y - buildY);
                    bool isAdjacent = (dx == 1 && dy == 0) || (dx == 0 && dy == 1);

                    if (isAdjacent)
                    {
                        UnitType barracksType = env.UnitTypeTable.GetUnitType("Barracks");
                        if (barracksType != null && executor.ScheduleBuildAtPosition(firstWorker, buildX, buildY, barracksType))
                        {
                            workerResourceAssignments.Remove(firstWorker.ID);
                            currentPhase = TestPhase.ProduceBattleUnits;
                        }
                    }
                    else
                    {
                        if (executor.GetNextStepTowardTarget(firstWorker, buildX, buildY, out int stepX, out int stepY))
                        {
                            int direction = GetDirectionToTarget(firstWorker.X, firstWorker.Y, stepX, stepY);
                            if (direction != MicroRTSUtils.DIRECTION_NONE)
                            {
                                actionsOut.AddDiscreteAction($"moveDirection_unit{firstWorker.ID}", direction);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            barracksID = myBarracks[0].ID;
            currentPhase = TestPhase.ProduceBattleUnits;
        }
    }

    // ============================================================================
    // PHASE 5: PRODUCE BATTLE UNITS
    // ============================================================================
    // Produce one of each battle unit type (Light, Heavy, Ranged) from barracks
    private void HandleProduceBattleUnitsPhase(MicroRTSActionExecutor executor, List<Unit> myUnits, List<Unit> myBarracks, ActionBuffer actionsOut)
    {
        if (myBarracks.Count == 0) return;

        var barracks = myBarracks[0];

        int lightCount = myUnits.Count(u => u.Type.name == "Light");
        int heavyCount = myUnits.Count(u => u.Type.name == "Heavy");
        int rangedCount = myUnits.Count(u => u.Type.name == "Ranged");

        if (lightCount > 0 && heavyCount > 0 && rangedCount > 0)
        {
            currentPhase = TestPhase.Attack;
        }
        else if (currentBattleUnitIndex < battleUnitTypes.Count)
        {
            string unitType = battleUnitTypes[currentBattleUnitIndex];
            int currentCount = myUnits.Count(u => u.Type.name == unitType);

            if (!executor.HasPendingAction(barracks) && currentCount == 0)
            {
                string actionName = $"produce_{unitType}_unit{barracks.ID}";
                actionsOut.AddDiscreteAction(actionName, 1);
            }

            if (currentCount > 0 && !executor.HasPendingAction(barracks))
            {
                currentBattleUnitIndex++;
            }
        }
    }

    // ============================================================================
    // PHASE 6: ATTACK
    // ============================================================================
    // Heavy units attack enemy workers, Ranged and Light units attack enemy base
    private void HandleAttackPhase(MicroRTSEnvironmentController env, List<Unit> myUnits, ActionBuffer actionsOut)
    {
        var myHeavyUnits = myUnits.Where(u => u.Type.name == "Heavy" && u.Type.canAttack && u.HitPoints > 0).ToList();
        var myRangedUnits = myUnits.Where(u => u.Type.name == "Ranged" && u.Type.canAttack && u.HitPoints > 0).ToList();
        var myLightUnits = myUnits.Where(u => u.Type.name == "Light" && u.Type.canAttack && u.HitPoints > 0).ToList();

        foreach (var unit in myHeavyUnits)
        {
            var enemyUnits = env.GetAllUnits()
                .Where(u => u.Player != unit.Player && u.HitPoints > 0 && u.Player >= 0)
                .ToList();
            var enemyWorkers = enemyUnits.Where(u => u.Type.name == "Worker").ToList();
            var enemyBase = enemyUnits.FirstOrDefault(u => u.Type.name == "Base");

            Unit target = enemyWorkers.FirstOrDefault();
            if (target == null && enemyBase != null)
            {
                target = enemyBase;
            }

            if (target != null)
            {
                actionsOut.AddDiscreteAction($"attackTargetX_unit{unit.ID}", target.X);
                actionsOut.AddDiscreteAction($"attackTargetY_unit{unit.ID}", target.Y);
            }
        }

        foreach (var unit in myRangedUnits.Concat(myLightUnits))
        {
            var enemyUnits = env.GetAllUnits()
                .Where(u => u.Player != unit.Player && u.HitPoints > 0 && u.Player >= 0)
                .ToList();
            var enemyBase = enemyUnits.FirstOrDefault(u => u.Type.name == "Base");

            if (enemyBase != null)
            {
                actionsOut.AddDiscreteAction($"attackTargetX_unit{unit.ID}", enemyBase.X);
                actionsOut.AddDiscreteAction($"attackTargetY_unit{unit.ID}", enemyBase.Y);
            }
        }
    }

    // ============================================================================
    // HELPER METHODS
    // ============================================================================

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

    public override AgentController Clone() => this;
    public override void AddAgentControllerToSO(ScriptableObject parent) { }
}