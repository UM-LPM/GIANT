using AgentControllers;
using UnityEngine;
using UnityEngine.EventSystems;
using Problems.MicroRTS.Core;
using Utils;
using System.Collections.Generic;

namespace Problems.MicroRTS
{
    [CreateAssetMenu(fileName = "MicroRTSManualAgentController", menuName = "AgentControllers/ManualAgentControllers/MicroRTSManualAgentController")]
    public class MicroRTSManualAgentController : ManualAgentController
    {
        [SerializeField] private long selectedUnitID = -1;
        private Dictionary<long, (int targetX, int targetY)> persistentAttackTargets = new Dictionary<long, (int, int)>();
        private Dictionary<long, (int targetX, int targetY)> persistentHarvestTargets = new Dictionary<long, (int, int)>();
        private const int TEAM_0 = 0;

        private bool leftClickPressed = false;
        private bool rightClickPressed = false;
        private long previouslyHighlightedUnitID = -1;

        public override void GetActions(in ActionBuffer actionsOut)
        {
            MicroRTSEnvironmentController environmentController = Object.FindObjectOfType<MicroRTSEnvironmentController>();
            if (environmentController == null)
            {
                DebugSystem.LogWarning("EnvironmentController not found");
                return;
            }

            ValidatePersistentTargets(environmentController);
            HandleSelection(environmentController);
            UpdateHighlighting(environmentController);
            HandleKeyboardInput(environmentController, actionsOut);
            HandleRightClick(environmentController, actionsOut);
            ApplyPersistentTargets(environmentController, actionsOut);
        }

        private void HandleSelection(MicroRTSEnvironmentController environmentController)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            bool currentLeftClick = Input.GetMouseButtonDown(0);
            if (currentLeftClick && !leftClickPressed)
            {
                leftClickPressed = true;

                Camera cam = Camera.main;
                if (cam == null) return;

                Vector3 worldPoint = GetWorldPointFromMouse(cam);
                if (worldPoint == Vector3.zero)
                {
                    selectedUnitID = -1;
                    return;
                }

                if (environmentController.TryWorldToGrid(worldPoint, out int gridX, out int gridY))
                {
                    Unit unitAtPosition = environmentController.GetUnitAt(gridX, gridY);
                    if (unitAtPosition != null && unitAtPosition.Player == TEAM_0)
                    {
                        selectedUnitID = unitAtPosition.ID;
                        DebugSystem.Log($"Selected {unitAtPosition.Type.name} at ({unitAtPosition.X}, {unitAtPosition.Y})");
                    }
                    else
                    {
                        selectedUnitID = -1;
                    }
                }
                else
                {
                    selectedUnitID = -1;
                }
            }
            else if (!currentLeftClick)
            {
                leftClickPressed = false;
            }
        }

        private void HandleKeyboardInput(MicroRTSEnvironmentController environmentController, ActionBuffer actionsOut)
        {
            if (selectedUnitID < 0) return;

            Unit selectedUnit = environmentController.GetUnit(selectedUnitID);
            if (selectedUnit == null || selectedUnit.Player != TEAM_0 || selectedUnit.HitPoints <= 0)
            {
                selectedUnitID = -1;
                return;
            }

            string unitActionPrefix = $"unit{selectedUnit.ID}";

            if (selectedUnit.Type.canMove)
            {
                int direction = GetMovementDirection();
                if (direction != MicroRTSUtils.DIRECTION_NONE)
                {
                    actionsOut.AddDiscreteAction($"moveDirection_{unitActionPrefix}", direction);
                }
            }

            string unitTypeName = selectedUnit.Type.name;

            if (unitTypeName == "Base")
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    actionsOut.AddDiscreteAction($"produce_Worker_{unitActionPrefix}", 1);
                    DebugSystem.Log($"Base producing Worker");
                }
            }
            else if (unitTypeName == "Worker")
            {
                int buildType = Input.GetKeyDown(KeyCode.Alpha1) ? 1 : (Input.GetKeyDown(KeyCode.Alpha2) ? 2 : 0);
                if (buildType > 0)
                {
                    int direction = GetArrowKeyDirection();
                    if (direction != MicroRTSUtils.DIRECTION_NONE)
                    {
                        string buildingType = buildType == 1 ? "Base" : "Barracks";
                        UnitType unitType = environmentController.UnitTypeTable.GetUnitType(buildingType);
                        if (unitType != null)
                        {
                            Vector2Int offset = MicroRTSUtils.GetDirectionOffset(direction);
                            int targetX = selectedUnit.X + offset.x;
                            int targetY = selectedUnit.Y + offset.y;

                            MicroRTSActionExecutor actionExecutor = environmentController.GetComponent<MicroRTSActionExecutor>();
                            if (actionExecutor != null && actionExecutor.ScheduleBuildAtPosition(selectedUnit, targetX, targetY, unitType))
                            {
                                DebugSystem.Log($"Worker building {buildingType} {GetDirectionName(direction)}");
                            }
                        }
                    }
                }
            }
            else if (unitTypeName == "Barracks")
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    actionsOut.AddDiscreteAction($"produce_Light_{unitActionPrefix}", 1);
                    DebugSystem.Log($"Barracks producing Light");
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    actionsOut.AddDiscreteAction($"produce_Heavy_{unitActionPrefix}", 1);
                    DebugSystem.Log($"Barracks producing Heavy");
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    actionsOut.AddDiscreteAction($"produce_Ranged_{unitActionPrefix}", 1);
                    DebugSystem.Log($"Barracks producing Ranged");
                }
            }
        }

        private void HandleRightClick(MicroRTSEnvironmentController environmentController, ActionBuffer actionsOut)
        {
            if (selectedUnitID < 0) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            bool currentRightClick = Input.GetMouseButtonDown(1);
            if (currentRightClick && !rightClickPressed)
            {
                rightClickPressed = true;

                Unit selectedUnit = environmentController.GetUnit(selectedUnitID);
                if (selectedUnit == null || selectedUnit.Player != TEAM_0 || selectedUnit.HitPoints <= 0)
                {
                    selectedUnitID = -1;
                    return;
                }

                Camera cam = Camera.main;
                if (cam == null) return;

                Vector3 worldPoint = GetWorldPointFromMouse(cam);
                if (worldPoint == Vector3.zero) return;

                if (environmentController.TryWorldToGrid(worldPoint, out int targetX, out int targetY))
                {
                    Unit targetUnit = environmentController.GetUnitAt(targetX, targetY);
                    string unitActionPrefix = $"unit{selectedUnit.ID}";

                    if (targetUnit != null)
                    {
                        if (targetUnit.Type.isResource && selectedUnit.Type.canHarvest)
                        {
                            persistentHarvestTargets[selectedUnit.ID] = (targetX, targetY);
                            actionsOut.AddDiscreteAction($"harvestTargetX_{unitActionPrefix}", targetX);
                            actionsOut.AddDiscreteAction($"harvestTargetY_{unitActionPrefix}", targetY);
                            DebugSystem.Log($"Harvesting resource at ({targetX}, {targetY})");
                        }
                        else if (targetUnit.Player != TEAM_0 && targetUnit.Player >= 0 && selectedUnit.Type.canAttack)
                        {
                            persistentAttackTargets[selectedUnit.ID] = (targetX, targetY);
                            actionsOut.AddDiscreteAction($"attackTargetX_{unitActionPrefix}", targetX);
                            actionsOut.AddDiscreteAction($"attackTargetY_{unitActionPrefix}", targetY);
                            DebugSystem.Log($"Attacking {targetUnit.Type.name} at ({targetX}, {targetY})");
                        }
                    }
                    else if (selectedUnit.Type.canMove)
                    {
                        int dx = targetX - selectedUnit.X;
                        int dy = targetY - selectedUnit.Y;

                        int direction = Mathf.Abs(dx) + Mathf.Abs(dy) == 1
                            ? GetDirectionFromOffset(dx, dy)
                            : GetClosestDirection(dx, dy);
                        if (direction != MicroRTSUtils.DIRECTION_NONE)
                        {
                            actionsOut.AddDiscreteAction($"moveDirection_{unitActionPrefix}", direction);
                        }
                    }
                }
            }
            else if (!currentRightClick)
            {
                rightClickPressed = false;
            }
        }

        private Vector3 GetWorldPointFromMouse(Camera cam)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                return hit.point;
            }

            Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
            float distance;
            if (groundPlane.Raycast(ray, out distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        private int GetDirectionFromOffset(int dx, int dy)
        {
            if (dx == 0 && dy == -1) return MicroRTSUtils.DIRECTION_UP;
            if (dx == 1 && dy == 0) return MicroRTSUtils.DIRECTION_RIGHT;
            if (dx == 0 && dy == 1) return MicroRTSUtils.DIRECTION_DOWN;
            if (dx == -1 && dy == 0) return MicroRTSUtils.DIRECTION_LEFT;
            return MicroRTSUtils.DIRECTION_NONE;
        }

        private int GetMovementDirection()
        {
            if (Input.GetKey(KeyCode.W)) return MicroRTSUtils.DIRECTION_UP;
            if (Input.GetKey(KeyCode.S)) return MicroRTSUtils.DIRECTION_DOWN;
            if (Input.GetKey(KeyCode.A)) return MicroRTSUtils.DIRECTION_LEFT;
            if (Input.GetKey(KeyCode.D)) return MicroRTSUtils.DIRECTION_RIGHT;
            return MicroRTSUtils.DIRECTION_NONE;
        }

        private int GetArrowKeyDirection()
        {
            if (Input.GetKey(KeyCode.UpArrow)) return MicroRTSUtils.DIRECTION_UP;
            if (Input.GetKey(KeyCode.DownArrow)) return MicroRTSUtils.DIRECTION_DOWN;
            if (Input.GetKey(KeyCode.LeftArrow)) return MicroRTSUtils.DIRECTION_LEFT;
            if (Input.GetKey(KeyCode.RightArrow)) return MicroRTSUtils.DIRECTION_RIGHT;
            return MicroRTSUtils.DIRECTION_NONE;
        }

        private string GetDirectionName(int direction)
        {
            return direction == MicroRTSUtils.DIRECTION_UP ? "UP" :
                   direction == MicroRTSUtils.DIRECTION_DOWN ? "DOWN" :
                   direction == MicroRTSUtils.DIRECTION_LEFT ? "LEFT" : "RIGHT";
        }

        private int GetClosestDirection(int dx, int dy)
        {
            if (Mathf.Abs(dy) > Mathf.Abs(dx))
                return dy < 0 ? MicroRTSUtils.DIRECTION_UP : MicroRTSUtils.DIRECTION_DOWN;
            return dx > 0 ? MicroRTSUtils.DIRECTION_RIGHT : MicroRTSUtils.DIRECTION_LEFT;
        }

        private void ValidatePersistentTargets(MicroRTSEnvironmentController environmentController)
        {
            ValidateTargets(persistentAttackTargets, environmentController, (unit, target) =>
                target == null || target.HitPoints <= 0 || target.Player == TEAM_0 ||
                target.Player < 0 || target.Type.isResource, "attack");

            ValidateTargets(persistentHarvestTargets, environmentController, (unit, target) =>
                target == null || !target.Type.isResource || target.Resources <= 0, "harvest");
        }

        private void ValidateTargets(Dictionary<long, (int targetX, int targetY)> targets,
            MicroRTSEnvironmentController env, System.Func<Unit, Unit, bool> isInvalid, string type)
        {
            var toRemove = new List<long>();
            foreach (var kvp in targets)
            {
                Unit unit = env.GetUnit(kvp.Key);
                if (unit == null || unit.Player != TEAM_0 || unit.HitPoints <= 0)
                {
                    toRemove.Add(kvp.Key);
                    continue;
                }

                Unit target = env.GetUnitAt(kvp.Value.targetX, kvp.Value.targetY);
                if (isInvalid(unit, target))
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var id in toRemove) targets.Remove(id);
        }

        private void ApplyPersistentTargets(MicroRTSEnvironmentController environmentController, ActionBuffer actionsOut)
        {
            ApplyTargets(persistentAttackTargets, environmentController, actionsOut,
                (u) => u.Type.canAttack, "attackTarget");
            ApplyTargets(persistentHarvestTargets, environmentController, actionsOut,
                (u) => u.Type.canHarvest, "harvestTarget");
        }

        private void ApplyTargets(Dictionary<long, (int targetX, int targetY)> targets,
            MicroRTSEnvironmentController env, ActionBuffer actionsOut,
            System.Func<Unit, bool> canPerform, string actionPrefix)
        {
            foreach (var kvp in targets)
            {
                Unit unit = env.GetUnit(kvp.Key);
                if (unit != null && unit.Player == TEAM_0 && unit.HitPoints > 0 && canPerform(unit))
                {
                    string unitActionPrefix = $"unit{unit.ID}";
                    actionsOut.AddDiscreteAction($"{actionPrefix}X_{unitActionPrefix}", kvp.Value.targetX);
                    actionsOut.AddDiscreteAction($"{actionPrefix}Y_{unitActionPrefix}", kvp.Value.targetY);
                }
            }
        }

        private void UpdateHighlighting(MicroRTSEnvironmentController environmentController)
        {
            if (previouslyHighlightedUnitID != selectedUnitID)
            {
                if (previouslyHighlightedUnitID >= 0)
                {
                    GameObject prevUnitObj = environmentController.GetUnitGameObject(previouslyHighlightedUnitID);
                    if (prevUnitObj != null)
                    {
                        MicroRTSUnitHighlighter highlighter = prevUnitObj.GetComponent<MicroRTSUnitHighlighter>();
                        if (highlighter != null)
                        {
                            highlighter.SetHighlighted(false);
                        }
                    }
                }

                if (selectedUnitID >= 0)
                {
                    GameObject unitObj = environmentController.GetUnitGameObject(selectedUnitID);
                    if (unitObj != null)
                    {
                        MicroRTSUnitHighlighter highlighter = unitObj.GetComponent<MicroRTSUnitHighlighter>();
                        if (highlighter == null)
                        {
                            highlighter = unitObj.AddComponent<MicroRTSUnitHighlighter>();
                        }
                        highlighter.SetHighlighted(true);
                    }
                }

                previouslyHighlightedUnitID = selectedUnitID;
            }
        }

        public override AgentController Clone()
        {
            return this;
        }

        public override void AddAgentControllerToSO(ScriptableObject parent)
        {
            return;
        }
    }
}