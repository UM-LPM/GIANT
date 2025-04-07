using AgentControllers;
using AgentOrganizations;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Spawners;
using System.Linq;
using Base;

namespace Problems.PlanetConquest
{
    public class PlanetConquest1vs1MatchSpawner : MatchSpawner
    {
        [Header("Moba_game 1vs1 Match Configuration")]
        [SerializeField] public Transform[] SpawnPoints;

        // Respawn variables
        Vector3 respawnPos = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        List<Vector3> agentPositions;

        bool isFarEnough;
        int counter = 0;
        int maxSpawnPoints = 100;

        PlanetConquestEnvironmentController planetConquestEnvironmentController;

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            if (environmentController.AgentPrefab == null)
            {
                UnityEngine.Debug.LogWarning("AgentPrefab is not defined");
            }

            if (environmentController.Match == null || environmentController.Match.Teams == null)
            {
                throw new System.Exception("Match is not defined");
                // TODO add error reporting here
            }

            if (environmentController.Match.Teams.Length != 2)
            {
                throw new System.Exception("Match should have 2 teams");
                // TODO add error reporting here
            }

            if (environmentController.Match.Teams[0].Individuals.Length != 1 || environmentController.Match.Teams[1].Individuals.Length != 1)
            {
                throw new System.Exception("Each team should have 1 individual");
                // TODO add error reporting here
            }

            if (environmentController.Match.Teams[0].Individuals[0].AgentControllers.Length != 1 || environmentController.Match.Teams[1].Individuals[0].AgentControllers.Length != 1)
            {
                throw new System.Exception("Each individual should have 1 agent controller");
                // TODO add error reporting here
            }

            if (SpawnPoints == null || SpawnPoints.Length < 2)
            {
                throw new System.Exception("Spawn points are not defined");
                // TODO add error reporting here
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> agents = new List<T>();

            // Spawn agents
            for (int i = 0; i < environmentController.Match.Teams.Length; i++)
            {
                foreach (Individual individual in environmentController.Match.Teams[i].Individuals)
                {
                    foreach (AgentController agentController in individual.AgentControllers)
                    {
                        // // Instantiate and configure agent
                        T agent = SpawnAgent<T>(environmentController, i, AgentType.Lava);

                        // Update list
                        agents.Add(agent);
                    }
                }
            }
            return agents.ToArray();
        }

        public T SpawnAgent<T>(EnvironmentControllerBase environmentController, int teamIndex, AgentType agentType)
        {
            validateSpawnConditions(environmentController);

            Individual individual = environmentController.Match.Teams[teamIndex].Individuals[0];
            AgentController agentController = individual.AgentControllers[0];
            GameObject agentGameObject;

            planetConquestEnvironmentController = environmentController as PlanetConquestEnvironmentController;

            switch (agentType)
            {
                case AgentType.Lava:
                    agentGameObject = Instantiate(planetConquestEnvironmentController.LavaAgentPrefab, SpawnPoints[teamIndex].position, SpawnPoints[teamIndex].rotation, gameObject.transform);
                    break;
                case AgentType.Ice:
                    agentGameObject = Instantiate(planetConquestEnvironmentController.IceAgentPrefab, SpawnPoints[teamIndex].position, SpawnPoints[teamIndex].rotation, gameObject.transform);
                    break;
                default:
                    throw new System.Exception("Invalid agent type");
                    // TODO add error reporting here
            }

            // Configure agent
            agentGameObject.layer = environmentController.gameObject.layer;

            T agent = agentGameObject.GetComponent<T>();
            PlanetConquestAgentComponent agentComponent = agent as PlanetConquestAgentComponent;
            agentComponent.AgentController = agentController.Clone();
            environmentController.ConfigureAgentController(agentComponent);
            agentComponent.IndividualID = individual.IndividualId;
            agentComponent.TeamIdentifier.TeamID = teamIndex;
            agentComponent.HealthComponent.Health = PlanetConquestEnvironmentController.MAX_HEALTH;
            agentComponent.EnergyComponent.Energy = PlanetConquestEnvironmentController.MAX_ENERGY;

            // Set agent color based on team
            if (teamIndex < planetConquestEnvironmentController.TeamColors.Length)
            {
                SpriteRenderer sp = agentGameObject.GetComponent<SpriteRenderer>();
                sp.color = planetConquestEnvironmentController.TeamColors[teamIndex];
            }

            (agent as PlanetConquestAgentComponent).NumOfSpawns++;
            return agent;
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            if (!(respawnComponent is AgentComponent))
            {
                throw new System.Exception("Invalid respawn component");
                // TODO add error reporting here
            }

            planetConquestEnvironmentController = environmentController as PlanetConquestEnvironmentController;
            AgentComponent agent = respawnComponent as AgentComponent;

            respawnPos = Vector3.zero;
            rotation = Quaternion.identity;

            if (planetConquestEnvironmentController.AgentRespawnType == PlanetConquestAgentRespawnType.StartPos)
            {
                respawnPos = agent.StartPosition;
                rotation = agent.StartRotation;
            }
            else if (planetConquestEnvironmentController.AgentRespawnType == PlanetConquestAgentRespawnType.RandomPos)
            {
                GetRandomSpawnPositionAndRotation(planetConquestEnvironmentController, out respawnPos, out rotation);
            }
            else
            {
                throw new System.Exception("Invalid respawn type");
                // TODO add error reporting here
            }

            agent.transform.position = respawnPos;
            agent.transform.rotation = rotation;

            (agent as PlanetConquestAgentComponent).NumOfSpawns++;
        }

        void GetRandomSpawnPositionAndRotation(PlanetConquestEnvironmentController environmentController, out Vector3 spawnPos, out Quaternion rotation)
        {
            counter = 0;
            agentPositions = environmentController.Agents.Select(agent => agent.transform.position).ToList();
            do
            {
                isFarEnough = true;
                spawnPos = GetRandomSpawnPoint(
                                environmentController.Util,
                                environmentController.GameType,
                                environmentController.ArenaSize,
                                environmentController.ArenaRadius,
                                environmentController.ArenaCenterPoint,
                                environmentController.ArenaOffset);
                if (environmentController.SceneLoadMode == SceneLoadMode.GridMode)
                    spawnPos += environmentController.GridCell.GridCellPosition;

                rotation = GetRandomRotation(environmentController.Util, environmentController.GameType);

                if (!SpawnPointSuitable(environmentController.GameType,
                            spawnPos,
                            rotation,
                            agentPositions,
                            environmentController.AgentColliderExtendsMultiplier,
                            environmentController.MinAgentDistance,
                            true,
                            environmentController.gameObject.layer,
                            environmentController.DefaultLayer))
                {
                    isFarEnough = false;
                }

                // Check if current spawn point is far enough from the agents
                foreach (AgentComponent agent in environmentController.Agents)
                {
                    if (Vector3.Distance(agent.transform.position, spawnPos) < environmentController.MinAgentDistance)
                    {
                        isFarEnough = false;
                        break;
                    }
                }

                if (counter >= maxSpawnPoints)
                    break;

                counter++;
            } while (!isFarEnough);
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T[] respawnComponents)
        {
            throw new System.NotImplementedException();
        }
    }
}