using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheKiwiCoder;
using UnityEngine;

public enum TargetGameObject2D {
    Agent,
    Indestructible,
    Destructible,
    Bomb,
    Explosion,
    PowerUp,
    Empty,
    Food,
    Water,
    FoodPheromone,
    WaterPheromone,
    BoundaryPheromone,
    ThreatPheromone,
    Hive

}

public class GridCellContainsObject : ConditionNode {

    public TargetGameObject2D targetGameObject;
    public int gridPositionX;
    public int gridPositionY;

    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    public bool Check() {
        return CheckConditions();
    }

    protected override bool CheckConditions() {
        GridSensor2D gridSensor2D = context.gameObject.GetComponent<GridSensor2D>();
        gridSensor2D.LayerMask = (1 << context.gameObject.layer) + 1;
        SensorPerceiveOutput[,] sensorOutputs = gridSensor2D.Perceive();

        bool gridContainsTarget = false;

        if (sensorOutputs[gridPositionX, gridPositionY] != null && sensorOutputs[gridPositionX, gridPositionY].HasHit) {
            foreach (GameObject obj in sensorOutputs[gridPositionX, gridPositionY].HitGameObjects) {
                switch (targetGameObject) {
                    case TargetGameObject2D.Agent:
                        if (obj.GetComponent<BombermanAgentComponent>() != null) {
                            gridContainsTarget = true;
                        }
                        break;
                    case TargetGameObject2D.Indestructible:
                        if (obj.name.Contains("Indestructable")) {
                            gridContainsTarget = true;
                        }
                        break;
                    case TargetGameObject2D.Destructible:
                        if (obj.name.Contains("Destructable")) {
                            gridContainsTarget = true;
                        }
                        break;
                    case TargetGameObject2D.Bomb:
                        if (obj.name.Contains("Bomb")) {
                            gridContainsTarget = true;
                        }
                        break;
                    case TargetGameObject2D.Explosion:
                        if (obj.name.Contains("Explosion")) {
                            gridContainsTarget = true;
                        }
                        break;
                    case TargetGameObject2D.Food:
                        if (obj.name.Contains("Food")){
                            gridContainsTarget = true;
                        }
                        break;
                    case TargetGameObject2D.Water:
                        if (obj.name.Contains("Water"))
                        {
                            gridContainsTarget = true;
                        }
                        break;
                    case TargetGameObject2D.WaterPheromone:
                        if (obj.name.Contains("WaterPheromone")){
                            gridContainsTarget = true;
                        }
                        break;
                    case TargetGameObject2D.FoodPheromone:
                        if (obj.name.Contains("FoodPheromone"))
                        {
                            gridContainsTarget = true;
                        }
                        break;
                    case TargetGameObject2D.BoundaryPheromone:
                        if (obj.name.Contains("BoundaryPheromone"))
                        {
                            gridContainsTarget = true;
                        }
                        break;
                    case TargetGameObject2D.ThreatPheromone:
                        if (obj.name.Contains("ThreatPheromone"))
                        {
                            gridContainsTarget = true;
                        }
                        break;
                    case TargetGameObject2D.Hive:
                        if (obj.name.Contains("Hive")){
                            gridContainsTarget = true;
                        }
                        break;
                    
                }
            }
        }
        else if (targetGameObject == TargetGameObject2D.Empty) {
            gridContainsTarget = true;
        }

        return gridContainsTarget;

    }
}