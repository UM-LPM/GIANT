using AgentOrganizations;
using Base;
using Problems.Collector;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Spawners
{
    public class DummyMatchSpawner : MatchSpawner
    {
        public Individual Opponent;

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            if (environmentController == null)
            {
                throw new System.Exception("EnvironmentController is not defined");
            }
            if (!(environmentController is CollectorEnvironmentController))
            {
                throw new System.Exception("EnvironmentController is not of type CollectorEnvironmentController");
            }
            if (environmentController.Match == null)
            {
                throw new System.Exception("Match is not defined");
            }
            if (environmentController.AgentPrefab == null)
            {
                throw new System.Exception("AgentPrefab is not defined");
            }

            if(Opponent == null)
            {
                throw new System.Exception("Opponent is not defined");
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> agents = new List<T>();

            // Randomly spawn agents in the environment
            List<Vector3> usedSpawnPoints = new List<Vector3>();
            Vector3 spawnPos;
            Quaternion rotation;

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
                                usedSpawnPoints,
                                environmentController.AgentColliderExtendsMultiplier,
                                environmentController.MinAgentDistance,
                                true,
                                environmentController.gameObject.layer))
                            {
                                continue;
                            }

                            GameObject obj = Instantiate(environmentController.AgentPrefab, spawnPos, rotation, gameObject.transform);
                            if (environmentController.RandomTeamColor)
                            {
                                //Apply random color to the agent
                                obj.GetComponent<Renderer>().material = material;
                            }

                            T agent = obj.GetComponent<T>();
                            (agent as AgentComponent).AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
                            (agent as AgentComponent).IndividualID = individual.IndividualId;
                            (agent as AgentComponent).TeamIdentifier.TeamID = team.TeamId;
                            usedSpawnPoints.Add(spawnPos);

                            agents.Add(agent);
                            break;
                        }

                    }
                }
            }

            agents.AddRange(SpawnOpponent<T>(environmentController, usedSpawnPoints));

            return agents.ToArray();
        }

        private List<T> SpawnOpponent<T>(EnvironmentControllerBase environmentController, List<Vector3> usedSpawnPoints) where T: Component
        {
            List<T> opponentAgents = new List<T>();

            // Randomly spawn opponents in the environment
            Vector3 spawnPos;
            Quaternion rotation;

            foreach (var agentController in Opponent.AgentControllers)
            {
                Material material = new Material(Shader.Find("Standard"));
                material.color = new Color(Random.value, Random.value, Random.value, 1.0f);

                while (true)
                {
                    // Get random spawn point and rotation
                    spawnPos = GetRandomSpawnPoint(environmentController.Util,environmentController.GameType,environmentController.ArenaSize,environmentController.ArenaRadius,environmentController.ArenaCenterPoint,environmentController.ArenaOffset);

                    rotation = GetRandomRotation(environmentController.Util, environmentController.GameType);

                    // Validate spawn point
                    if (!SpawnPointSuitable(
                        environmentController.PhysicsScene,
                        environmentController.PhysicsScene2D,
                        environmentController.GameType,
                        spawnPos,
                        rotation,
                        usedSpawnPoints,
                        environmentController.AgentColliderExtendsMultiplier,
                        environmentController.MinAgentDistance,
                        true,
                        environmentController.gameObject.layer))
                    {
                        continue;
                    }

                    // Instantiate Agent and set layer
                    GameObject obj = Instantiate(environmentController.AgentPrefab, spawnPos, rotation, gameObject.transform);
                    if (environmentController.RandomTeamColor)
                    {
                        //Apply random color to the agent
                        obj.GetComponent<Renderer>().material = material;
                    }

                    // Configure agent
                    T agent = obj.GetComponent<T>();
                    AgentComponent agentComponent = agent as AgentComponent;
                    agentComponent.AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
                    agentComponent.IndividualID = Opponent.IndividualId;
                    agentComponent.TeamIdentifier.TeamID = -1;

                    // Upate lists
                    usedSpawnPoints.Add(spawnPos);
                    opponentAgents.Add(agent);
                    break;
                }
            }

            return opponentAgents;
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