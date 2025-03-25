using UnityEngine;
using System.Collections.Generic;
using AgentControllers;
using AgentOrganizations;
using Base;

namespace Spawners
{
    /// <summary>
    /// Class that randomly spawns the individuals in the environment for the Dummy problem domain
    /// </summary>
    public class RandomMatchSpawner : MatchSpawner
    {
        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            if (environmentController == null)
            {
                throw new System.Exception("EnvironmentController is not defined");
                // TODO Add error reporting here
            }
            if (environmentController.Match == null)
            {
                throw new System.Exception("Match is not defined");
                // TODO Add error reporting here
            }
            if (environmentController.AgentPrefab == null)
            {
                throw new System.Exception("AgentPrefab is not defined");
                // TODO Add error reporting here
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> agents = new List<T>();

            // Randomly spawn all agents in the environment
            List<Vector3> usedSpawnPoints = new List<Vector3>();
            Vector3 spawnPos;
            Quaternion rotation;

            Renderer renderer;

            foreach (var team in environmentController.Match.Teams)
            {
                Material material = new Material(Shader.Find("Standard"));
                material.color = new Color(Random.value, Random.value, Random.value, 1.0f);

                foreach (var individual in team.Individuals)
                {
                    foreach (var agentController in individual.AgentControllers)
                    {
                        while (true)
                        {
                            // Get random spawn point and rotation
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

                            // Validate spawn point
                            if (!SpawnPointSuitable(
                                environmentController.GameType,
                                spawnPos,
                                rotation,
                                usedSpawnPoints,
                                environmentController.AgentColliderExtendsMultiplier,
                                environmentController.MinAgentDistance,
                                true,
                                environmentController.gameObject.layer,
                                environmentController.DefaultLayer))
                            {
                                continue;
                            }

                            // Instantiate agent and set layer
                            GameObject obj = Instantiate(environmentController.AgentPrefab, spawnPos, rotation, gameObject.transform);
                            if (environmentController.RandomTeamColor)
                            {
                                //Apply random color to the agent
                                renderer = obj.GetComponent<Renderer>();
                                if(renderer != null)
                                {
                                    renderer.material = material;
                                }
                            }

                            // Configure agent
                            T agent = obj.GetComponent<T>();
                            AgentComponent agentComponent = agent as AgentComponent;
                            agentComponent.AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
                            agentComponent.IndividualID = individual.IndividualId;
                            agentComponent.TeamIdentifier.TeamID = team.TeamId;

                            // Update lists
                            usedSpawnPoints.Add(spawnPos);
                            agents.Add(agent);
                            break;
                        }

                    }
                }
            }

            return agents.ToArray();
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            throw new System.NotImplementedException();
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T[] respawnComponents)
        {
            throw new System.NotImplementedException();
        }
    }
}