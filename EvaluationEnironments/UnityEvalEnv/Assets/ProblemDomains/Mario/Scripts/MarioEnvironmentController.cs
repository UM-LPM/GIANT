using Base;
using Configuration;
using Spawners;
using System;
using UnityEngine;
using Utils;

namespace Problems.Mario
{
    public class MarioEnvironmentController : EnvironmentControllerBase
    {
        [Header("Mario Movement Configuration")]
        public float MaxSpeed = 8f;
        public float Acceleration = 60f;
        public float Deceleration = 80f;
        public float TurnDeceleration = 100f;
        public float AirControlMultiplier = 0.7f;
        public const float SkinWidth = 0.02f;

        public float JumpVelocity = 22f;
        public float Gravity = 35f;
        public float FallMultiplier = 1.8f;
        public float LowJumpMultiplier = 2.3f;
        public float SpeedJumpBonus = 0.5f;

        public float CoyoteTime = 0.1f;

        private bool finishReached = false;
        private bool isDead = false;

        private MarioMatchSpawner MarioMatchSpawner;
        private FinishComponent FinishComponent;

        // fitness calculation temp variables
        private float timePenalty;
        private string agentFitnessLog;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();

            MarioMatchSpawner = GetComponent<MarioMatchSpawner>();
            FinishComponent = GetComponentInChildren<FinishComponent>();

            if(MarioMatchSpawner == null)
                throw new Exception("MarioEnvironmentController: No MarioMatchSpawner found on the EnvironmentController GameObject.");

            if (FinishComponent == null)
                throw new Exception("MarioEnvironmentController: No FinishComponent found in the EnvironmentController children GameObjects.");
        }

        protected override void OnPostFixedUpdate()
        {
            AgentFellFromPlatform();
        }

        protected override void OnPreFinishGame()
        {
            SetAgentsFitness();
        }

        public override bool IsSimulationFinished()
        {
            return base.IsSimulationFinished() || finishReached || isDead;
        }

        public void AgentFellFromPlatform()
        {
            foreach (MarioAgentComponent agent in Agents)
            {
                if (agent.transform.position.y < (MarioMatchSpawner.AgentSpawnPoint.position.y - 0.5f))
                {
                    OnAgentFellFromPlatform(agent);
                }
            }
        }

        public void OnAgentReachedFinish(MarioAgentComponent agent, FinishComponent finish)
        {
            finishReached = true;
            DebugSystem.LogVerbose($"MarioEnvironmentController: Agent TeamID {agent.TeamIdentifier.TeamID}, ID: {agent.IndividualID} reached the finish!");
        }

        public void OnAgentFellFromPlatform(MarioAgentComponent agent)
        {
            isDead = true;
            DebugSystem.LogVerbose($"MarioEnvironmentController: Agent TeamID {agent.TeamIdentifier.TeamID}, ID: {agent.IndividualID} fell from platform and died!");
        }

        public void SetAgentsFitness()
        {
            foreach (MarioAgentComponent agent in Agents)
            {
                // Time penalty
                timePenalty = isDead? 1f : (CurrentSimulationSteps / (float)SimulationSteps);
                timePenalty = (float)Math.Round(MarioFitness.FitnessValues[MarioFitness.FitnessKeys.TimePenalty.ToString()] * timePenalty, 4);
                agent.AgentFitness.UpdateFitness(timePenalty, MarioFitness.FitnessKeys.TimePenalty.ToString());

                // Distance fitness
                float maxDistance = Math.Abs(MarioMatchSpawner.AgentSpawnPoint.transform.position.x - FinishComponent.transform.position.x);
                float agentDistance = Math.Abs(agent.transform.position.x - FinishComponent.transform.position.x);
                if(agentDistance > maxDistance)
                    agentDistance = maxDistance;
                if(finishReached)
                    agentDistance = 0f;

                float distanceFitness = (1 - (agentDistance / maxDistance)) * Math.Abs(MarioFitness.FitnessValues[MarioFitness.FitnessKeys.Distance.ToString()]);
                distanceFitness = (float)Math.Round(distanceFitness, 4);
                agent.AgentFitness.UpdateFitness(distanceFitness, MarioFitness.FitnessKeys.Distance.ToString());

                agentFitnessLog = "========================================\n" +
                                  $"[Agent]: TeamID {agent.TeamIdentifier.TeamID}, ID: {agent.IndividualID} \n" +
                                  $"[Distance]: {agentDistance} / {maxDistance} = {distanceFitness}\n" +
                                  $"[Time penalty]: {CurrentSimulationSteps} / {SimulationSteps} = {timePenalty}\n";

                DebugSystem.LogVerbose(agentFitnessLog);
            }
        }

        void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;

                MarioFitness.FitnessValues = conf.FitnessValues;

                if (conf.ProblemConfiguration.ContainsKey("MaxSpeed"))
                {
                    MaxSpeed = float.Parse(conf.ProblemConfiguration["MaxSpeed"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("Acceleration"))
                {
                    Acceleration = float.Parse(conf.ProblemConfiguration["Acceleration"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("Deceleration"))
                {
                    Deceleration = float.Parse(conf.ProblemConfiguration["Deceleration"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("TurnDeceleration"))
                {
                    TurnDeceleration = float.Parse(conf.ProblemConfiguration["TurnDeceleration"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("AirControlMultiplier"))
                {
                    AirControlMultiplier = float.Parse(conf.ProblemConfiguration["AirControlMultiplier"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("JumpVelocity"))
                {
                    JumpVelocity = float.Parse(conf.ProblemConfiguration["JumpVelocity"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("Gravity"))
                {
                    Gravity = float.Parse(conf.ProblemConfiguration["Gravity"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("FallMultiplier"))
                {
                    FallMultiplier = float.Parse(conf.ProblemConfiguration["FallMultiplier"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("LowJumpMultiplier"))
                {
                    LowJumpMultiplier = float.Parse(conf.ProblemConfiguration["LowJumpMultiplier"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("SpeedJumpBonus"))
                {
                    SpeedJumpBonus = float.Parse(conf.ProblemConfiguration["SpeedJumpBonus"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("CoyoteTime"))
                {
                    CoyoteTime = float.Parse(conf.ProblemConfiguration["CoyoteTime"]);
                }
            }
        }
    }
}