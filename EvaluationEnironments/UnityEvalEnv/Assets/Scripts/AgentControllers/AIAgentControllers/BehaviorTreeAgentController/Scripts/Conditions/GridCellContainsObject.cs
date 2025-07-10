using Base;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class GridCellContainsObject : ConditionNode
    {
        public int targetGameObject;
        public ObjectTeamType targetTeamType;
        public int gridPositionX;
        public int gridPositionY;

        private TeamIdentifier baseGameObjectTeam;
        private TeamIdentifier targetGameObjectTeam;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        public bool Check()
        {
            return CheckConditions();
        }

        protected override bool CheckConditions()
        {
            GridSensor2D gridSensor2D = context.gameObject.GetComponent<GridSensor2D>();
            gridSensor2D.LayerMask = (1 << context.gameObject.layer) + 1;
            SensorPerceiveOutput[,] sensorOutputs = gridSensor2D.PerceiveAll();

            bool gridContainsTarget = false;

            if (sensorOutputs[gridPositionX, gridPositionY] != null && sensorOutputs[gridPositionX, gridPositionY].HasHit)
            {
                foreach (GameObject obj in sensorOutputs[gridPositionX, gridPositionY].HitGameObjects)
                {
                    if (obj.tag.Contains(TargetGameObjects[targetGameObject]) && TargetTeamHit(obj))
                    {
                        gridContainsTarget = true;
                    }
                }
            }
            else if (targetGameObject == -1)
            {
                gridContainsTarget = true;
            }

            return gridContainsTarget;

        }

        public void GetBaseGameObjectTeam()
        {
            // Search for TeamIdentifier components in base game object
            baseGameObjectTeam = context.gameObject.GetComponent<TeamIdentifier>();

            // If base game object doesn't contain the component try to find it in children game objects
            if (baseGameObjectTeam == null)
                baseGameObjectTeam = context.gameObject.GetComponentInChildren<TeamIdentifier>();
        }

        public bool TargetTeamHit(GameObject hitGameObject)
        {
            if (baseGameObjectTeam != null)
            {
                targetGameObjectTeam = hitGameObject.GetComponent<TeamIdentifier>();

                if (targetGameObjectTeam != null)
                {
                    switch (targetTeamType)
                    {
                        case ObjectTeamType.Default:
                            break;
                        case ObjectTeamType.Teammate:
                            return baseGameObjectTeam.TeamID == targetGameObjectTeam.TeamID;
                        case ObjectTeamType.Opponent:
                            return baseGameObjectTeam.TeamID != targetGameObjectTeam.TeamID;
                    }
                }
            }
            return true;
        }
    }
}