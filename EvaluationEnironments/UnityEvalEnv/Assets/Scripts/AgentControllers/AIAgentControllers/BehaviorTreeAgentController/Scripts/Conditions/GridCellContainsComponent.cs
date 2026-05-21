using Base;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class GridCellContainsComponent: ConditionNode
    {
        public int targetGameObject;
        public ObjectTeamType targetTeamType;
        public int gridPositionX;
        public int gridPositionY;
        public int gridPositionZ;

        private TeamIdentifier baseGameObjectTeam;
        private TeamIdentifier targetGameObjectTeam;

        public GridCellContainsComponent(GridCellContainsComponent other) : base(other)
        {
            if (other == null)
                return;

            this.targetGameObject = other.targetGameObject;
            this.targetTeamType = other.targetTeamType;
            this.gridPositionX = other.gridPositionX;
            this.gridPositionY = other.gridPositionY;
            this.gridPositionZ = other.gridPositionZ;
        }

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
            bool gridContainsTarget = false;

            Problems.Grid grid = context.gameObject.GetComponentInParent<Problems.Grid>();

            Component[] components = grid.GetCellItem(gridPositionX, gridPositionY, gridPositionZ);
            if (components == null || components.Length == 0)
            {
                gridContainsTarget = false;
            }
            else
            {
                foreach (var component in components)
                {
                    if (component == null) continue;

                    if (component.gameObject.tag.Contains(TargetGameObjects[targetGameObject]) && TargetTeamHit(component.gameObject))
                    {
                        gridContainsTarget = true;
                        break;
                    }
                }
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

        public override BTNode Clone()
        {
            return new GridCellContainsComponent(this);
        }
    }
}