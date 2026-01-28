using Base;
using System;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class GridCellContainsObject : Activator
    {
        public int targetGameObject;
        public ObjectTeamType targetTeamType;
        public int gridPositionX;
        public int gridPositionY;

        private GridSensor2D gridSensor;

        private TeamIdentifier baseGameObjectTeam;
        private TeamIdentifier targetGameObjectTeam;

        public override void Init()
        {
            gridSensor = context.gameObject.GetComponent<GridSensor2D>();
            gridSensor.LayerMask = (1 << context.gameObject.layer);

            GetBaseGameObjectTeam();
        }

        public override bool IsActivated()
        {
            SensorPerceiveOutput[,] sensorOutputs = gridSensor.PerceiveSingle(gridPositionX, gridPositionY);

            bool gridContainsTarget = false;

            if (sensorOutputs[0, 0] != null && sensorOutputs[0, 0].HasHit)
            {
                foreach (GameObject obj in sensorOutputs[0, 0].HitGameObjects)
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