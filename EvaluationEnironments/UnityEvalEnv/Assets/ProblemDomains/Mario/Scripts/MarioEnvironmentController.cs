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
        public float SmallJumpMultiplier = 2.3f;
        public float BigJumpMultiplier = 1.5f;
        public float SpeedJumpBonus = 0.5f;

        public float CoyoteTime = 0.1f;

        [Header("Enemy Movement Configuration")]
        public float EnemyPatrolSpeed = 2f;

        private bool finishReached = false;
        private bool isDead = false;

        private MarioMatchSpawner MarioMatchSpawner;
        private FinishComponent FinishComponent;

        private EnemyComponent[] Enemies;
        private CoinComponent[] Coins;
        private MysteryBlockComponent[] MysteryBlocks;
        private int TotalCoins; // Coins spawned in the level + coins obtained through mystery boxes

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

            Enemies = GetComponentsInChildren<EnemyComponent>();
            Coins = GetComponentsInChildren<CoinComponent>();
            TotalCoins = Coins.Length;
            MysteryBlocks = GetComponentsInChildren<MysteryBlockComponent>();
        }

        protected override void OnPostFixedUpdate()
        {
            UpdateEnemies();
            AgentFellFromPlatform();
            AgentCollectedCoin();
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

        public void AgentCollectedCoin()
        {
            foreach(MarioAgentComponent agent in Agents)
            {
                foreach(CoinComponent coin in Coins)
                {
                    if(coin.isActiveAndEnabled && agent.BoxCollider2D.bounds.Intersects(coin.BoxCollider2D.bounds))
                    {
                        coin.enabled = false;
                        coin.gameObject.SetActive(false);
                        agent.CoinsCollected++;
                    }
                }
            }
        }

        public void UpdateEnemies()
        {
            foreach (EnemyComponent enemy in Enemies)
            {
                if(enemy.isActiveAndEnabled)
                    enemy.UpdateEnemy();
            }
        }

        public void OnAgentReachedFinish(MarioAgentComponent agent, FinishComponent finish)
        {
            finishReached = true;
        }

        public void OnAgentFellFromPlatform(MarioAgentComponent agent)
        {
            isDead = true;
        }

        public void OnAgentKilledEnemy(MarioAgentComponent agent, EnemyComponent enemy)
        {
            agent.EnemiesKilled++;
            enemy.enabled = false;
            enemy.gameObject.SetActive(false);
        }

        public void OnEnemyKilledAgent(MarioAgentComponent agent, EnemyComponent enemy)
        {
            isDead = true;
        }

        public void OnMysteryBlockWithCoinHit(MarioAgentComponent agent)
        {
            TotalCoins++;
            agent.CoinsCollected++;
            agent.MysteryBlocksDestroyed++;
        }

        public void SetAgentsFitness()
        {
            foreach (MarioAgentComponent agent in Agents)
            {
                // Time penalty
                timePenalty = isDead? 1f : (CurrentSimulationSteps / (float)SimulationSteps);
                timePenalty = (float)Math.Round(MarioFitness.FitnessValues[MarioFitness.FitnessKeys.TimePenalty.ToString()] * timePenalty, 4);
                agent.AgentFitness.UpdateFitness(timePenalty, MarioFitness.FitnessKeys.TimePenalty.ToString());

                // Death penalty
                if(isDead)
                {
                    float deathPenalty = MarioFitness.FitnessValues[MarioFitness.FitnessKeys.DeathPenalty.ToString()];
                    deathPenalty = (float)Math.Round(deathPenalty, 4);
                    agent.AgentFitness.UpdateFitness(deathPenalty, MarioFitness.FitnessKeys.DeathPenalty.ToString());
                }

                // Enemies killed fitness
                float enemiesKilled = agent.EnemiesKilled / Enemies.Length;
                float enemiesKilledFitness = enemiesKilled * MarioFitness.FitnessValues[MarioFitness.FitnessKeys.EnemiesKilled.ToString()];
                enemiesKilledFitness = (float)Math.Round(enemiesKilledFitness, 4);
                agent.AgentFitness.UpdateFitness(enemiesKilledFitness, MarioFitness.FitnessKeys.EnemiesKilled.ToString());

                // Coins collected fitness
                float coinsCollected = agent.CoinsCollected / TotalCoins;
                float coinsCollectedFitness = coinsCollected * MarioFitness.FitnessValues[MarioFitness.FitnessKeys.CoinsCollected.ToString()];
                coinsCollectedFitness = (float)Math.Round(coinsCollectedFitness, 4);
                agent.AgentFitness.UpdateFitness(coinsCollectedFitness, MarioFitness.FitnessKeys.CoinsCollected.ToString());

                // Mystery blocks destroyed fitness
                float mysteryBlocksDestroyed =  agent.MysteryBlocksDestroyed / MysteryBlocks.Length;
                float mysteryBlocksDestroyedFitness = mysteryBlocksDestroyed * MarioFitness.FitnessValues[MarioFitness.FitnessKeys.MysteryBlocksDestroyed.ToString()];
                mysteryBlocksDestroyedFitness = (float)Math.Round(mysteryBlocksDestroyedFitness, 4);
                agent.AgentFitness.UpdateFitness(mysteryBlocksDestroyedFitness, MarioFitness.FitnessKeys.MysteryBlocksDestroyed.ToString());

                // Distance fitness
                float maxDistance = Math.Abs(MarioMatchSpawner.AgentSpawnPoint.transform.position.x - FinishComponent.transform.position.x);
                float agentDistance = Math.Abs(agent.transform.position.x - FinishComponent.transform.position.x);
                if(agentDistance > maxDistance)
                    agentDistance = maxDistance;
                if(finishReached)
                    agentDistance = 0f;

                float distanceFitness = (1 - (agentDistance / maxDistance)) * MarioFitness.FitnessValues[MarioFitness.FitnessKeys.Distance.ToString()];
                distanceFitness = (float)Math.Round(distanceFitness, 4);
                agent.AgentFitness.UpdateFitness(distanceFitness, MarioFitness.FitnessKeys.Distance.ToString());

                agentFitnessLog = "========================================\n" +
                                  $"[Agent]: TeamID {agent.TeamIdentifier.TeamID}, ID: {agent.IndividualID} \n" +
                                  $"[Time penalty]: {CurrentSimulationSteps} / {SimulationSteps} = {timePenalty}\n" +
                                  $"[Death penalty]: {(isDead? MarioFitness.FitnessValues[MarioFitness.FitnessKeys.DeathPenalty.ToString()] : 0f)}\n" +
                                  $"[Enemies killed]: {agent.EnemiesKilled} / {Enemies.Length} = {enemiesKilledFitness}\n" +
                                  $"[Coins collected]: {agent.CoinsCollected} / {TotalCoins} = {coinsCollectedFitness}\n" +
                                  $"[Mystery blocks destroyed]: {agent.MysteryBlocksDestroyed} / {MysteryBlocks.Length} = {mysteryBlocksDestroyedFitness}\n" +
                                  $"[Distance]: {agentDistance} / {maxDistance} = {distanceFitness}\n";

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
                if (conf.ProblemConfiguration.ContainsKey("SmallJumpMultiplier"))
                {
                    SmallJumpMultiplier = float.Parse(conf.ProblemConfiguration["SmallJumpMultiplier"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("BigJumpMultiplier"))
                {
                    BigJumpMultiplier = float.Parse(conf.ProblemConfiguration["BigJumpMultiplier"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("SpeedJumpBonus"))
                {
                    SpeedJumpBonus = float.Parse(conf.ProblemConfiguration["SpeedJumpBonus"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("CoyoteTime"))
                {
                    CoyoteTime = float.Parse(conf.ProblemConfiguration["CoyoteTime"]);
                }
                if (conf.ProblemConfiguration.ContainsKey("EnemyPatrolSpeed"))
                {
                    EnemyPatrolSpeed = float.Parse(conf.ProblemConfiguration["EnemyPatrolSpeed"]);
                }
            }
        }
    }
}