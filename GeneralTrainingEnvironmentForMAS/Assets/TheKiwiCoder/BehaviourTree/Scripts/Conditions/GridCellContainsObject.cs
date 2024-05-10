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
    Empty
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
                }
            }
        }
        else if (targetGameObject == TargetGameObject2D.Empty) {
            gridContainsTarget = true;
        }

        return gridContainsTarget;

    }

    public static Node CreateNodeFromBehaviourTreeNodeDef(BehaviourTreeNodeDef behaviourTreeNodeDef, List<BehaviourTreeNodeDef> behaviourTreeNodeDefs, BehaviourTree tree) {
        // Create node
        GridCellContainsObject gridCellContainsObjectNode = new GridCellContainsObject();

        // Set node properties
        gridCellContainsObjectNode.targetGameObject = (TargetGameObject2D)int.Parse(behaviourTreeNodeDef.node_properties["targetGameObject"]);
        gridCellContainsObjectNode.gridPositionX = int.Parse(behaviourTreeNodeDef.node_properties["gridPositionX"]);
        gridCellContainsObjectNode.gridPositionY = int.Parse(behaviourTreeNodeDef.node_properties["gridPositionY"]);

        tree.nodes.Add(gridCellContainsObjectNode);
        return gridCellContainsObjectNode;
    }
}