using UnityEngine;
using System;
using System.Collections.Generic;
using Base;
using System.Linq;

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

    public enum RayHitObjectDetectionType
    {
        RayIndex,
        RaySide
    }

    public enum ObjectTeamType
    {
        Default,
        Teammate,
        Opponent
    }

    public class RayHitObject : ConditionNode
    {
        public static RayHitObjectDetectionType RAY_HIT_OBJECT_DETECTION_TYPE = RayHitObjectDetectionType.RayIndex;
        public static event EventHandler<OnTargetHitEventargs> OnTargetHit;

        public int targetGameObject;
        public AgentSideAdvanced side;
        public int rayIndex;
        public ObjectTeamType targetTeamType;
        public RaySensorBase raySensor;

        private SensorPerceiveOutput[] sensorPerceiveOutputs;
        private bool targetHit;

        private TeamIdentifier baseGameObjectTeam;
        private TeamIdentifier targetGameObjectTeam;

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
                raySensor.SetLayerMask((1 << context.gameObject.layer));

                GetBaseGameObjectTeam();
            }

            targetHit = false;

            if (RAY_HIT_OBJECT_DETECTION_TYPE == RayHitObjectDetectionType.RayIndex)
            {
                // Option 1 : Check if the target game object is hit by the single ray based on RayIndex
                sensorPerceiveOutputs = raySensor.PerceiveSingle(xPos: rayIndex);

                if (sensorPerceiveOutputs[rayIndex].HasHit && sensorPerceiveOutputs[rayIndex].HitGameObjects[0].tag.Contains(TargetGameObjects[targetGameObject]) && TargetTeamHit(sensorPerceiveOutputs[rayIndex].HitGameObjects[0]))
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
                    if (sensorPerceiveOutputs[0].HasHit && sensorPerceiveOutputs[0].HitGameObjects[0].tag.Contains(TargetGameObjects[targetGameObject]) && TargetTeamHit(sensorPerceiveOutputs[0].HitGameObjects[0]))
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
                        if (sensorPerceiveOutputs[i].HasHit && sensorPerceiveOutputs[i].HitGameObjects[0].tag.Contains(TargetGameObjects[targetGameObject]) && TargetTeamHit(sensorPerceiveOutputs[i].HitGameObjects[0]))
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
                        if (sensorPerceiveOutputs[i].HasHit && sensorPerceiveOutputs[i].HitGameObjects[0].tag.Contains(TargetGameObjects[targetGameObject]) && TargetTeamHit(sensorPerceiveOutputs[i].HitGameObjects[0]))
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

        public void GetBaseGameObjectTeam()
        {
            // Search for TeamIdentifier components in base game object
            baseGameObjectTeam = context.gameObject.GetComponent<TeamIdentifier>();

            // If base game object doesn't contain the component try to find it in children game objects
            if(baseGameObjectTeam == null)
                baseGameObjectTeam = context.gameObject.GetComponentInChildren<TeamIdentifier>();
        }

        public bool TargetTeamHit(GameObject hitGameObject)
        {
            if (baseGameObjectTeam != null)
            {
                targetGameObjectTeam = hitGameObject.GetComponent<TeamIdentifier>();

                if(targetGameObjectTeam != null)
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
    public class OnTargetHitEventargs : EventArgs
    {
        public GameObject TargetGameObject { get; set; }
        public AgentComponent Agent { get; set; }
    }
}