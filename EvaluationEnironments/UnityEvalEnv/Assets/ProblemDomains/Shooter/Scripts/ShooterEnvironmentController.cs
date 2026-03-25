using Base;
using Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utils;

namespace Problems.Shooter
{
    public class ShooterEnvironmentController : EnvironmentControllerBase
    {
        [Header("Shooter Agent Movement Configuration")]
        [SerializeField] public float AgentMoveSpeed = 5f;
        [SerializeField] public float AgentRotationSpeed = 180f;
        [HideInInspector] public float ForwardSpeed = 1f;

        [Header("Shooter General Configuration")]
        [SerializeField] int AgentStartHealth = 5;
        [SerializeField] int AgentStartShield = 0;
        [SerializeField] int AgentMaxShield = 5;

        [Header("Shooter Weapon Item Configuration")]
        [SerializeField] public GameObject WeaponItemPrefab;
        [SerializeField] public GameObject WeaponPrefab;
        [SerializeField] public GameObject WeaponBulletPrefab;
        [SerializeField] public int BulletShootCooldown = 50;
        [SerializeField] public float BulletSpeed = 20f;
        [SerializeField] public int BulletDamage = 1;

        [Header("Shooter Health Item Configuration")]
        [SerializeField] public GameObject HealthItemPrefab;

        [Header("Shooter Shield Item Configuration")]
        [SerializeField] public GameObject ShieldItemPrefab;

        private ShooterItemSpawner ItemSpawner;
        private List<Item> Items;

        // Bullet Controller
        [HideInInspector] public WeaponBulletController BulletController;

        // Sectors
        private SectorComponent[] Sectors;

        // Private variables
        private ShooterAgentComponent agent;
        private Vector3 sectorPosition;


        // Temp variables 
        private List<Item> items;

        // Private variables for fitness calculation
        float sectorExplorationFitness;
        float bulletsFired;
        float allPossibleBulletsFired;
        float bulletsFiredAccuracy;
        float opponentsDefeatedBonus;
        float damageTakenPenalty;
        float survivalBonus;
        float friendlyFirePenalty;

        int numOfOpponents;
        int numOfFiredOpponentBullets;
        int opponentsHealthSum;

        protected override void DefineAdditionalDataOnPostAwake()
        {
            ReadParamsFromMainConfiguration();
            SetBestAndWorstFitnesses(ShooterFitness.FitnessValues);

            ItemSpawner = GetComponent<ShooterItemSpawner>();
            if (ItemSpawner == null)
            {
                throw new Exception("ShooterItemSpawner is not defined");
            }

            Sectors = GetComponentsInChildren<SectorComponent>();
            Items = new List<Item>();

            BulletController = GetComponent<WeaponBulletController>();
            if (BulletController == null)
            {
                throw new Exception("BulletController is not defined");
            }
        }

        protected override void DefineAdditionalDataOnPostStart()
        {
            foreach (ShooterAgentComponent agent in Agents)
            {
                agent.HealthComponent.Health = AgentStartHealth;
            }

            // Spawn target
            Items.AddRange(ItemSpawner.Spawn<Item>(this).ToList());
        }

        protected override void OnPostFixedUpdate()
        {
            if (GameState == GameState.RUNNING)
            {
                CheckAgentsPickedItem();
                BulletController.UpdateBulletPosAndCheckForColls();
                CheckAgentsExploration();
            }
        }

        private void CheckAgentsPickedItem()
        {
            foreach (ShooterAgentComponent agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                {
                    foreach (Item item in Items)
                    {
                        float distanceToItem = Vector3.Distance(agent.transform.position, item.transform.position);
                        if (distanceToItem <= (AgentColliderExtendsMultiplier.x + (item.transform.localScale.x / 2f)))
                        {
                            if (item is WeaponItem weaponItem)
                            {
                                if (AgentPickedUpWeaponItem(agent, weaponItem))
                                {
                                    Destroy(item.gameObject);
                                    item.gameObject.SetActive(false);
                                }
                            }
                            else if (item is HealthItem healthItem)
                            {
                                if (agent.HealthComponent.Health < AgentStartHealth)
                                {
                                    agent.HealthItemCollected = true;
                                    agent.HealthComponent.Health = AgentStartHealth;
                                    Destroy(item.gameObject);
                                    item.gameObject.SetActive(false);
                                }
                            }
                            else if (item is ShieldItem shieldItem)
                            {
                                if (agent.ShieldComponent.Shield < AgentMaxShield)
                                {
                                    agent.ShieldItemCollected = true;
                                    agent.ShieldComponent.Shield = AgentMaxShield;
                                    Destroy(item.gameObject);
                                    item.gameObject.SetActive(false);
                                }
                            }
                        }
                    }

                    // Remove inactive items
                    Items.RemoveAll(i => !i.gameObject.activeSelf);
                }
            }
        }

        private bool AgentPickedUpWeaponItem(ShooterAgentComponent agent, WeaponItem weaponItem)
        {
            if (agent.HasWeapon())
            {
                return false;
            }

            // Spawn weapon and attach to agent
            GameObject weaponObj = Instantiate(WeaponPrefab, agent.WeaponPlaceholder.position, Quaternion.identity, agent.WeaponPlaceholder);

            WeaponComponent weapon = weaponObj.GetComponent<WeaponComponent>();
            agent.EquipWeapon(weapon);

            return true;
        }

        private void CheckAgentsExploration()
        {
            // Exploration bonus
            for (int i = 0; i < Agents.Length; i++)
            {
                agent = Agents[i] as ShooterAgentComponent;
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

        private bool IsAgentInSector(Vector3 agentPosition, Collider2D colliderComponent)
        {
            if (colliderComponent.bounds.Contains(agentPosition))
            {
                return true;
            }

            return false;
        }

        public void AgentHit(WeaponBulletComponent bullet, AgentComponent hitAgent)
        {
            if(bullet.Parent.TeamIdentifier.TeamID == hitAgent.TeamIdentifier.TeamID)
            {
                // Friendly fire
                (bullet.Parent as ShooterAgentComponent).BulletHitTeamMate();
                UpdateAgentHealth(bullet, hitAgent as ShooterAgentComponent, true);
            }
            else
            {
                // Opponent hit
                (bullet.Parent as ShooterAgentComponent).BulletHitOpponent();
                (hitAgent as ShooterAgentComponent).HitByOpponentBullet();
                UpdateAgentHealth(bullet, hitAgent as ShooterAgentComponent, false);
            }
        }

        void UpdateAgentHealth(WeaponBulletComponent bullet, ShooterAgentComponent hitAgent, bool friendlyFire)
        {
            hitAgent.TakeDamage(BulletDamage);

            if (hitAgent.HealthComponent.Health <= 0)
            {
                hitAgent.SurvivedSimulationSteps = CurrentSimulationSteps;
                hitAgent.gameObject.SetActive(false);
                if(!friendlyFire)
                    (bullet.Parent as ShooterAgentComponent).OpponentsDefeated++;
            }
        }

        public override int GetNumOfActiveAgents()
        {
            // Check if at least two agents from different teams are alive, otherwise finish the simulation

            Dictionary<int, int> aliveAgentsPerTeam = new Dictionary<int, int>();
            for (int i = 0; i < Match.Teams.Length; i++)
                {
                    aliveAgentsPerTeam.Add(Match.Teams[i].TeamId, 0);
            }

            foreach (var agent in Agents)
            {
                if (agent.gameObject.activeSelf)
                    aliveAgentsPerTeam[agent.TeamIdentifier.TeamID]++;
            }

            // Check if at least two teams have alive agents
            int teamsWithAliveAgents = aliveAgentsPerTeam.Values.Count(count => count > 0);
            return teamsWithAliveAgents;
        }

        public override bool IsSimulationFinished()
        {
            return base.IsSimulationFinished() || GetNumOfActiveAgents() < 2;
        }

        protected override void OnPreFinishGame()
        {
            SetAgentsFitness();
        }

        private void SetAgentsFitness()
        {
            foreach (ShooterAgentComponent agent in Agents)
            {
                // Sector exploration
                sectorExplorationFitness = agent.SectorsExplored / (float)Sectors.Length;
                sectorExplorationFitness = (float)Math.Round(ShooterFitness.FitnessValues[ShooterFitness.FitnessKeys.SectorExploration.ToString()] * sectorExplorationFitness, 4);
                agent.AgentFitness.UpdateFitness(sectorExplorationFitness, ShooterFitness.FitnessKeys.SectorExploration.ToString());

                // Weapontem pick up bonus
                if (agent.HasWeapon())
                {
                    agent.AgentFitness.UpdateFitness(ShooterFitness.FitnessValues[ShooterFitness.FitnessKeys.WeaponItemPickUp.ToString()], ShooterFitness.FitnessKeys.WeaponItemPickUp.ToString());
                }

                if (agent.HealthItemCollected)
                {
                    agent.AgentFitness.UpdateFitness(ShooterFitness.FitnessValues[ShooterFitness.FitnessKeys.HealthItemPickUp.ToString()], ShooterFitness.FitnessKeys.HealthItemPickUp.ToString());
                }

                if (agent.ShieldItemCollected)
                {
                    agent.AgentFitness.UpdateFitness(ShooterFitness.FitnessValues[ShooterFitness.FitnessKeys.ShieldItemPickUp.ToString()], ShooterFitness.FitnessKeys.ShieldItemPickUp.ToString());
                }

                // Bullets fired
                if(agent.BulletsFired > 0)
                {
                    // Bullets fired
                    agent.AgentFitness.UpdateFitness(ShooterFitness.FitnessValues[ShooterFitness.FitnessKeys.BulletsFired.ToString()], ShooterFitness.FitnessKeys.BulletsFired.ToString());
                }

                // Bullets fired accuracy
                if (agent.BulletsFired > 0)
                {
                    int opponents = Agents.Where(a => a.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID).Count();
                    int opponentsHealthSum = opponents * AgentStartHealth; // Max possible health of opponents
                    int reuiredShots = (int)Math.Ceiling(opponentsHealthSum / (float)BulletDamage); // Minimum bullets required to defeat all opponents

                    double accuracy = agent.BulletsHitOpponent / (float)agent.BulletsFired;
                    double progress = agent.BulletsHitOpponent / (float)reuiredShots;

                    bulletsFiredAccuracy = (float)(accuracy * progress);
                    bulletsFiredAccuracy = (float)Math.Round(ShooterFitness.FitnessValues[ShooterFitness.FitnessKeys.BulletsFiredAccuracy.ToString()] * bulletsFiredAccuracy, 4);
                    agent.AgentFitness.UpdateFitness(bulletsFiredAccuracy, ShooterFitness.FitnessKeys.BulletsFiredAccuracy.ToString());
                }

                // Opponents destroyed
                numOfOpponents = Agents.Where(a => a.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID).Count();
                if (numOfOpponents > 0)
                {
                    opponentsDefeatedBonus = agent.OpponentsDefeated / (float)numOfOpponents;
                    opponentsDefeatedBonus = (float)Math.Round(ShooterFitness.FitnessValues[ShooterFitness.FitnessKeys.OpponentDefeatedBonus.ToString()] * opponentsDefeatedBonus, 4);
                    agent.AgentFitness.UpdateFitness(opponentsDefeatedBonus, ShooterFitness.FitnessKeys.OpponentDefeatedBonus.ToString());
                }

                // Survival bonus
                survivalBonus = (agent.SurvivedSimulationSteps == -1? SimulationSteps : agent.SurvivedSimulationSteps) / (float)SimulationSteps;
                survivalBonus = (float)Math.Round(ShooterFitness.FitnessValues[ShooterFitness.FitnessKeys.SurvivalBonus.ToString()] * survivalBonus, 4);
                agent.AgentFitness.UpdateFitness(survivalBonus, ShooterFitness.FitnessKeys.SurvivalBonus.ToString());

                // Damage taken
                if (agent.HitByOpponentBullets > 0)
                {
                    damageTakenPenalty = agent.HitByOpponentBullets / (float)AgentStartHealth;
                    if (damageTakenPenalty > 1)
                        damageTakenPenalty = 1f;
                    damageTakenPenalty = (float)Math.Round(ShooterFitness.FitnessValues[ShooterFitness.FitnessKeys.DamageTakenPenalty.ToString()] * damageTakenPenalty, 4);
                    agent.AgentFitness.UpdateFitness(damageTakenPenalty, ShooterFitness.FitnessKeys.DamageTakenPenalty.ToString());
                }

                opponentsHealthSum = Agents.Where(a => a.TeamIdentifier.TeamID != agent.TeamIdentifier.TeamID).Sum(a => AgentStartHealth) - AgentStartHealth; // - AgentStartHealth because agent can't hit itself
                // FriendlyFire penalty
                if (agent.BulletsHitTeamMate > 0)
                {
                    friendlyFirePenalty = agent.BulletsHitTeamMate / (float)opponentsHealthSum;
                    if(friendlyFirePenalty > 1)
                        friendlyFirePenalty = 1f;
                    friendlyFirePenalty = (float)Math.Round(ShooterFitness.FitnessValues[ShooterFitness.FitnessKeys.FriendlyFirePenalty.ToString()] * friendlyFirePenalty, 4);
                    agent.AgentFitness.UpdateFitness(friendlyFirePenalty, ShooterFitness.FitnessKeys.FriendlyFirePenalty.ToString());
                }

                string agentFitnessLog = "========================================\n" +
                    $"[Agent]: Team ID + {agent.TeamIdentifier.TeamID} , ID: " + agent.IndividualID + "\n" +
                    $"[Sectors explored]: " + agent.SectorsExplored + " / " + Sectors.Length + " = " + sectorExplorationFitness + "\n" +
                    $"[Weapon item pick up]: " + (agent.HasWeapon() ? "Yes" : "No") + " = " + (agent.HasWeapon() ? ShooterFitness.FitnessValues[ShooterFitness.FitnessKeys.WeaponItemPickUp.ToString()] : 0) + "\n" +
                    $"[Bullets fired]: " + agent.BulletsFired + " / " + allPossibleBulletsFired + " = " + bulletsFired + "\n" +
                    $"[Bullets fired accuracy]: " + agent.BulletsHitOpponent + " / " + agent.BulletsFired + " = " + bulletsFiredAccuracy + "\n" +
                    $"[Opponents defeated]: " + agent.OpponentsDefeated + " / " + numOfOpponents + " = " + opponentsDefeatedBonus + "\n" +
                    $"[Damage taken]: " + agent.HitByOpponentBullets + " / " + AgentStartHealth + " = " + damageTakenPenalty + "\n" +
                    $"[Survival bonus]: " + (agent.SurvivedSimulationSteps == -1 ? SimulationSteps : agent.SurvivedSimulationSteps) + " / " + SimulationSteps + " = " + survivalBonus + "\n" +
                    $"[Friendly fire penalty]: " + agent.BulletsHitTeamMate + " / " + opponentsHealthSum + " = " + (agent.BulletsHitTeamMate > 0 ? ShooterFitness.FitnessValues[ShooterFitness.FitnessKeys.FriendlyFirePenalty.ToString()] * (agent.BulletsHitTeamMate / (float)allPossibleBulletsFired) : 0) + "\n" +
                    "========================================\n";

                DebugSystem.LogVerbose(agentFitnessLog);
            }
        }

        public void ReadParamsFromMainConfiguration()
        {
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                MainConfiguration conf = MenuManager.Instance.MainConfiguration;

                ShooterFitness.FitnessValues = conf.FitnessValues;

                if (conf.ProblemConfiguration.ContainsKey("AgentMoveSpeed"))
                {
                    AgentMoveSpeed = float.Parse(conf.ProblemConfiguration["AgentMoveSpeed"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentRotationSpeed"))
                {
                    AgentRotationSpeed = float.Parse(conf.ProblemConfiguration["AgentRotationSpeed"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("BulletShootCooldown"))
                {
                    BulletShootCooldown = int.Parse(conf.ProblemConfiguration["BulletShootCooldown"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("BulletSpeed"))
                {
                    BulletSpeed = float.Parse(conf.ProblemConfiguration["BulletSpeed"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("BulletDamage"))
                {
                    BulletDamage = int.Parse(conf.ProblemConfiguration["BulletDamage"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentStartHealth"))
                {
                    AgentStartHealth = int.Parse(conf.ProblemConfiguration["AgentStartHealth"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentStartShield"))
                {
                    AgentStartShield = int.Parse(conf.ProblemConfiguration["AgentStartShield"]);
                }

                if (conf.ProblemConfiguration.ContainsKey("AgentMaxShield"))
                {
                    AgentMaxShield = int.Parse(conf.ProblemConfiguration["AgentMaxShield"]);
                }
            }
        }
    }
}