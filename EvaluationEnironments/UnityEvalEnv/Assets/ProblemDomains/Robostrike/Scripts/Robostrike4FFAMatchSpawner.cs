using AgentControllers;
using AgentOrganizations;
using Base;
using Problems.Collector;
using Spawners;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

namespace Problems.Robostrike
{
    public class Robostrike4FFAMatchSpawner : MatchSpawner
    {
        [Header("Robostrike 0vs4 Match Configuration")]
        [SerializeField] public Transform[] SpawnPoints;

        // Respawn variables
        Vector3 respawnPos = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        List<Vector3> agentPositions;

        Color teamColor;
        Color[] teamColors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.white, Color.black, Color.gray, Color.grey };

        bool isFarEnough;
        int counter = 0;
        int maxSpawnPoints = 100;

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            if (environmentController.AgentPrefab == null)
            {
                throw new System.Exception("AgentPrefab is not defined");
            }

            if (environmentController.Match == null || environmentController.Match.Teams == null)
            {
                throw new System.Exception("Match is not defined");
            }

            if (environmentController.Match.Teams.Length != 4)
            {
                throw new System.Exception("Match should have 4 teams");
            }

            if (environmentController.Match.Teams[0].Individuals.Length != 1 || environmentController.Match.Teams[1].Individuals.Length != 1)
            {
                throw new System.Exception("Each team should have 1 individual");
            }

            if (environmentController.Match.Teams[0].Individuals[0].AgentControllers.Length != 1 || environmentController.Match.Teams[1].Individuals[0].AgentControllers.Length != 1)
            {
                throw new System.Exception("Each individual should have 1 agent controller");
            }

            if (SpawnPoints == null || SpawnPoints.Length != 4)
            {
                throw new System.Exception("Spawn points are not defined correctly");
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> agents = new List<T>();

            // Spawn agents
            for (int i = 0; i < environmentController.Match.Teams.Length; i++)
            {
                // Define team color (Random color from (0,0,0) to (255,255,255))
                teamColor = teamColors[environmentController.Util.NextInt(0, teamColors.Length - 1)];

                foreach (Individual individual in environmentController.Match.Teams[i].Individuals)
                {
                    foreach(AgentController agentController in individual.AgentControllers)
                    {
                        // Instantiate and configure agent
                        GameObject agentGameObject = Instantiate(environmentController.AgentPrefab, SpawnPoints[i].position, SpawnPoints[i].rotation, gameObject.transform);

                        // Configure agent
                        T agent = agentGameObject.GetComponent<T>();
                        AgentComponent agentComponent = agent as AgentComponent;
                        agentComponent.AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
                        agentComponent.IndividualID = individual.IndividualId;
                        agentComponent.TeamIdentifier.TeamID = environmentController.Match.Teams[i].TeamId;

                        // Set agent's team color
                        (agent as RobostrikeAgentComponent).SetTeamColor(teamColor);

                        // Update list
                        agents.Add(agent);

                        (agent as RobostrikeAgentComponent).NumOfSpawns++;
                    }
                }
            }

            return agents.ToArray();
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            if(!(respawnComponent is AgentComponent))
            {
                throw new System.Exception("Invalid respawn component");
            }

            RobostrikeEnvironmentController robostrikeEnvironmentController = environmentController as RobostrikeEnvironmentController;
            AgentComponent agent = respawnComponent as AgentComponent;

            respawnPos = Vector3.zero;
            rotation = Quaternion.identity;

            if (robostrikeEnvironmentController.AgentRespawnType == RobostrikeAgentRespawnType.StartPos)
            {
                respawnPos = agent.StartPosition;
                rotation = agent.StartRotation;
            }
            else if(robostrikeEnvironmentController.AgentRespawnType == RobostrikeAgentRespawnType.RandomPos)
            {
                GetRandomSpawnPositionAndRotation(robostrikeEnvironmentController, out respawnPos, out rotation);
            }
            else
            {
                throw new System.Exception("Invalid respawn type");
            }

            agent.transform.position = respawnPos;
            agent.transform.rotation = rotation;

            (agent as RobostrikeAgentComponent).NumOfSpawns++;
        }
    
        void GetRandomSpawnPositionAndRotation(RobostrikeEnvironmentController environmentController, out Vector3 spawnPos, out Quaternion rotation)
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

                rotation = GetRandomRotation(environmentController.Util, environmentController.GameType);

                if (!SpawnPointSuitable(
                            environmentController.PhysicsScene,
                            environmentController.PhysicsScene2D,
                            environmentController.GameType,
                            spawnPos,
                            rotation,
                            agentPositions,
                            environmentController.AgentColliderExtendsMultiplier,
                            environmentController.MinAgentDistance,
                            true,
                            environmentController.gameObject.layer))
                {
                    isFarEnough = false;
                }

                // Check if current spawn point is far enough from the agents
                foreach (AgentComponent agent in environmentController.Agents)
                {
                    if (Vector3.Distance(agent.transform.position, spawnPos) < environmentController.MinPowerUpDistanceFromAgents)
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