using Base;
using System;
using UnityEngine;
using Configuration;
using Utils;
using static Problems.Soccer2D.Soccer2DUtils;
using Unity.VisualScripting;

namespace Problems.Pong
{
    public class PongEnvironmentController : EnvironmentControllerBase
    {
        [Header("Pong Configuration")]
        [SerializeField] public int MaxPoints;
        [SerializeField] Vector2[] initBallDirections = new Vector2[]
        {
            new Vector2(0.5f, 0.7f),
            new Vector2(-0.7f, 0.5f),
            new Vector2(0.5f, -0.7f),
            new Vector2(-0.7f, -0.5f)
        };

        [Header("Pong Agent Configuration")]
        [SerializeField] public float AgentSpeed = 5f;

        [Header("Pong Ball Configuration")]
        [SerializeField] public GameObject PongBallPrefab;
        [SerializeField] public float BallCollisionCheckRadius = 0.05f;
        [SerializeField] public float BallSpeed = 10f;


        PongBallSpawner PongBallSpawner;
        PongBallComponent PongBall;

        bool NeedsRespawn = false;

        PongSideComponent lastReceivedPointPongSideComponent;

        int lastPushedPongSideComponentCounter = 0;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();

            PongBallSpawner = GetComponent<PongBallSpawner>();
            if (PongBallSpawner == null)
            {
                throw new Exception("PongBallSpawner is not defined");
            }
        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            // Spawn Ball
            PongBall = PongBallSpawner.Spawn<PongBallComponent>(this)[0];
            PongBall.SetVelocity(initBallDirections[lastPushedPongSideComponentCounter++ % initBallDirections.Length] * BallSpeed);
        }

        protected override void OnPreFixedUpdate()
        {
            CheckIfPointScored();

            if (NeedsRespawn)
            {
                MatchSpawner.Respawn<PongAgentComponent>(this, Agents as PongAgentComponent[]);
                PongBallSpawner.Respawn<PongBallComponent>(this, PongBall);
                ForceNewDecisions = true;
                NeedsRespawn = false;

                if (GameState == GameState.RUNNING)
                {
                    PongBall.SetVelocity(initBallDirections[lastPushedPongSideComponentCounter++ % initBallDirections.Length] * BallSpeed);
                }
            }
        }

        private bool CheckIfPointScored()
        {
            lastReceivedPointPongSideComponent = PhysicsUtil.PhysicsOverlapSphereTargetObject<PongSideComponent>(
                    PhysicsScene,
                    PhysicsScene2D,
                    GameType,
                    PongBall.gameObject,
                    PongBall.transform.position,
                    PongBall.Radius,
                    false,
                    PongBall.gameObject.layer);

            if (lastReceivedPointPongSideComponent != null)
            {
                PointScored(lastReceivedPointPongSideComponent);
                return true;
            }

            return false;
        }

        public void PointScored(PongSideComponent sideComponent)
        {
            // Add point to all agents who didn't let the ball through
            foreach(PongAgentComponent agent in Agents)
            {
                if(agent.Side != sideComponent.Side)
                {
                    agent.PointsScored++;
                }
            }

            NeedsRespawn = true;
        }

        protected override void OnPostFixedUpdate()
        {
            PongBall.OnStep();
        }

        protected override void OnPreFinishGame()
        {
            SetAgentsFitness();
        }

        public void SetAgentsFitness()
        {
            float pointsScoredFitness;
            float ballBouncesFitness;
            string agentFitnessLog;

            // 1 step = 20ms
            // Max ball bounces per episode = bounce per 5 seconds
            float maxBallBounces = SimulationSteps / ((1000f / 20f) * 2f);

            foreach (PongAgentComponent agent in Agents)
            {
                // Ball bounces
                ballBouncesFitness = agent.BallBounces / maxBallBounces;
                if(ballBouncesFitness > 1f)
                {
                    ballBouncesFitness = 1f;
                }
                ballBouncesFitness = (float)Math.Round(PongFitness.FitnessValues[PongFitness.FitnessKeys.BallBounces.ToString()] * ballBouncesFitness, 4);
                agent.AgentFitness.UpdateFitness(ballBouncesFitness, PongFitness.FitnessKeys.BallBounces.ToString());

                // Points scored
                pointsScoredFitness = agent.PointsScored / (float)PongBall.NumOfSpawns;
                pointsScoredFitness = (float)Math.Round(PongFitness.FitnessValues[PongFitness.FitnessKeys.PointsScored.ToString()] * pointsScoredFitness, 4);
                agent.AgentFitness.UpdateFitness(pointsScoredFitness, PongFitness.FitnessKeys.PointsScored.ToString());

                agentFitnessLog = "========================================\n" +
                                  $"[Agent]: Team ID {agent.TeamIdentifier.TeamID}, ID: {agent.IndividualID}\n" +
                                  $"[Ball bounces]: {agent.BallBounces} / {maxBallBounces} = {ballBouncesFitness}\n" +
                                  $"[Points scored]: {agent.PointsScored} / {PongBall.NumOfSpawns} = {pointsScoredFitness}";

                DebugSystem.LogVerbose(agentFitnessLog);


            }
        }


        public void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;

                PongFitness.FitnessValues = conf.FitnessValues;

                if (conf.ProblemConfiguration.ContainsKey("MaxPoints"))
                {
                    MaxPoints = int.Parse(conf.ProblemConfiguration["MaxPoints"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentSpeed"))
                {
                    AgentSpeed = float.Parse(conf.ProblemConfiguration["AgentSpeed"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("BallSpeed"))
                {
                    BallSpeed = float.Parse(conf.ProblemConfiguration["BallSpeed"]);
                }
            }
        }
    }
}