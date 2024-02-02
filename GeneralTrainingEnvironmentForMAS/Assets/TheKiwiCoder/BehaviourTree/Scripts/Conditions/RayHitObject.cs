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
    Target1,
    Target2,
    Target3,
    Target4,
    Target5,
    Target6,
    Target7,
    Unknown
}

public class RayHitObject : ConditionNode {

    public TargetGameObject targetGameObject;
    public AgentSideAdvanced side;

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

        if (side == AgentSideAdvanced.Center) {
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
        }

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
            case TargetGameObject.Target1:
                return "Target1";
            case TargetGameObject.Target2:
                return "Target2";
            case TargetGameObject.Target3:
                return "Target3";
            case TargetGameObject.Target4:
                return "Target4";
            case TargetGameObject.Target5:
                return "Target5";
            case TargetGameObject.Target6:
                return "Target6";
            case TargetGameObject.Target7:
                return "Target7";
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
            case "Target1":
                return TargetGameObject.Target1;
            case "Target2":
                return TargetGameObject.Target2;
            case "Target3":
                return TargetGameObject.Target3;
            case "Target4":
                return TargetGameObject.Target4;
            case "Target5":
                return TargetGameObject.Target5;
            case "Target6":
                return TargetGameObject.Target6;
            case "Target7":
                return TargetGameObject.Target7;
        }

        return TargetGameObject.Unknown;
    }
}
