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

        [Header("Ball configuration")]
        [SerializeField] public GameObject BallPrefab;


        private DodgeBallBallSpawner BallSpawner;
        private DodgeBallBallComponent[] Balls;

        // Sectors
        private SectorComponent[] Sectors;

        // Temp variables
        private DodgeBallBallComponent closestBallInRange;
        private float distanceToClosestBall = float.MaxValue;
        private float distance;
        private Vector3 nextPosition;

        // Fitness calculation
        private float sectorExplorationFitness;
        private float ballsPickedUpFitness;
        private float ballsThrownFitness;
        private float ballsInterceptedFitness;
        private float opponentsHitFitness;
        private float ballsHitByFitness;

        private int numOfOpponents;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();

            BallSpawner = GetComponent<DodgeBallBallSpawner>();
            if (BallSpawner == null)
            {
                throw new Exception("BallSpawner is not defined");
                // TODO Add error reporting here
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
        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            Balls = BallSpawner.Spawn<DodgeBallBallComponent>(this);
        }

        public void BallHitAgent(DodgeBallAgentComponent hitAgent, DodgeBallBallComponent ball)
        {
            if (hitAgent && ball)
            {
                hitAgent.HitByBall(ball);

                MatchSpawner.Respawn<DodgeBallAgentComponent>(this, hitAgent);

                ball.ResetBall();
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
                    opponentsHitFitness = (float)Math.Round(DodgeBallFitness.FitnessValues[DodgeBallFitness.FitnessKeys.BallsHit.ToString()] * opponentsHitFitness, 4);
                    agent.AgentFitness.UpdateFitness(opponentsHitFitness, DodgeBallFitness.FitnessKeys.BallsHit.ToString());

                }

                // Balls hit by
                ballsHitByFitness = agent.BallsHitBy / secondsPased;
                ballsHitByFitness = ballsHitByFitness > 1? 1 : ballsHitByFitness;
                ballsHitByFitness = (float)Math.Round(DodgeBallFitness.FitnessValues[DodgeBallFitness.FitnessKeys.BallsHitBy.ToString()] * ballsHitByFitness, 4);
                agent.AgentFitness.UpdateFitness(ballsHitByFitness, DodgeBallFitness.FitnessKeys.BallsHitBy.ToString());

                Debug.Log("========================================");
                Debug.Log("Agent: Team ID" + agent.TeamIdentifier.TeamID + ", ID: " + agent.IndividualID);
                Debug.Log("Sectors explored: " + agent.SectorsExplored + " / " + (Sectors.Length / 1.6f) + " =" + sectorExplorationFitness);
                Debug.Log("Balls picked up: " + agent.BallsPickedUp + " / " + secondsPased + " = " + ballsPickedUpFitness);
                Debug.Log("Balls thrown: " + agent.BallsThrown + " / " + secondsPased + " = " + ballsThrownFitness);
                Debug.Log("Balls intercepted: " + agent.BallsIntercepted + " / " + secondsPased + " = " + ballsInterceptedFitness);
                Debug.Log("Opponents hit: " + agent.OpponentsHit + " / " + numOfOpponents + " = " + opponentsHitFitness);
                Debug.Log("Balls hit by: " + agent.BallsHitBy + " / " + secondsPased + " = " + ballsHitByFitness);
                Debug.Log("========================================");

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
            }
        }

    }

    public enum BallPickUpMode
    {
        Automatic,
        Manual
    }
}
