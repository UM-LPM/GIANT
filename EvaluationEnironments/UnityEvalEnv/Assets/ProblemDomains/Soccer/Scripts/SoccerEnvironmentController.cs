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

        // Soccer Ball
        SoccerBallSpawner SoccerBallSpawner;
        SoccerBallComponent SoccerBall;

        // Goals
        GoalComponent GoalPurple;
        GoalComponent GoalBlue;

        // Sectors
        private SectorComponent[] Sectors;

        // Fitness calculation
        private float sectorExplorationFitness;
        private float goalsScoredFitness;
        private float autoGoalsScoredFitness;
        private float goalsReceivedFitness;
        private float passesToOponentGoalFitness;
        private float passesToOwnGoalFitness;
        private float passesFitness;

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

            if (SimulationSteps > 0)
            {
                MaxPasses = (int)Math.Floor((SimulationSteps * Time.fixedDeltaTime)) / 2;
            }
            else if(SimulationTime > 0)
            {
                MaxPasses = (int)Math.Floor(SimulationTime) / 2;
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

                if (conf.ProblemConfiguration.ContainsKey("MaxGoals"))
                {
                    MaxGoals = int.Parse(conf.ProblemConfiguration["MaxGoals"]);
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
                goalsScoredFitness = agent.GoalsScored / (float)MaxGoals;
                goalsScoredFitness = (float)Math.Round(SoccerFitness.FitnessValues[SoccerFitness.FitnessKeys.GoalsScored.ToString()] * goalsScoredFitness, 4);
                agent.AgentFitness.UpdateFitness(goalsScoredFitness, SoccerFitness.FitnessKeys.GoalsScored.ToString());

                // Auto goals scored
                autoGoalsScoredFitness = agent.AutoGoalsScored / (float)MaxGoals;
                autoGoalsScoredFitness = (float)Math.Round(SoccerFitness.FitnessValues[SoccerFitness.FitnessKeys.AutoGoals.ToString()] * autoGoalsScoredFitness, 4);
                agent.AgentFitness.UpdateFitness(autoGoalsScoredFitness, SoccerFitness.FitnessKeys.AutoGoals.ToString());

                // Goals received (Every agents gets portion of the team goals received fitness)
                goalsReceivedFitness = (agent.Team == SoccerTeam.Blue ? GoalBlue.GoalsReceived : GoalPurple.GoalsReceived) / (float)MaxGoals;
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

                /*
                Debug.Log("========================================");
                Debug.Log("Agent: Team ID" + agent.TeamID + ", ID: " + agent.IndividualID);
                Debug.Log("Sectors explored: " + agent.SectorsExplored + " / " + Sectors.Length + "= " + sectorExplorationFitness);
                Debug.Log("Goals scored: " + agent.GoalsScored + " / " + MaxGoals + "= " + goalsScoredFitness);
                Debug.Log("Auto goals scored: " + agent.AutoGoalsScored + " / " + MaxGoals + "= " + autoGoalsScoredFitness);
                Debug.Log("Goals received: " + (agent.Team == SoccerTeam.Blue ? GoalBlue.GoalsReceived : GoalPurple.GoalsReceived) + " / " + MaxGoals + "= " + goalsReceivedFitness);
                Debug.Log("Passes to oponent goal: " + agent.PassesToOponentGoal + " / " + MaxPasses + "= " + passesToOponentGoalFitness);
                Debug.Log("Passes to own goal: " + agent.PassesToOwnGoal + " / " + MaxPasses + "= " + passesToOwnGoalFitness);
                Debug.Log("Passes: " + agent.Passes + " / " + MaxPasses + "= " + passesFitness);
                */
            }
        }
    }
}