using Base;
using Configuration;
using Problems.Soccer;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Utils;
using static Problems.Soccer2D.Soccer2DUtils;

namespace Problems.Soccer2D
{
    public class Soccer2DEnvironmentController : EnvironmentControllerBase
    {
        [Header("Soccer General Configuration")]
        [SerializeField] SoccerGameScenarioType GameScenarioType = SoccerGameScenarioType.GoldenGoal;
        [SerializeField] public GameObject SoccerBallPrefab;

        [Header("Soccer Game Configuration")]
        [SerializeField] public int MaxGoals = 10;
        [HideInInspector] public int MaxPasses = -1;

        [Header("Soccer Ball Configuration")]
        [SerializeField] public float BallBounceFactor = 0.8f;
        [SerializeField] public float BallCollisionCheckRadius = 0.05f;
        [SerializeField] public float BallDampingFactor = 0.995f;
        [SerializeField] public float BallMinVelocityThreshold = 0.01f;
        [SerializeField] public float BallMaxVelocity = 15f;

        [Header("Soccer Agent Configuration")]
        [SerializeField] public float ForwardSpeed = 1f;
        [SerializeField] public float LateralSpeed = 1f;
        [SerializeField] public float AgentAcceleration = 5f;
        [SerializeField] public float AgentMaxAcceleration = 10f;
        [SerializeField] public float AgentMoveDamping = 0.9f;
        [SerializeField] public float AgentRotationSpeed = 100f;
        [SerializeField] public float KickPower = 15f;
        [SerializeField] public static float VelocityPassTreshold = 0.2f;
        [SerializeField] public float PassTolerance = 10f; // Tolerance in degrees.
        [SerializeField] public float CalculateAgentToBallDistanceEvery = 1f;
        [SerializeField] public float MaxAgentToBallAngle = 30f;
        [SerializeField] public float MaxAgentToGoalAngle = 45;
        [SerializeField] public float BallStartPushForce = 1f;
        [SerializeField] public float CalculateBallToGoalDistanceEvery = 0.5f;

        private bool NeedsRespawn = false;
        private bool maxGoalsScored = false;

        // Soccer Ball
        Soccer2DBallSpawner SoccerBallSpawner;
        Soccer2DSoccerBallComponent SoccerBall;

        // Goals
        Soccer2DGoalComponent GoalPurple;
        Soccer2DGoalComponent GoalBlue;
        Soccer2DGoalComponent[] Goals;

        SectorComponent[] Sectors;

        Vector3 soccerBallPushDirection;
        Soccer2DGoalComponent receivedGoalComponent;

        Soccer2DGoalComponent lastReceivedGoalComponent;

        float randomAngle;


        private float timeSinceLastAgentToBallDistanceCalc;
        private float maxAgentToBallDistance;
        private float playgroundDiagonal = 17.493f;

        private float timeSinceLastBallToGoalDistanceCalc;
        private float maxBallToGoalDistance;

        private float distance;

        private Vector3 directionToBall;
        private float angle;

        Vector3 directionToGoal;
        float angleToGoal;

        Soccer2DAgentComponent agent;
        Vector3 sectorPosition;

        // Fitness calculation
        private float sectorExplorationFitness;
        private float goalsScoredFitness;
        private float autoGoalsScoredFitness;
        private float goalsReceivedFitness;
        private float passesToOponentGoalFitness;
        private float passesToOwnGoalFitness;
        private float passesFitness;
        private float agentToBallDistanceFitness;
        private float ballToGoalDistanceFitness;
        private float timeWithoutGoalBonusFitness;
        private float timeLookingAtBallFitness;

        private string agentFitnessLog;
        private int teamAgentCount;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();

            GoalBlue = GetComponentsInChildren<Soccer2DGoalComponent>().Where(a => a.Team == SoccerTeam.Blue).First();
            GoalPurple = GetComponentsInChildren<Soccer2DGoalComponent>().Where(a => a.Team == SoccerTeam.Purple).First();
            Goals = new Soccer2DGoalComponent[] { GoalBlue, GoalPurple };

            SoccerBallSpawner = GetComponent<Soccer2DBallSpawner>();
            if (SoccerBallSpawner == null)
            {
                throw new Exception("SoccerBallSpawner is not defined");
            }

            Sectors = GetComponentsInChildren<SectorComponent>();

            if (SimulationSteps > 0)
            {
                MaxPasses = (int)Math.Floor((SimulationSteps * Time.fixedDeltaTime));
            }
            else if (SimulationTime > 0)
            {
                MaxPasses = (int)Math.Floor(SimulationTime);
            }
        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            // Spawn Soccer Ball
            SoccerBall = SoccerBallSpawner.Spawn<Soccer2DSoccerBallComponent>(this)[0];

            BallDampingFactor = Mathf.Clamp01(BallDampingFactor);
            BallDampingFactor = Mathf.Max(BallDampingFactor);
        }

        protected override void OnPreFixedUpdate()
        {
            CheckIfGoalScored();

            if (NeedsRespawn)
            {
                MatchSpawner.Respawn<Soccer2DAgentComponent>(this, Agents as Soccer2DAgentComponent[]);
                SoccerBallSpawner.Respawn<Soccer2DSoccerBallComponent>(this, SoccerBall);
                ForceNewDecisions = true;
                NeedsRespawn = false;

                if (GameState == GameState.RUNNING)
                {
                    // Based on the last team that scored, push the ball towards this side after ball is respawned
                    soccerBallPushDirection = (receivedGoalComponent.Team == SoccerTeam.Blue ? GoalBlue.transform.position : GoalPurple.transform.position) - SoccerBall.transform.position;
                    soccerBallPushDirection.Normalize();

                    // Based on the direction, select a random angle between -30 and 30 degrees to add some noise to the push direction
                    randomAngle = Util.Rnd.Next(-30, 30);
                    soccerBallPushDirection = Quaternion.Euler(0, randomAngle, 0) * soccerBallPushDirection;

                    SoccerBall.AddForce(soccerBallPushDirection * BallStartPushForce);
                }
            }
        }

        protected override void OnPostFixedUpdate()
        {
            SoccerBall.OnStep();

            if (GameState == GameState.RUNNING)
            {
                UpdateAgentTimeWithoutGoal();

                // Agent to ball distance calculation
                timeSinceLastAgentToBallDistanceCalc += Time.fixedDeltaTime;

                if (timeSinceLastAgentToBallDistanceCalc >= CalculateAgentToBallDistanceEvery)
                {
                    UpdateAgentProxityToBall();
                    timeSinceLastAgentToBallDistanceCalc = 0f;
                }

                // Ball to goal distance calculation
                timeSinceLastBallToGoalDistanceCalc += Time.fixedDeltaTime;
                if (timeSinceLastBallToGoalDistanceCalc >= CalculateBallToGoalDistanceEvery)
                {
                    UpdateBallToGoalProximity();

                    timeSinceLastBallToGoalDistanceCalc = 0f;
                }

                UpdateTimeLookingAtBall();

                CheckAgentsExploration();
            }

        }

        private void CheckAgentsExploration()
        {
            // Exploration bonus
            for (int i = 0; i < Agents.Length; i++)
            {
                agent = Agents[i] as Soccer2DAgentComponent;
                if (agent.gameObject.activeSelf)
                {
                    foreach (SectorComponent sector in Sectors)
                    {
                        sectorPosition = sector.transform.position;
                        if (IsAgentInSector(agent.transform.position, sector.gameObject.GetComponent<Collider2D>()))
                        {
                            if (agent.LastSectorPosition == null || agent.LastSectorPosition != sectorPosition)
                            {
                                if (!agent.LastKnownSectorPositions.Contains(sectorPosition))
                                {
                                    // Agent explored new sector
                                    agent.SectorsExplored++;

                                    agent.LastKnownSectorPositions.Add(sectorPosition);
                                }

                                agent.LastSectorPosition = sector.transform.position;
                            }

                            // Agent can only be in one sector at once
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateAgentTimeWithoutGoal()
        {
            foreach (Soccer2DAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    agent.CurrentTimeWithoutGoal++;
                }
            }
        }

        private void UpdateAgentProxityToBall()
        {
            maxAgentToBallDistance += (playgroundDiagonal / 2);
            foreach (Soccer2DAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    distance = Vector3.Distance(agent.transform.position, SoccerBall.transform.position);
                    if (distance > (playgroundDiagonal / 2))
                        distance = playgroundDiagonal / 2;

                    agent.AgentToBallDistance += distance;
                }
            }
        }

        public void UpdateBallToGoalProximity()
        {
            maxBallToGoalDistance += (playgroundDiagonal / 2);
            distance = Vector3.Distance(SoccerBall.transform.position, GoalBlue.transform.position);
            if (distance > (playgroundDiagonal / 2))
                distance = playgroundDiagonal / 2;
            SoccerBall.BallToBlueGoalDistance += distance;

            distance = Vector3.Distance(SoccerBall.transform.position, GoalPurple.transform.position);
            if (distance > (playgroundDiagonal / 2))
                distance = playgroundDiagonal / 2;
            SoccerBall.BallToPurpleGoalDistance += distance;
        }

        public void UpdateTimeLookingAtBall()
        {
            foreach (Soccer2DAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    directionToBall = (SoccerBall.transform.position - agent.transform.position).normalized;
                    angle = Vector3.Angle(agent.transform.forward, directionToBall);
                    if (angle < MaxAgentToBallAngle)
                    {
                        agent.TimeLookingAtBall += Time.fixedDeltaTime;
                    }
                }
            }
        }

        public void AgentTouchedSoccerBall(Soccer2DAgentComponent agent)
        {
            Soccer2DGoalComponent goal = agent.Team == SoccerTeam.Blue ? GoalPurple : GoalBlue;
            // Only update if agent intentionaly hit the ball
            if (Mathf.Abs(SoccerBall.GetVelocity().x) > VelocityPassTreshold)
            {
                Vector3 directionToTarget = (goal.transform.position - SoccerBall.transform.position).normalized;
                float dotProduct = Vector3.Dot(SoccerBall.GetVelocity().normalized, directionToTarget);

                if (dotProduct > PassTolerance)
                {
                    // The ball is moving towards the target
                    agent.PassesToOponentGoal++;
                }
                else if (dotProduct < -PassTolerance)
                {
                    // The object is not moving towards the target
                    agent.PassesToOwnGoal++;
                }
                else
                {
                    // The object is not moving in any dirrection
                    agent.Passes++;
                }
            }
        }

        public void GoalScored(Soccer2DAgentComponent striker, Soccer2DGoalComponent goalComponent)
        {
            goalComponent.GoalsReceived++;

            if (striker.Team == goalComponent.Team)
            {
                // Autogol was scored
                striker.AutoGoalsScored++;
            }
            else
            {
                // Legit goal was scored
                striker.GoalsScored++;
            }

            // ResetTimeWithoutGoal for all agents in the team
            AgentComponent[] agents = Agents.Where(x => (x as Soccer2DAgentComponent).Team == goalComponent.Team).ToArray();
            foreach (Soccer2DAgentComponent agent in agents)
            {
                agent.ResetTimeWithoutGoal();
            }

            receivedGoalComponent = goalComponent;

            // Check engind state
            CheckEndingState();

            if (GameState == GameState.RUNNING)
            {
                // Based on the last team that scored, push the ball towards this side after ball is respawned
                soccerBallPushDirection = (goalComponent.Team == SoccerTeam.Blue ? GoalBlue.transform.position : GoalPurple.transform.position) - SoccerBall.transform.position;
                soccerBallPushDirection.Normalize();
            }
        }

        public override void CheckEndingState()
        {
            if (GameScenarioType == SoccerGameScenarioType.GoldenGoal)
            {
                maxGoalsScored = true;
            }
            else if (GameScenarioType == SoccerGameScenarioType.Match)
            {
                if (GoalBlue.GoalsReceived >= MaxGoals || GoalPurple.GoalsReceived >= MaxGoals)
                {
                    maxGoalsScored = true;
                }
                else
                {
                    NeedsRespawn = true;
                }
            }
        }

        public override bool IsSimulationFinished()
        {
            return base.IsSimulationFinished() || maxGoalsScored;
        }

        protected override void OnPreFinishGame()
        {
            SetAgentsFitness();
        }

        public Soccer2DGoalComponent TeamGoalAhead(Soccer2DAgentComponent agent)
        {
            // Return teams goal if agent is facing it
            directionToGoal = (agent.Team == SoccerTeam.Blue ? GoalBlue.transform.position : GoalPurple.transform.position) - agent.transform.position;
            directionToGoal.Normalize();
            angleToGoal = Vector3.Angle(agent.transform.forward, directionToGoal);
            if (angleToGoal < MaxAgentToGoalAngle)
            {
                return agent.Team == SoccerTeam.Blue ? GoalBlue : GoalPurple;
            }

            // Return opponent goal if agent is facing it
            directionToGoal = (agent.Team == SoccerTeam.Blue ? GoalPurple.transform.position : GoalBlue.transform.position) - agent.transform.position;
            directionToGoal.Normalize();
            angleToGoal = Vector3.Angle(agent.transform.forward, directionToGoal);
            if (angleToGoal < MaxAgentToGoalAngle)
            {
                return agent.Team == SoccerTeam.Blue ? GoalPurple : GoalBlue;
            }

            // Return null if agent is not facing any goal
            return null;
        }

        private bool IsAgentInSector(Vector3 agentPosition, Collider2D colliderComponent)
        {
            if (colliderComponent.bounds.Contains(agentPosition))
            {
                return true;
            }

            return false;
        }

        private bool CheckIfGoalScored()
        {
            lastReceivedGoalComponent = PhysicsUtil.PhysicsOverlapSphereTargetObject<Soccer2DGoalComponent> (
                    PhysicsScene,
                    PhysicsScene2D,
                    GameType,
                    SoccerBall.gameObject,
                    SoccerBall.transform.position,
                    SoccerBall.Radius,
                    false,
                    SoccerBall.gameObject.layer);

            if (lastReceivedGoalComponent != null)
            {
                GoalScored(SoccerBall.LastTouchedAgent, lastReceivedGoalComponent);
                return true;
            }

            return false;
        }

        private void SetAgentsFitness()
        {
            foreach (Soccer2DAgentComponent agent in Agents)
            {
                teamAgentCount = Agents.Where(a => a.TeamIdentifier.TeamID == agent.TeamIdentifier.TeamID).ToArray().Length;
                // Sector exploration
                sectorExplorationFitness = agent.SectorsExplored / (float)Sectors.Length;
                sectorExplorationFitness = (float)Math.Round(Soccer2DFitness.FitnessValues[Soccer2DFitness.FitnessKeys.SectorExploration.ToString()] * sectorExplorationFitness, 4);
                sectorExplorationFitness /= teamAgentCount;
                agent.AgentFitness.UpdateFitness(sectorExplorationFitness, Soccer2DFitness.FitnessKeys.SectorExploration.ToString());

                // Goals scored
                goalsScoredFitness = agent.GoalsScored / (float)SoccerBall.NumOfSpawns;
                goalsScoredFitness = (float)Math.Round(Soccer2DFitness.FitnessValues[Soccer2DFitness.FitnessKeys.GoalsScored.ToString()] * goalsScoredFitness, 4);
                agent.AgentFitness.UpdateFitness(goalsScoredFitness, Soccer2DFitness.FitnessKeys.GoalsScored.ToString());

                // Auto goals scored
                autoGoalsScoredFitness = agent.AutoGoalsScored / (float)SoccerBall.NumOfSpawns;
                autoGoalsScoredFitness = (float)Math.Round(Soccer2DFitness.FitnessValues[Soccer2DFitness.FitnessKeys.AutoGoals.ToString()] * autoGoalsScoredFitness, 4);
                agent.AgentFitness.UpdateFitness(autoGoalsScoredFitness, Soccer2DFitness.FitnessKeys.AutoGoals.ToString());

                // Goals received (Every agents gets portion of the team goals received fitness)
                goalsReceivedFitness = (agent.Team == SoccerTeam.Blue ? GoalBlue.GoalsReceived : GoalPurple.GoalsReceived) / (float)SoccerBall.NumOfSpawns;
                goalsReceivedFitness = (float)Math.Round(Soccer2DFitness.FitnessValues[Soccer2DFitness.FitnessKeys.GoalsReceived.ToString()] * goalsReceivedFitness, 4);
                goalsReceivedFitness /= teamAgentCount;
                agent.AgentFitness.UpdateFitness(goalsReceivedFitness, Soccer2DFitness.FitnessKeys.GoalsReceived.ToString());

                // Passes to oponent goal
                if (agent.PassesToOponentGoal > MaxPasses)
                    agent.PassesToOponentGoal = MaxPasses;

                passesToOponentGoalFitness = agent.PassesToOponentGoal / (float)MaxPasses;
                passesToOponentGoalFitness = (float)Math.Round(Soccer2DFitness.FitnessValues[Soccer2DFitness.FitnessKeys.PassesToOponentGoal.ToString()] * passesToOponentGoalFitness, 4);
                agent.AgentFitness.UpdateFitness(passesToOponentGoalFitness, Soccer2DFitness.FitnessKeys.PassesToOponentGoal.ToString());

                // Passes to own goal
                if (agent.PassesToOwnGoal > MaxPasses)
                    agent.PassesToOwnGoal = MaxPasses;

                passesToOwnGoalFitness = agent.PassesToOwnGoal / (float)MaxPasses;
                passesToOwnGoalFitness = (float)Math.Round(Soccer2DFitness.FitnessValues[Soccer2DFitness.FitnessKeys.PassesToOwnGoal.ToString()] * passesToOwnGoalFitness, 4);
                agent.AgentFitness.UpdateFitness(passesToOwnGoalFitness, Soccer2DFitness.FitnessKeys.PassesToOwnGoal.ToString());

                // Passes
                if (agent.Passes > MaxPasses)
                    agent.Passes = MaxPasses;
                passesFitness = agent.Passes / (float)MaxPasses;
                passesFitness = (float)Math.Round(Soccer2DFitness.FitnessValues[Soccer2DFitness.FitnessKeys.Passes.ToString()] * passesFitness, 4);
                agent.AgentFitness.UpdateFitness(passesFitness, Soccer2DFitness.FitnessKeys.Passes.ToString());

                // Agent to ball distance
                agentToBallDistanceFitness = (maxAgentToBallDistance - agent.AgentToBallDistance) / maxAgentToBallDistance;
                agentToBallDistanceFitness = (float)Math.Round(Soccer2DFitness.FitnessValues[Soccer2DFitness.FitnessKeys.AgentToBallDistance.ToString()] * agentToBallDistanceFitness, 4);
                agentToBallDistanceFitness /= teamAgentCount;
                agent.AgentFitness.UpdateFitness(agentToBallDistanceFitness, Soccer2DFitness.FitnessKeys.AgentToBallDistance.ToString());

                // Ball to goal opponent goal distance
                if (agent.Team == SoccerTeam.Blue)
                {
                    distance = SoccerBall.BallToPurpleGoalDistance;
                }
                else
                {
                    distance = SoccerBall.BallToBlueGoalDistance;
                }

                ballToGoalDistanceFitness = (maxBallToGoalDistance - distance) / maxBallToGoalDistance;
                ballToGoalDistanceFitness = (float)Math.Round(Soccer2DFitness.FitnessValues[Soccer2DFitness.FitnessKeys.BallToOpponentGoalDistance.ToString()] * ballToGoalDistanceFitness, 4);
                ballToGoalDistanceFitness /= teamAgentCount;
                agent.AgentFitness.UpdateFitness(ballToGoalDistanceFitness, Soccer2DFitness.FitnessKeys.BallToOpponentGoalDistance.ToString());

                // Time without goal bonus
                agent.ResetTimeWithoutGoal();

                timeWithoutGoalBonusFitness = agent.MaxTimeWithoutGoal / (float)CurrentSimulationSteps;
                timeWithoutGoalBonusFitness = (float)Math.Round(Soccer2DFitness.FitnessValues[Soccer2DFitness.FitnessKeys.TimeWithoutGoalBonus.ToString()] * timeWithoutGoalBonusFitness, 4);
                timeWithoutGoalBonusFitness /= teamAgentCount;
                agent.AgentFitness.UpdateFitness(timeWithoutGoalBonusFitness, Soccer2DFitness.FitnessKeys.TimeWithoutGoalBonus.ToString());

                // Time looking at ball
                timeLookingAtBallFitness = agent.TimeLookingAtBall / (float)CurrentSimulationTime;
                timeLookingAtBallFitness = (float)Math.Round(Soccer2DFitness.FitnessValues[Soccer2DFitness.FitnessKeys.TimeLookingAtBall.ToString()] * timeLookingAtBallFitness, 4);
                timeLookingAtBallFitness /= teamAgentCount;
                agent.AgentFitness.UpdateFitness(timeLookingAtBallFitness, Soccer2DFitness.FitnessKeys.TimeLookingAtBall.ToString());

                agentFitnessLog = "========================================\n" +
                                  $"[Agent]: Team ID {agent.TeamIdentifier.TeamID}, ID: {agent.IndividualID}\n" +
                                  $"[Sectors explored]: {agent.SectorsExplored} / {Sectors.Length} = {sectorExplorationFitness}\n" +
                                  $"[Goals scored]: {agent.GoalsScored} / {MaxGoals} = {goalsScoredFitness}\n" +
                                  $"[Auto goals scored]: {agent.AutoGoalsScored} / {MaxGoals} = {autoGoalsScoredFitness}\n" +
                                  $"[Goals received]: {(agent.Team == SoccerTeam.Blue ? GoalBlue.GoalsReceived : GoalPurple.GoalsReceived)} / {MaxGoals} = {goalsReceivedFitness}\n" +
                                  $"[Passes to oponent goal]: {agent.PassesToOponentGoal} / {MaxPasses} = {passesToOponentGoalFitness}\n" +
                                  $"[Passes]:  {agent.Passes} / {MaxPasses} = {passesFitness}\n" +
                                  $"[Passes to own goal]: {agent.PassesToOwnGoal} / {MaxPasses} = {passesToOwnGoalFitness}\n" +
                                  $"[Agent to ball distance]: {maxAgentToBallDistance - agent.AgentToBallDistance} / {maxAgentToBallDistance} = {agentToBallDistanceFitness}\n" +
                                  $"[Ball to goal distance]: {maxBallToGoalDistance - distance} / {maxBallToGoalDistance} = {ballToGoalDistanceFitness}\n" +
                                  $"[Time without goal bonus]: {agent.MaxTimeWithoutGoal} / {CurrentSimulationSteps} = {timeWithoutGoalBonusFitness}\n" +
                                  $"[Time looking at ball]: {agent.TimeLookingAtBall} / {CurrentSimulationSteps} = {timeLookingAtBallFitness}";

                DebugSystem.LogVerbose(agentFitnessLog);

            }
        }

        public void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;

                Soccer2DFitness.FitnessValues = conf.FitnessValues;

                if (conf.ProblemConfiguration.ContainsKey("AgentAcceleration"))
                {
                    AgentAcceleration = float.Parse(conf.ProblemConfiguration["AgentAcceleration"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentMaxAcceleration"))
                {
                    AgentMaxAcceleration = float.Parse(conf.ProblemConfiguration["AgentMaxAcceleration"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentMoveDamping"))
                {
                    AgentMoveDamping = float.Parse(conf.ProblemConfiguration["AgentMoveDamping"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentRotationSpeed"))
                {
                    AgentRotationSpeed = float.Parse(conf.ProblemConfiguration["AgentRotationSpeed"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("KickPower"))
                {
                    KickPower = float.Parse(conf.ProblemConfiguration["KickPower"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("GameScenarioType"))
                {
                    GameScenarioType = (SoccerGameScenarioType)int.Parse(conf.ProblemConfiguration["GameScenarioType"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("PassTolerance"))
                {
                    PassTolerance = float.Parse(conf.ProblemConfiguration["PassTolerance"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MaxGoals"))
                {
                    MaxGoals = int.Parse(conf.ProblemConfiguration["MaxGoals"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("BallBounceFactor"))
                {
                    BallBounceFactor = float.Parse(conf.ProblemConfiguration["BallBounceFactor"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("BallCollisionCheckRadius"))
                {
                    BallCollisionCheckRadius = float.Parse(conf.ProblemConfiguration["BallCollisionCheckRadius"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("BallDampingFactor"))
                {
                    BallDampingFactor = float.Parse(conf.ProblemConfiguration["BallDampingFactor"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("BallMinVelocityThreshold"))
                {
                    BallMinVelocityThreshold = float.Parse(conf.ProblemConfiguration["BallMinVelocityThreshold"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("BallMaxVelocity"))
                {
                    BallMaxVelocity = float.Parse(conf.ProblemConfiguration["BallMaxVelocity"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("CalculateAgentToBallDistanceEvery"))
                {
                    CalculateAgentToBallDistanceEvery = float.Parse(conf.ProblemConfiguration["CalculateAgentToBallDistanceEvery"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MaxAgentToBallAngle"))
                {
                    MaxAgentToBallAngle = float.Parse(conf.ProblemConfiguration["MaxAgentToBallAngle"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MaxAgentToGoalAngle"))
                {
                    MaxAgentToGoalAngle = float.Parse(conf.ProblemConfiguration["MaxAgentToGoalAngle"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("BallStartPushForce"))
                {
                    BallStartPushForce = float.Parse(conf.ProblemConfiguration["BallStartPushForce"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("CalculateBallToGoalDistanceEvery"))
                {
                    CalculateBallToGoalDistanceEvery = float.Parse(conf.ProblemConfiguration["CalculateBallToGoalDistanceEvery"]);
                }
            }
        }
    }
}