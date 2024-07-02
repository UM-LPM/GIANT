using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using Unity.VisualScripting;
using System.Linq;
using static EnvironmentControllerBase;
using System;

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

    public static event EventHandler<OnTargetHitEventargs> OnTargetHit;

    public TargetGameObject targetGameObject;
    public AgentSideAdvanced side;
    public int rayIndex;
    public RaySensorBase raySensor;
    
    private SensorPerceiveOutput[] sensorPerceiveOutputs;
    private bool targetHit;

    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override bool CheckConditions() {
        if(raySensor == null)
        {
            raySensor = context.gameObject.GetComponentInChildren<RaySensorBase>();
            raySensor.LayerMask = (1 << context.gameObject.layer) + 1; // base layer + default
        }

        targetHit = false;

        sensorPerceiveOutputs = raySensor.PerceiveSingle(xPos: rayIndex);
        //sensorPerceiveOutputs = raySensor.PerceiveAll();

        // Option 1 : Check if the target game object is hit by the single ray based on RayIndex
        if (sensorPerceiveOutputs[rayIndex].HasHit && sensorPerceiveOutputs[rayIndex].HitGameObjects[0].name.Contains(TargetGameObjectsToString(targetGameObject)))
        {
            targetHit = true;

            // Trigger the event
            OnTargetHit?.Invoke(this, new OnTargetHitEventargs { TargetGameObject = sensorPerceiveOutputs[rayIndex].HitGameObjects[0], Agent = context.gameObject.GetComponent<AgentComponent>() });
        }

        // Option 2 : Check if the target game object is hit by any of the rays based on Side
        /*int hitIndex = -1;
        if (side == AgentSideAdvanced.Center) {
            if (sensorPerceiveOutputs[0].HasHit && sensorPerceiveOutputs[0].HitGameObjects[0].name.Contains(TargetGameObjectsToString(targetGameObject)))
            {
                targetHit = true;
                hitIndex = 0;
            }
        }
        else if (side == AgentSideAdvanced.Left) {
            for (int i = 2; i < sensorPerceiveOutputs.Length; i += 2) {
                if (sensorPerceiveOutputs[i].HasHit && sensorPerceiveOutputs[i].HitGameObjects[0].name.Contains(TargetGameObjectsToString(targetGameObject)))
                {
                    targetHit = true;
                    hitIndex = i;
                }
            }
        }
        else if (side == AgentSideAdvanced.Right) {
            for (int i = 1; i < sensorPerceiveOutputs.Length; i += 2) {
                if (sensorPerceiveOutputs[i].HasHit && sensorPerceiveOutputs[i].HitGameObjects[0].name.Contains(TargetGameObjectsToString(targetGameObject)))
                {
                    targetHit = true;
                    hitIndex = i;
                }
            }
        }

        if (targetHit)
        {
            // Trigger the event
            OnTargetHit?.Invoke(this, new OnTargetHitEventargs { TargetGameObject = sensorPerceiveOutputs[hitIndex].HitGameObjects[0], Agent = context.gameObject.GetComponent<AgentComponent>() });
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
public class OnTargetHitEventargs : EventArgs
{
    public GameObject TargetGameObject { get; set; }
    public AgentComponent Agent { get; set; }
}
