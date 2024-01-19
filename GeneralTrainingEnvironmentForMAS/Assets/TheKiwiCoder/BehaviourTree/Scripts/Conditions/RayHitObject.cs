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
        RayPerceptionSensorComponent3D rayPerceptionSensorComponent = context.gameObject.GetComponent<RayPerceptionSensorComponent3D>();
        rayPerceptionSensorComponent.RayLayerMask = (1 << context.gameObject.layer) + 1; // base layer + default

        RayPerceptionOutput rayPerceptionOutput = RayPerceptionSensor.Perceive(rayPerceptionSensorComponent.GetRayPerceptionInput());

        bool targetHit = false;

        if (side == AgentSideAdvanced.Center) {
            //float hitDistancee = Vector3.Distance(rayPerceptionOutput.RayOutputs[0].StartPositionWorld, rayPerceptionOutput.RayOutputs[0].EndPositionWorld);
            if (rayPerceptionOutput.RayOutputs[0].HasHit && rayPerceptionOutput.RayOutputs[0].HitGameObject.name.Contains(TargetGameObjectsToString(targetGameObject)))
                targetHit = true;
            else
                targetHit = false;
        }
        else if (side == AgentSideAdvanced.Left) {
            for (int i = 2; i < rayPerceptionOutput.RayOutputs.Length; i += 2) {
                if (rayPerceptionOutput.RayOutputs[i].HasHit && rayPerceptionOutput.RayOutputs[i].HitGameObject.name.Contains(TargetGameObjectsToString(targetGameObject)))
                    targetHit = true;
            }
        }
        else if (side == AgentSideAdvanced.Right) {
            for (int i = 1; i < rayPerceptionOutput.RayOutputs.Length; i += 2) {
                if (rayPerceptionOutput.RayOutputs[i].HasHit && rayPerceptionOutput.RayOutputs[i].HitGameObject.name.Contains(TargetGameObjectsToString(targetGameObject)))
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
        }

        return TargetGameObject.Unknown;
    }
}
