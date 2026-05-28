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

        private AgentComponent agentComponent;
        private TeamIdentifier baseGameObjectTeam;
        private TeamIdentifier targetGameObjectTeam;

        public RayHitObject(RayHitObject other) : base(other)
        {
            if (other == null)
                return;

            this.targetGameObject = other.targetGameObject;
            this.side = other.side;
            this.rayIndex = other.rayIndex;
            this.targetTeamType = other.targetTeamType;
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override bool CheckConditions()
        {
            //return Coordinator.Instance.Random.NextDouble() < 0.5; // Temporary random condition for testing purposes, replace with actual ray hit logic below

            if (raySensor == null)
            {
                raySensor = context.gameObject.GetComponentInChildren<RaySensorBase>();
                raySensor.SetLayerMask((1 << context.gameObject.layer));
                agentComponent = context.gameObject.GetComponent<AgentComponent>();
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
                    OnTargetHit?.Invoke(this, new OnTargetHitEventargs { TargetGameObject = sensorPerceiveOutputs[rayIndex].HitGameObjects[0], Agent = agentComponent });
                }
            }
            else if (RAY_HIT_OBJECT_DETECTION_TYPE == RayHitObjectDetectionType.RaySide)
            {
                // Option 2 : Check if the target game object is hit by any of the rays based on Side
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
                    sensorPerceiveOutputs = raySensor.PerceiveRange(2, raySensor.RayCount, 2);
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
                    sensorPerceiveOutputs = raySensor.PerceiveRange(1, raySensor.RayCount, 2);
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
                    OnTargetHit?.Invoke(this, new OnTargetHitEventargs { TargetGameObject = sensorPerceiveOutputs[hitIndex].HitGameObjects[0], Agent = agentComponent });
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
            if (targetTeamType == ObjectTeamType.Default || baseGameObjectTeam == null)
                return true;

            targetGameObjectTeam = hitGameObject.GetComponent<TeamIdentifier>();
            if (targetGameObjectTeam == null)
                return true;

            return targetTeamType == ObjectTeamType.Teammate
                ? baseGameObjectTeam.TeamID == targetGameObjectTeam.TeamID
                : baseGameObjectTeam.TeamID != targetGameObjectTeam.TeamID;
        }

        public override BTNode Clone()
        {
            return new RayHitObject(this);
        }
    }
    public class OnTargetHitEventargs : EventArgs
    {
        public GameObject TargetGameObject { get; set; }
        public AgentComponent Agent { get; set; }
    }
}