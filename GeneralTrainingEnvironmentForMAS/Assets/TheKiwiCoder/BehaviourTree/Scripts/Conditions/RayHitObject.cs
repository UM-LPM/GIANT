using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using Unity.VisualScripting;
using System.Linq;

public enum AgentSideBasic {
    Center = 0,
    Left = 1,
    Right = 2
}
public enum AgentSideAdvanced {
    Center = 0,
    Left = 1,
    Right = 2,
    BackCenter = 3,
    BackLeft = 4,
    BackRight = 5
}

public enum TargetGameObject {
    Agent,
    Wall,
    Obstacle,
    Object1,
    Object2,
    Object3,
    Object4,
    Object5,
    Object6,
    Object7,
    Unknown
}

public class RayHitObject : ConditionNode {

    public TargetGameObject targetGameObject;
    public AgentSideAdvanced side;
    public int rayIndex;

    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    public bool Check() {
        return CheckConditions();
    }

    protected override bool CheckConditions() {
        RaySensorBase raySensor = context.gameObject.GetComponentInChildren<RaySensorBase>();
        raySensor.LayerMask = (1 << context.gameObject.layer) + 1; // base layer + default

        SensorPerceiveOutput[] sensorPerceiveOutputs = raySensor.Perceive();

        bool targetHit = false;

        if (sensorPerceiveOutputs[rayIndex].HasHit && sensorPerceiveOutputs[rayIndex].HitGameObjects[0].name.Contains(TargetGameObjectsToString(targetGameObject)))
            targetHit = true;

        /*if (side == AgentSideAdvanced.Center) {
            if (sensorPerceiveOutputs[0].HasHit && sensorPerceiveOutputs[0].HitGameObjects[0].name.Contains(TargetGameObjectsToString(targetGameObject)))
                targetHit = true;
            else
                targetHit = false;
        }
        else if (side == AgentSideAdvanced.Left) {
            for (int i = 2; i < sensorPerceiveOutputs.Length; i += 2) {
                if (sensorPerceiveOutputs[i].HasHit && sensorPerceiveOutputs[i].HitGameObjects[0].name.Contains(TargetGameObjectsToString(targetGameObject)))
                    targetHit = true;
            }
        }
        else if (side == AgentSideAdvanced.Right) {
            for (int i = 1; i < sensorPerceiveOutputs.Length; i += 2) {
                if (sensorPerceiveOutputs[i].HasHit && sensorPerceiveOutputs[i].HitGameObjects[0].name.Contains(TargetGameObjectsToString(targetGameObject)))
                    targetHit = true;
            }
        }*/

        return targetHit;
    }

    public static string TargetGameObjectsToString(TargetGameObject targetGameObject) {
        switch (targetGameObject) {
            case TargetGameObject.Agent:
                return "Agent";
            case TargetGameObject.Wall:
                return "Wall";
            case TargetGameObject.Obstacle:
                return "Obstacle";
            case TargetGameObject.Object1:
                return "Object1";
            case TargetGameObject.Object2:
                return "Object2";
            case TargetGameObject.Object3:
                return "Object3";
            case TargetGameObject.Object4:
                return "Object4";
            case TargetGameObject.Object5:
                return "Object5";
            case TargetGameObject.Object6:
                return "Object6";
            case TargetGameObject.Object7:
                return "Object7";
        }
        return "";
    }

    public static TargetGameObject TargetGameObjectsFromString(string targetGameObjectString) {
        switch (targetGameObjectString) {
            case "Agent":
                return TargetGameObject.Agent;
            case "Wall":
                return TargetGameObject.Wall;
            case "Obstacle":
                return TargetGameObject.Obstacle;
            case "Object1":
                return TargetGameObject.Object1;
            case "Object2":
                return TargetGameObject.Object2;
            case "Object3":
                return TargetGameObject.Object3;
            case "Object4":
                return TargetGameObject.Object4;
            case "Object5":
                return TargetGameObject.Object5;
            case "Object6":
                return TargetGameObject.Object6;
            case "Object7":
                return TargetGameObject.Object7;
        }

        return TargetGameObject.Unknown;
    }
    public static Node CreateNodeFromBehaviourTreeNodeDef(BehaviourTreeNodeDef behaviourTreeNodeDef, List<BehaviourTreeNodeDef> behaviourTreeNodeDefs, BehaviourTree tree) {
        // Create node
        RayHitObject rayHitObjectNode = new RayHitObject();

        // Set node properties
        rayHitObjectNode.targetGameObject = (TargetGameObject)int.Parse(behaviourTreeNodeDef.node_properties["targetGameObject"]);
        rayHitObjectNode.side = (AgentSideAdvanced)int.Parse(behaviourTreeNodeDef.node_properties["side"]);
        rayHitObjectNode.rayIndex = int.Parse(behaviourTreeNodeDef.node_properties["rayIndex"]);

        tree.nodes.Add(rayHitObjectNode);
        return rayHitObjectNode;
    }

}
