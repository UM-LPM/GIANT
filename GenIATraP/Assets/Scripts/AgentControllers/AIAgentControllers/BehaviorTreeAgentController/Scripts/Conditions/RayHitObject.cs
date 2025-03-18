using UnityEngine;
using System;
using Base;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    public enum AgentSideBasic
    {
        Center = 0,
        Left = 1,
        Right = 2
    }
    public enum AgentSideAdvanced
    {
        Center = 0,
        Left = 1,
        Right = 2,
        BackCenter = 3,
        BackLeft = 4,
        BackRight = 5
    }

    public enum TargetGameObject
    {
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
        Object8,
        Object9,
        Object10,
        Object11,
        Object12,
        Object13,
        Object14,
        Object15,
        Object16,
        Object17,
        Object18,
        Object19,
        Object20,
        Object21,
        Object22,
        Object23,
        Object24,
        Object25,
        Object26,
        Object27,
        Object28,
        Object29,
        Object30,
        Object31,
        Object32,
        Object33,
        Object34,
        Object35,
        Object36,
        Object37,
        Object38,
        Object39,
        Object40,
        Empty
    }

    public enum RayHitObjectDetectionType
    {
        RayIndex,
        RaySide
    }

    public class RayHitObject : ConditionNode
    {
        public static RayHitObjectDetectionType RAY_HIT_OBJECT_DETECTION_TYPE = RayHitObjectDetectionType.RayIndex;
        public static event EventHandler<OnTargetHitEventargs> OnTargetHit;

        public TargetGameObject targetGameObject;
        public AgentSideAdvanced side;
        public int rayIndex;
        public RaySensorBase raySensor;

        private SensorPerceiveOutput[] sensorPerceiveOutputs;
        private bool targetHit;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override bool CheckConditions()
        {
            if (raySensor == null)
            {
                raySensor = context.gameObject.GetComponentInChildren<RaySensorBase>();
                raySensor.SetLayerMask((1 << context.gameObject.layer) + 1); // base layer + default
            }

            targetHit = false;

            if (RAY_HIT_OBJECT_DETECTION_TYPE == RayHitObjectDetectionType.RayIndex)
            {
                // Option 1 : Check if the target game object is hit by the single ray based on RayIndex
                sensorPerceiveOutputs = raySensor.PerceiveSingle(xPos: rayIndex);

                if (sensorPerceiveOutputs[rayIndex].HasHit && sensorPerceiveOutputs[rayIndex].HitGameObjects[0].tag.Contains(TargetGameObjectsToString(targetGameObject)))
                {
                    targetHit = true;

                    // Trigger the event
                    OnTargetHit?.Invoke(this, new OnTargetHitEventargs { TargetGameObject = sensorPerceiveOutputs[rayIndex].HitGameObjects[0], Agent = context.gameObject.GetComponent<AgentComponent>() });
                }
            }

            if (RAY_HIT_OBJECT_DETECTION_TYPE == RayHitObjectDetectionType.RaySide)
            {
                // Option 2 : Check if the target game object is hit by any of the rays based on Side
                //sensorPerceiveOutputs = raySensor.PerceiveAll();

                int hitIndex = -1;
                if (side == AgentSideAdvanced.Center)
                {
                    sensorPerceiveOutputs = raySensor.PerceiveRange(0, 1, 2);
                    if (sensorPerceiveOutputs[0].HasHit && sensorPerceiveOutputs[0].HitGameObjects[0].tag.Contains(TargetGameObjectsToString(targetGameObject)))
                    {
                        targetHit = true;
                        hitIndex = 0;
                    }
                }
                else if (side == AgentSideAdvanced.Left)
                {
                    sensorPerceiveOutputs = raySensor.PerceiveRange(2, raySensor.GetRayPerceptionInput().Angles.Count, 2);
                    for (int i = 2; i < sensorPerceiveOutputs.Length; i += 2)
                    {
                        if (sensorPerceiveOutputs[i].HasHit && sensorPerceiveOutputs[i].HitGameObjects[0].tag.Contains(TargetGameObjectsToString(targetGameObject)))
                        {
                            targetHit = true;
                            hitIndex = i;
                        }
                    }
                }
                else if (side == AgentSideAdvanced.Right)
                {
                    sensorPerceiveOutputs = raySensor.PerceiveRange(1, raySensor.GetRayPerceptionInput().Angles.Count, 2);
                    for (int i = 1; i < sensorPerceiveOutputs.Length; i += 2)
                    {
                        if (sensorPerceiveOutputs[i].HasHit && sensorPerceiveOutputs[i].HitGameObjects[0].tag.Contains(TargetGameObjectsToString(targetGameObject)))
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
                }
            }

            return targetHit;
        }

        public static string TargetGameObjectsToString(TargetGameObject targetGameObject)
        {
            switch (targetGameObject)
            {
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
                case TargetGameObject.Object8:
                    return "Object8";
                case TargetGameObject.Object9:
                    return "Object9";
                case TargetGameObject.Object10:
                    return "Object10";
                case TargetGameObject.Object11:
                    return "Object11";
                case TargetGameObject.Object12:
                    return "Object12";
                case TargetGameObject.Object13:
                    return "Object13";
                case TargetGameObject.Object14:
                    return "Object14";
                case TargetGameObject.Object15:
                    return "Object15";
                case TargetGameObject.Object16:
                    return "Object16";
                case TargetGameObject.Object17:
                    return "Object17";
                case TargetGameObject.Object18:
                    return "Object18";
                case TargetGameObject.Object19:
                    return "Object19";
                case TargetGameObject.Object20:
                    return "Object20";
                case TargetGameObject.Object21:
                    return "Object21";
                case TargetGameObject.Object22:
                    return "Object22";
                case TargetGameObject.Object23:
                    return "Object23";
                case TargetGameObject.Object24:
                    return "Object24";
                case TargetGameObject.Object25:
                    return "Object25";
                case TargetGameObject.Object26:
                    return "Object26";
                case TargetGameObject.Object27:
                    return "Object27";
                case TargetGameObject.Object28:
                    return "Object28";
                case TargetGameObject.Object29:
                    return "Object29";
                case TargetGameObject.Object30:
                    return "Object30";
                case TargetGameObject.Object31:
                    return "Object31";
                case TargetGameObject.Object32:
                    return "Object32";
                case TargetGameObject.Object33:
                    return "Object33";
                case TargetGameObject.Object34:
                    return "Object34";
                case TargetGameObject.Object35:
                    return "Object35";
                case TargetGameObject.Object36:
                    return "Object36";
                case TargetGameObject.Object37:
                    return "Object37";
                case TargetGameObject.Object38:
                    return "Object38";
                case TargetGameObject.Object39:
                    return "Object39";
                case TargetGameObject.Object40:
                    return "Object40";
                case TargetGameObject.Empty:
                    return "Empty";
            }
            return "";
        }

        public static TargetGameObject TargetGameObjectsFromString(string targetGameObjectString)
        {
            switch (targetGameObjectString)
            {
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
                case "Object8":
                    return TargetGameObject.Object8;
                case "Object9":
                    return TargetGameObject.Object9;
                case "Object10":
                    return TargetGameObject.Object10;
                case "Object11":
                    return TargetGameObject.Object11;
                case "Object12":
                    return TargetGameObject.Object12;
                case "Object13":
                    return TargetGameObject.Object13;
                case "Object14":
                    return TargetGameObject.Object14;
                case "Object15":
                    return TargetGameObject.Object15;
                case "Object16":
                    return TargetGameObject.Object16;
                case "Object17":
                    return TargetGameObject.Object17;
                case "Object18":
                    return TargetGameObject.Object18;
                case "Object19":
                    return TargetGameObject.Object19;
                case "Object20":
                    return TargetGameObject.Object20;
                case "Object21":
                    return TargetGameObject.Object21;
                case "Object22":
                    return TargetGameObject.Object22;
                case "Object23":
                    return TargetGameObject.Object23;
                case "Object24":
                    return TargetGameObject.Object24;
                case "Object25":
                    return TargetGameObject.Object25;
                case "Object26":
                    return TargetGameObject.Object26;
                case "Object27":
                    return TargetGameObject.Object27;
                case "Object28":
                    return TargetGameObject.Object28;
                case "Object29":
                    return TargetGameObject.Object29;
                case "Object30":
                    return TargetGameObject.Object30;
                case "Object31":
                    return TargetGameObject.Object31;
                case "Object32":
                    return TargetGameObject.Object32;
                case "Object33":
                    return TargetGameObject.Object33;
                case "Object34":
                    return TargetGameObject.Object34;
                case "Object35":
                    return TargetGameObject.Object35;
                case "Object36":
                    return TargetGameObject.Object36;
                case "Object37":
                    return TargetGameObject.Object37;
                case "Object38":
                    return TargetGameObject.Object38;
                case "Object39":
                    return TargetGameObject.Object39;
                case "Object40":
                    return TargetGameObject.Object40;
            }

            return TargetGameObject.Empty;
        }
    }
    public class OnTargetHitEventargs : EventArgs
    {
        public GameObject TargetGameObject { get; set; }
        public AgentComponent Agent { get; set; }
    }
}