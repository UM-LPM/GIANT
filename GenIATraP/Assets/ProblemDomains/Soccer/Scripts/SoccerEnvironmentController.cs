using Base;
using static Problems.Soccer.SoccerUtils;
using UnityEngine;
using System;
using Configuration;
using System.Linq;

namespace Problems.Soccer
{
    public class SoccerEnvironmentController : EnvironmentControllerBase
    {
        [Header("Soccer General Configuration")]
        [SerializeField] SoccerGameScenarioType GameScenarioType = SoccerGameScenarioType.GoldenGoal;
        [SerializeField] public GameObject SoccerBallPrefab;

        [Header("Soccer Agent Configuration")]
        [SerializeField] public float ForwardSpeed = 1f;
        [SerializeField] public float LateralSpeed = 1f;
        [SerializeField] public float AgentRunSpeed = 2f;
        [SerializeField] public float AgentRotationSpeed = 100f;
        [SerializeField] public float KickPower = 2000f;
        [SerializeField] public static float VelocityPassTreshold = 0.2f;
        [SerializeField] public float PassTolerance = 0.1f; // Tolerance in degrees.


        private SoccerBallSpawner SoccerBallSpawner;
        private SoccerBallComponent SoccerBall;

        GoalComponent GoalPurple;
        GoalComponent GoalBlue;

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
                // TODO Add error reporting here
            }
        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            // Spawn Soccer Ball
            SoccerBall = SoccerBallSpawner.Spawn<SoccerBallComponent>(this)[0];
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

        public void GoalScored(SoccerAgentComponent striker, SoccerTeam goalReceivingTeam)
        {
            if (striker.Team == goalReceivingTeam)
            {
                // Autogol was scored
                striker.AutoGoalsScored++;
            }
            else
            {
                // Legit goal was scored
                striker.GoalsScored++;
            }

            // Check engind state
            CheckEndingState();
        }

        public override void CheckEndingState()
        {
            if (GameScenarioType == SoccerGameScenarioType.GoldenGoal)
            {
                FinishGame();
            }
            else if (GameScenarioType == SoccerGameScenarioType.Match)
            {
                MatchSpawner.Respawn<SoccerAgentComponent>(this, Agents as SoccerAgentComponent[]);
                SoccerBallSpawner.Respawn<SoccerBallComponent>(this, SoccerBall);
            }
        }

        protected override void OnPreFinishGame()
        {
            SetAgentsFitness();
        }

        private void SetAgentsFitness()
        {
            foreach (SoccerAgentComponent agent in Agents)
            {
                // TODO Implement this
                Debug.Log("========================================");
                Debug.Log("Agent: Team ID" + agent.TeamID + ", ID: " + agent.IndividualID);
                Debug.Log("Sectors explored: " + agent.SectorsExplored);
                Debug.Log("Goals scored: " + agent.GoalsScored);
                Debug.Log("Auto goals scored: " + agent.AutoGoalsScored);
                Debug.Log("Passes: " + agent.Passes);
                Debug.Log("Passes to oponent goal: " + agent.PassesToOponentGoal);
                Debug.Log("Passes to own goal: " + agent.PassesToOwnGoal);
            }
        }
    }
}