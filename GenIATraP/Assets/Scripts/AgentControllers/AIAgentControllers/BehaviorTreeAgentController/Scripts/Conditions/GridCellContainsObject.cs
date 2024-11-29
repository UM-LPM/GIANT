using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public class GridCellContainsObject : ConditionNode
    {

        public TargetGameObject targetGameObject;
        public int gridPositionX;
        public int gridPositionY;

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
                    if (obj.tag.Contains(RayHitObject.TargetGameObjectsToString(targetGameObject)))
                    {
                        gridContainsTarget = true;
                    }
                }
            }
            else if (targetGameObject == TargetGameObject.Empty)
            {
                gridContainsTarget = true;
            }

            return gridContainsTarget;

        }
    }
}