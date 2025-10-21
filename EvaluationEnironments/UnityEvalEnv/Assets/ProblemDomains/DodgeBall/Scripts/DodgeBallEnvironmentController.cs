using Base;
using Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace Problems.DodgeBall
{
    public class DodgeBallEnvironmentController : EnvironmentControllerBase
    {
        [Header("DodgeBall Game Configuration")]
        [SerializeField] public BallPickUpMode BallPickUpMode = BallPickUpMode.Automatic;

        [Header("DodgeBall Agent Configuration")]
        [SerializeField] public float ForwardSpeed = 1f;
        [SerializeField] public float LateralSpeed = 1f;
        [SerializeField] public float AgentRunSpeed = 1f;
        [SerializeField] public float AgentRotationSpeed = 100f;
        [SerializeField] public float AgentThrowPower = 40f;
        [SerializeField] public float AgentBallPickupRadius = 1f;
        [SerializeField] public float AgentBallInterceptProbability = 0.6f; // The probability of an agent intercepting a ball thrown at them
        [SerializeField] public float AgentBallThrowCooldown = 0.5f; // Cooldown time between throws
        [SerializeField] public float MaxBallToOpponentAngle = 45f; // The maximum angle between the throw direction and the opponent's position to consider them hit
        [SerializeField] public float CalculateAgentToBallDistanceEvery = 1f;

        [Header("Ball configuration")]
        [SerializeField] public GameObject BallPrefab;

        private bool NeedsRespawn = false;

        private DodgeBallBallSpawner BallSpawner;
        private DodgeBallBallComponent[] Balls;

        // Sectors
        private SectorComponent[] Sectors;

        // Temp variables
        private DodgeBallBallComponent closestBallInRange;
        private float distanceToClosestBall = float.MaxValue;
        private float distance;
        private Vector3 nextPosition;
        private Vector3 directionToOpponent;
        private float angle;

        private float timeSinceLastAgentToBallDistanceCalc;
        private float maxAgentToBallDistance;
        private float playgroundDiagonal = 18.317f;

        // Fitness calculation
        private float sectorExplorationFitness;
        private float ballsPickedUpFitness;
        private float ballsThrownFitness;
        private float ballsThrownAtOpponent;
        private float ballsInterceptedFitness;
        private float opponentsHitFitness;
        private float ballsHitByFitness;
        private float survivalBonusFitness;
        private float agentToBallDistanceFitness;

        private string agentFitnessLog;

        private int numOfOpponents;

        DodgeBallAgentComponent hitAgent;
        DodgeBallBallComponent ball;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();

            BallSpawner = GetComponent<DodgeBallBallSpawner>();
            if (BallSpawner == null)
            {
                throw new Exception("BallSpawner is not defined");
            }

            Sectors = GetComponentsInChildren<SectorComponent>();
        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            Balls = BallSpawner.Spawn<DodgeBallBallComponent>(this);
        }

        protected override void OnPreFixedUpdate()
        {
            if (NeedsRespawn)
            {
                ForceNewDecisions = true;
                NeedsRespawn = false;

                MatchSpawner.Respawn<DodgeBallAgentComponent>(this, hitAgent);
                ball.ResetBall();
            }
        }

        protected override void OnPostFixedUpdate()
        {
            if (GameState == GameState.RUNNING)
            {
                UpdateAgentsSurvivalTime();

                timeSinceLastAgentToBallDistanceCalc += Time.fixedDeltaTime;

                if (timeSinceLastAgentToBallDistanceCalc >= CalculateAgentToBallDistanceEvery)
                {
                    UpdateAgentProxityToBalls();
                    timeSinceLastAgentToBallDistanceCalc = 0f;
                }
            }
        }

        private void UpdateAgentsSurvivalTime()
        {
            foreach (DodgeBallAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    agent.CurrentSurvivalTime++;
                }
            }
        }

        private void UpdateAgentProxityToBalls()
        {
            foreach (var ball in Balls)
            {
                maxAgentToBallDistance += playgroundDiagonal;
                foreach (DodgeBallAgentComponent agent in Agents)
                {
                    if (agent.gameObject.activeSelf)
                    {
                        agent.AgentToBallDistance += Vector3.Distance(agent.transform.position, ball.transform.position);
                    }
                }
            }
        }

        public void BallHitAgent(DodgeBallAgentComponent hitAgent, DodgeBallBallComponent ball)
        {
            if (hitAgent && ball)
            {
                hitAgent.HitByBall(ball);

                NeedsRespawn = true;
                this.hitAgent = hitAgent;
                this.ball = ball;
            }
        }

        public DodgeBallBallComponent BallInRange(DodgeBallAgentComponent agent)
        {
            // Check if any ball is within the pickup radius of the agent
            closestBallInRange = null;
            distanceToClosestBall = float.MaxValue;
            foreach (DodgeBallBallComponent ball in Balls)
            {
                if (ball && ball.Parent != agent && ball.SphereCollider.enabled)
                {
                    distance = Vector3.Distance(agent.transform.position, ball.transform.position);
                    if (distance <= AgentBallPickupRadius && distance < distanceToClosestBall)
                    {
                        if (ball.Parent)
                        {
                            // If other agent has thrown the ball, we can intercept it with a probability
                            if (Util.Rnd.NextDouble() < AgentBallInterceptProbability)
                            {
                                closestBallInRange = ball;
                                distanceToClosestBall = distance;
                            }
                        }
                        else
                        {
                            closestBallInRange = ball;
                            distanceToClosestBall = distance;
                        }
                    }
                }
            }

            return closestBallInRange;
        }

        protected override void OnPreFinishGame()
        {
            SetAgentsFitness();
        }

        public bool OpponentInThrowDirection(DodgeBallAgentComponent agent, Vector3 throwDirection)
        {
            // Check if there are any opponents in the direction of the throw
            foreach (DodgeBallAgentComponent opponent in Agents)
            {
                if (opponent.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID)
                {
                    directionToOpponent = opponent.transform.position - agent.transform.position;
                    angle = Vector3.Angle(throwDirection, directionToOpponent);
                    if (angle < MaxBallToOpponentAngle) 
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void SetAgentsFitness()
        {
            float secondsPased = CurrentSimulationSteps * Time.fixedDeltaTime;

            foreach (DodgeBallAgentComponent agent in Agents)
            {
                // Sector exploration
                sectorExplorationFitness = agent.SectorsExplored / (float)(Sectors.Length / 1.6f); // 1.6 because only 60% of sectors can be explored
                sectorExplorationFitness = (float)Math.Round(DodgeBallFitness.FitnessValues[DodgeBallFitness.FitnessKeys.SectorExploration.ToString()] * sectorExplorationFitness, 4);
                agent.AgentFitness.UpdateFitness(sectorExplorationFitness, DodgeBallFitness.FitnessKeys.SectorExploration.ToString());

                // Balls picked up
                ballsPickedUpFitness = agent.BallsPickedUp / secondsPased; // We suggest that the agent can pick up at least 1 ball per second(Upper limit)
                ballsPickedUpFitness = ballsPickedUpFitness > 1? 1 : ballsPickedUpFitness;
                ballsPickedUpFitness = (float)Math.Round(DodgeBallFitness.FitnessValues[DodgeBallFitness.FitnessKeys.BallsPickedUp.ToString()] * ballsPickedUpFitness, 4);
                agent.AgentFitness.UpdateFitness(ballsPickedUpFitness, DodgeBallFitness.FitnessKeys.BallsPickedUp.ToString());

                // Balls thrown
                ballsThrownFitness = agent.BallsThrown / secondsPased; // We suggest that the agent can throw at least 1 ball per second (Upper limit)
                ballsThrownFitness = ballsThrownFitness > 1? 1 : ballsThrownFitness;
                ballsThrownFitness = (float)Math.Round(DodgeBallFitness.FitnessValues[DodgeBallFitness.FitnessKeys.BallsThrown.ToString()] * ballsThrownFitness, 4);
                agent.AgentFitness.UpdateFitness(ballsThrownFitness, DodgeBallFitness.FitnessKeys.BallsThrown.ToString());

                // Balls thrown at opponent
                if (agent.BallsThrown > 0)
                {
                    ballsThrownAtOpponent = agent.BallsThrownAtOpponent / (float)agent.BallsThrown;
                    ballsThrownAtOpponent = (float)Math.Round(DodgeBallFitness.FitnessValues[DodgeBallFitness.FitnessKeys.BallsThrownAtOpponent.ToString()] * ballsThrownAtOpponent, 4);
                    agent.AgentFitness.UpdateFitness(ballsThrownAtOpponent, DodgeBallFitness.FitnessKeys.BallsThrownAtOpponent.ToString());
                }

                // Balls intercepted
                ballsInterceptedFitness = agent.BallsIntercepted / secondsPased; // We suggest that the agent can intercept at least 1 ball per second (Upper limit)
                ballsInterceptedFitness = ballsInterceptedFitness > 1? 1 : ballsInterceptedFitness;
                ballsInterceptedFitness = (float)Math.Round(DodgeBallFitness.FitnessValues[DodgeBallFitness.FitnessKeys.BallsIntercepted.ToString()] * ballsInterceptedFitness, 4);
                agent.AgentFitness.UpdateFitness(ballsInterceptedFitness, DodgeBallFitness.FitnessKeys.BallsIntercepted.ToString());

                // Balls hit
                numOfOpponents = Agents.Where(a => a.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID).Select(a => (a as DodgeBallAgentComponent).NumOfSpawns).Sum();
                if (numOfOpponents > 0)
                {
                    opponentsHitFitness = agent.OpponentsHit / (float)numOfOpponents;
                    opponentsHitFitness = (float)Math.Round(DodgeBallFitness.FitnessValues[DodgeBallFitness.FitnessKeys.OpponentsHit.ToString()] * opponentsHitFitness, 4);
                    agent.AgentFitness.UpdateFitness(opponentsHitFitness, DodgeBallFitness.FitnessKeys.OpponentsHit.ToString());

                }

                // Survival bonus
                agent.ResetSurvivalTime();

                survivalBonusFitness = agent.MaxSurvivalTime / (float)CurrentSimulationSteps;
                survivalBonusFitness = (float)Math.Round(DodgeBallFitness.FitnessValues[DodgeBallFitness.FitnessKeys.SurvivalBonus.ToString()] * survivalBonusFitness, 4);
                agent.AgentFitness.UpdateFitness(survivalBonusFitness, DodgeBallFitness.FitnessKeys.SurvivalBonus.ToString());

                // Agent to ball distance
                agentToBallDistanceFitness = (maxAgentToBallDistance - agent.AgentToBallDistance) / maxAgentToBallDistance;
                agentToBallDistanceFitness = (float)Math.Round(DodgeBallFitness.FitnessValues[DodgeBallFitness.FitnessKeys.AgentToBallDistance.ToString()] * agentToBallDistanceFitness, 4);
                agent.AgentFitness.UpdateFitness(agentToBallDistanceFitness, DodgeBallFitness.FitnessKeys.AgentToBallDistance.ToString());

                agentFitnessLog = "========================================\n" +
                                  $"[Agent]: Team ID + {agent.TeamIdentifier.TeamID} , ID: " + agent.IndividualID +
                                  "\n" +
                                  $"[Sectors explored]: {agent.SectorsExplored} / {(Sectors.Length / 1.6f)} = {sectorExplorationFitness}\n" +
                                  $"[Balls picked up]: {agent.BallsPickedUp} / {secondsPased} = {ballsPickedUpFitness}\n" +
                                  $"[Balls thrown]: {agent.BallsThrown} / {secondsPased} = {ballsThrownFitness}\n" +
                                  $"[Balls intercepted]: {agent.BallsIntercepted} / {secondsPased} = {ballsInterceptedFitness} \n" +
                                  $"[Opponents hit]: {agent.OpponentsHit} / {numOfOpponents} = {opponentsHitFitness} \n" +
                                  $"[Survival bonus]: {agent.MaxSurvivalTime} / {CurrentSimulationSteps} = {survivalBonusFitness}\n" +
                                  $"[Agent to ball distance]: {(maxAgentToBallDistance - agent.AgentToBallDistance)} / {maxAgentToBallDistance} = {agentToBallDistanceFitness}\n";
                DebugSystem.LogVerbose(agentFitnessLog);
            }
        }
        void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;

                DodgeBallFitness.FitnessValues = conf.FitnessValues;

                if (conf.ProblemConfiguration.ContainsKey("AgentRunSpeed"))
                {
                    AgentRunSpeed = float.Parse(conf.ProblemConfiguration["AgentRunSpeed"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentRotationSpeed"))
                {
                    AgentRotationSpeed = float.Parse(conf.ProblemConfiguration["AgentRotationSpeed"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentThrowPower"))
                {
                    AgentThrowPower = float.Parse(conf.ProblemConfiguration["AgentThrowPower"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentBallPickupRadius"))
                {
                    AgentBallPickupRadius = float.Parse(conf.ProblemConfiguration["AgentBallPickupRadius"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentBallInterceptProbability"))
                {
                    AgentBallInterceptProbability = float.Parse(conf.ProblemConfiguration["AgentBallInterceptProbability"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentBallThrowCooldown"))
                {
                    AgentBallThrowCooldown = float.Parse(conf.ProblemConfiguration["AgentBallThrowCooldown"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("MaxBallToOpponentAngle"))
                {
                    MaxBallToOpponentAngle = float.Parse(conf.ProblemConfiguration["MaxBallToOpponentAngle"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("CalculateAgentToBallDistanceEvery"))
                {
                    CalculateAgentToBallDistanceEvery = float.Parse(conf.ProblemConfiguration["CalculateAgentToBallDistanceEvery"]);
                }
            }
        }

    }

    public enum BallPickUpMode
    {
        Automatic,
        Manual
    }
}
