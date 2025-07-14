using Base;
using static Problems.Soccer.SoccerUtils;
using UnityEngine;
using System;
using Configuration;
using System.Linq;
using UnityEngine.UIElements;

namespace Problems.Soccer
{
    public class SoccerEnvironmentController : EnvironmentControllerBase
    {
        [Header("Soccer General Configuration")]
        [SerializeField] SoccerGameScenarioType GameScenarioType = SoccerGameScenarioType.GoldenGoal;
        [SerializeField] public GameObject SoccerBallPrefab;

        [Header("Soccer Game Configuration")]
        [SerializeField] public int MaxGoals = 10;
        [HideInInspector] public int MaxPasses = -1;

        [Header("Soccer Agent Configuration")]
        [SerializeField] public float ForwardSpeed = 1f;
        [SerializeField] public float LateralSpeed = 1f;
        [SerializeField] public float AgentRunSpeed = 2f;
        [SerializeField] public float AgentRotationSpeed = 100f;
        [SerializeField] public float KickPower = 2000f;
        [SerializeField] public static float VelocityPassTreshold = 0.2f;
        [SerializeField] public float PassTolerance = 10f; // Tolerance in degrees.
        [SerializeField] public float CalculateAgentToBallDistanceEvery = 1f;
        [SerializeField] public float MaxAgentToBallAngle = 30f;
        [SerializeField] public float MaxAgentToGoalAngle = 45;
        [SerializeField] public float BallStartPushForce = 15f;
        [SerializeField] public float CalculateBallToGoalDistanceEvery = 0.5f;

        // Soccer Ball
        SoccerBallSpawner SoccerBallSpawner;
        SoccerBallComponent SoccerBall;

        // Goals
        GoalComponent GoalPurple;
        GoalComponent GoalBlue;

        // Sectors
        private SectorComponent[] Sectors;

        private float timeSinceLastAgentToBallDistanceCalc;
        private float maxAgentToBallDistance;
        private float playgroundDiagonal = 33.54f;

        private float timeSinceLastBallToGoalDistanceCalc;
        private float maxBallToGoalDistance;

        private float distance;

        private Vector3 directionToBall;
        private float angle;

        Vector3 directionToGoal;
        float angleToGoal;

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

            if (SceneLoadMode == SceneLoadMode.LayerMode)
            {
                GoalBlue = FindObjectsByType<GoalComponent>(FindObjectsSortMode.InstanceID).Where(a => a.Team == SoccerTeam.Blue).First();
                GoalPurple = FindObjectsByType<GoalComponent>(FindObjectsSortMode.InstanceID).Where(a => a.Team == SoccerTeam.Purple).First();
            }
            else
            {
                GoalBlue = GetComponentsInChildren<GoalComponent>().Where(a => a.Team == SoccerTeam.Blue).First();
                GoalPurple = GetComponentsInChildren<GoalComponent>().Where(a => a.Team == SoccerTeam.Purple).First();
            }

            SoccerBallSpawner = GetComponent<SoccerBallSpawner>();
            if (SoccerBallSpawner == null)
            {
                throw new Exception("SoccerBallSpawner is not defined");
            }

            if (SceneLoadMode == SceneLoadMode.LayerMode)
            {
                // Only one problem environment exists
                Sectors = FindObjectsOfType<SectorComponent>();
            }
            else
            {
                // Each EnvironmentController contains its own problem environment
                Sectors = GetComponentsInChildren<SectorComponent>();
            }

            if (SimulationSteps > 0)
            {
                MaxPasses = (int)Math.Floor((SimulationSteps * Time.fixedDeltaTime));
            }
            else if(SimulationTime > 0)
            {
                MaxPasses = (int)Math.Floor(SimulationTime);
            }
        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            // Spawn Soccer Ball
            SoccerBall = SoccerBallSpawner.Spawn<SoccerBallComponent>(this)[0];
        }

        protected override void OnPostFixedUpdate()
        {
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
            }
        }

        private void UpdateAgentTimeWithoutGoal()
        {
            foreach (SoccerAgentComponent agent in Agents)
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
            foreach (SoccerAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    distance = Vector3.Distance(agent.transform.position, SoccerBall.transform.position);
                    if(distance > (playgroundDiagonal / 2))
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
            foreach (SoccerAgentComponent agent in Agents)
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

        public void AgentTouchedSoccerBall(SoccerAgentComponent agent)
        {
            GoalComponent goal = agent.Team == SoccerTeam.Blue ? GoalPurple : GoalBlue;
            // Only update if agent intentionaly hit the ball
            if (Mathf.Abs(SoccerBall.Rigidbody.velocity.x) > VelocityPassTreshold)
            {
                Vector3 directionToTarget = (goal.transform.position - SoccerBall.transform.position).normalized;
                float dotProduct = Vector3.Dot(SoccerBall.Rigidbody.velocity.normalized, directionToTarget);

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

        public void GoalScored(SoccerAgentComponent striker, GoalComponent goalComponent)
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
            AgentComponent[] agents = Agents.Where(x => (x as SoccerAgentComponent).Team == goalComponent.Team).ToArray();
            foreach (SoccerAgentComponent agent in agents)
            {
                agent.ResetTimeWithoutGoal();
            }

            // Check engind state
            CheckEndingState();

            if (GameState == GameState.RUNNING)
            {
                // Based on the last team that scored, push the ball towards this side after ball is respawned
                Vector3 pushDirection = (goalComponent.Team == SoccerTeam.Blue ? GoalBlue.transform.position : GoalPurple.transform.position) - SoccerBall.transform.position;
                pushDirection.Normalize();
                SoccerBall.Rigidbody.AddForce(pushDirection * BallStartPushForce, ForceMode.Impulse);
            }
        }

        public override void CheckEndingState()
        {
            if (GameScenarioType == SoccerGameScenarioType.GoldenGoal)
            {
                FinishGame();
            }
            else if (GameScenarioType == SoccerGameScenarioType.Match)
            {
                if(GoalBlue.GoalsReceived >= MaxGoals || GoalPurple.GoalsReceived >= MaxGoals)
                {
                    FinishGame();
                }
                else
                {
                    MatchSpawner.Respawn<SoccerAgentComponent>(this, Agents as SoccerAgentComponent[]);
                    SoccerBallSpawner.Respawn<SoccerBallComponent>(this, SoccerBall);
                }
            }
        }

        protected override void OnPreFinishGame()
        {
            SetAgentsFitness();
        }

        public int TeamGoalsScored(SoccerTeam team)
        {
            return team == SoccerTeam.Blue ? GoalPurple.GoalsReceived : GoalBlue.GoalsReceived;
        }

        public GoalComponent TeamGoalAhead(SoccerAgentComponent agent)
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

        private void SetAgentsFitness()
        {
            foreach (SoccerAgentComponent agent in Agents)
            {
                teamAgentCount = Agents.Where(a => a.TeamIdentifier.TeamID == agent.TeamIdentifier.TeamID).ToArray().Length;
                // Sector exploration
                sectorExplorationFitness = agent.SectorsExplored / (float)Sectors.Length;
                sectorExplorationFitness = (float)Math.Round(SoccerFitness.FitnessValues[SoccerFitness.FitnessKeys.SectorExploration.ToString()] * sectorExplorationFitness, 4);
                sectorExplorationFitness /= teamAgentCount;
                agent.AgentFitness.UpdateFitness(sectorExplorationFitness, SoccerFitness.FitnessKeys.SectorExploration.ToString());

                // Goals scored
                goalsScoredFitness = agent.GoalsScored / (float)SoccerBall.NumOfSpawns;
                goalsScoredFitness = (float)Math.Round(SoccerFitness.FitnessValues[SoccerFitness.FitnessKeys.GoalsScored.ToString()] * goalsScoredFitness, 4);
                agent.AgentFitness.UpdateFitness(goalsScoredFitness, SoccerFitness.FitnessKeys.GoalsScored.ToString());

                // Auto goals scored
                autoGoalsScoredFitness = agent.AutoGoalsScored / (float)SoccerBall.NumOfSpawns;
                autoGoalsScoredFitness = (float)Math.Round(SoccerFitness.FitnessValues[SoccerFitness.FitnessKeys.AutoGoals.ToString()] * autoGoalsScoredFitness, 4);
                agent.AgentFitness.UpdateFitness(autoGoalsScoredFitness, SoccerFitness.FitnessKeys.AutoGoals.ToString());

                // Goals received (Every agents gets portion of the team goals received fitness)
                goalsReceivedFitness = (agent.Team == SoccerTeam.Blue ? GoalBlue.GoalsReceived : GoalPurple.GoalsReceived) / (float)SoccerBall.NumOfSpawns;
                goalsReceivedFitness = (float)Math.Round(SoccerFitness.FitnessValues[SoccerFitness.FitnessKeys.GoalsReceived.ToString()] * goalsReceivedFitness, 4);
                goalsReceivedFitness /= teamAgentCount;
                agent.AgentFitness.UpdateFitness(goalsReceivedFitness, SoccerFitness.FitnessKeys.GoalsReceived.ToString());

                // Passes to oponent goal
                if(agent.PassesToOponentGoal > MaxPasses)
                    agent.PassesToOponentGoal = MaxPasses;

                passesToOponentGoalFitness = agent.PassesToOponentGoal / (float)MaxPasses;
                passesToOponentGoalFitness = (float)Math.Round(SoccerFitness.FitnessValues[SoccerFitness.FitnessKeys.PassesToOponentGoal.ToString()] * passesToOponentGoalFitness, 4);
                agent.AgentFitness.UpdateFitness(passesToOponentGoalFitness, SoccerFitness.FitnessKeys.PassesToOponentGoal.ToString());

                // Passes to own goal
                if (agent.PassesToOwnGoal > MaxPasses)
                    agent.PassesToOwnGoal = MaxPasses;

                passesToOwnGoalFitness = agent.PassesToOwnGoal / (float)MaxPasses;
                passesToOwnGoalFitness = (float)Math.Round(SoccerFitness.FitnessValues[SoccerFitness.FitnessKeys.PassesToOwnGoal.ToString()] * passesToOwnGoalFitness, 4);
                agent.AgentFitness.UpdateFitness(passesToOwnGoalFitness, SoccerFitness.FitnessKeys.PassesToOwnGoal.ToString());

                // Passes
                if (agent.Passes > MaxPasses)
                    agent.Passes = MaxPasses;
                passesFitness = agent.Passes / (float)MaxPasses;
                passesFitness = (float)Math.Round(SoccerFitness.FitnessValues[SoccerFitness.FitnessKeys.Passes.ToString()] * passesFitness, 4);
                agent.AgentFitness.UpdateFitness(passesFitness, SoccerFitness.FitnessKeys.Passes.ToString());

                // Agent to ball distance
                agentToBallDistanceFitness = (maxAgentToBallDistance - agent.AgentToBallDistance) / maxAgentToBallDistance;
                agentToBallDistanceFitness = (float)Math.Round(SoccerFitness.FitnessValues[SoccerFitness.FitnessKeys.AgentToBallDistance.ToString()] * agentToBallDistanceFitness, 4);
                agentToBallDistanceFitness /= teamAgentCount;
                agent.AgentFitness.UpdateFitness(agentToBallDistanceFitness, SoccerFitness.FitnessKeys.AgentToBallDistance.ToString());

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
                ballToGoalDistanceFitness = (float)Math.Round(SoccerFitness.FitnessValues[SoccerFitness.FitnessKeys.BallToOpponentGoalDistance.ToString()] * ballToGoalDistanceFitness, 4);
                ballToGoalDistanceFitness /= teamAgentCount;
                agent.AgentFitness.UpdateFitness(ballToGoalDistanceFitness, SoccerFitness.FitnessKeys.BallToOpponentGoalDistance.ToString());

                // Time without goal bonus
                agent.ResetTimeWithoutGoal();

                timeWithoutGoalBonusFitness = agent.MaxTimeWithoutGoal / (float)CurrentSimulationSteps;
                timeWithoutGoalBonusFitness = (float)Math.Round(SoccerFitness.FitnessValues[SoccerFitness.FitnessKeys.TimeWithoutGoalBonus.ToString()] * timeWithoutGoalBonusFitness, 4);
                timeWithoutGoalBonusFitness /= teamAgentCount;
                agent.AgentFitness.UpdateFitness(timeWithoutGoalBonusFitness, SoccerFitness.FitnessKeys.TimeWithoutGoalBonus.ToString());

                // Time looking at ball
                timeLookingAtBallFitness = agent.TimeLookingAtBall / (float)CurrentSimulationTime;
                timeLookingAtBallFitness = (float)Math.Round(SoccerFitness.FitnessValues[SoccerFitness.FitnessKeys.TimeLookingAtBall.ToString()] * timeLookingAtBallFitness, 4);
                timeLookingAtBallFitness /= teamAgentCount;
                agent.AgentFitness.UpdateFitness(timeLookingAtBallFitness, SoccerFitness.FitnessKeys.TimeLookingAtBall.ToString());

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

                Debug.Log(agentFitnessLog);

            }
        }

        public void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;

                SoccerFitness.FitnessValues = conf.FitnessValues;

                if (conf.ProblemConfiguration.ContainsKey("AgentRunSpeed"))
                {
                    AgentRunSpeed = float.Parse(conf.ProblemConfiguration["AgentRunSpeed"]);
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