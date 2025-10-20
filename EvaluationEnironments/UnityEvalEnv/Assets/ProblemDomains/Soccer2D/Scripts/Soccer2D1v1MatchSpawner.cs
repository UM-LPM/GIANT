using AgentControllers;
using AgentControllers.AIAgentControllers;
using AgentOrganizations;
using Base;
using Spawners;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Soccer2D
{
    public class Soccer2D1vs1MatchSpawner : MatchSpawner
    {
        [Header("Soccer 1vs1 Match Configuration")]
        [SerializeField] public Transform[] SpawnPointsBlue;
        [SerializeField] public Transform[] SpawnPointsPurple;

        [Header("Soccer 1vs1 Match Agent Configuration")]
        [SerializeField] public Color SoccerTeamColorBlue;
        [SerializeField] public Color SoccerTeamColorPurple;

        Transform[][] SpawnPoints;
        private Rigidbody rb;

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

            if (environmentController.Match.Teams.Length != 2)
            {
                throw new System.Exception("Match should have 2 teams");
            }

            if (environmentController.Match.Teams[0].Individuals.Length != 1 || environmentController.Match.Teams[1].Individuals.Length != 1)
            {
                throw new System.Exception("Each team should have 1 individual");
            }

            if (environmentController.Match.Teams[0].Individuals[0].AgentControllers.Length > 2 || environmentController.Match.Teams[1].Individuals[0].AgentControllers.Length > 2)
            {
                throw new System.Exception("Each individual should have 1 or 2 agent controllers");
            }

            if (SpawnPointsBlue == null || SpawnPointsBlue.Length != 2 || SpawnPointsPurple == null || SpawnPointsPurple.Length != 2)
            {
                throw new System.Exception("Spawn points are not defined correctly");
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> agents = new List<T>();

            SpawnPoints = new Transform[][] { SpawnPointsBlue, SpawnPointsPurple };
            // Spawn agents
            for (int i = 0; i < environmentController.Match.Teams.Length; i++)
            {
                foreach (Individual individual in environmentController.Match.Teams[i].Individuals)
                {
                    if (individual.AgentControllers.Length == 1)
                    {
                        for (int j = 0; j < SpawnPoints.Length; j++)
                        {
                            // Spawn agent
                            agents.Add(SpawnAgent<T>(environmentController as Soccer2DEnvironmentController, SpawnPoints[i][j].position, SpawnPoints[i][j].rotation, individual.AgentControllers[0], individual.IndividualId, environmentController.Match.Teams[i].TeamId, i));
                        }
                    }
                    else if (individual.AgentControllers.Length == 2)
                    {
                        // Spawn first agent
                        agents.Add(SpawnAgent<T>(environmentController as Soccer2DEnvironmentController, SpawnPoints[i][0].position, SpawnPoints[i][0].rotation, individual.AgentControllers[0], individual.IndividualId, environmentController.Match.Teams[i].TeamId, i));
                        // Spawn second agent
                        agents.Add(SpawnAgent<T>(environmentController as Soccer2DEnvironmentController, SpawnPoints[i][1].position, SpawnPoints[i][1].rotation, individual.AgentControllers[1], individual.IndividualId, environmentController.Match.Teams[i].TeamId, i));
                    }
                    else
                    {
                        throw new System.Exception("Each individual should have 1 or 2 agent controllers");
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
            foreach (Soccer2DAgentComponent agent in environmentController.Agents)
            {
                agent.transform.position = agent.StartPosition;
                agent.transform.rotation = agent.StartRotation;

            }
        }

        private T SpawnAgent<T>(Soccer2DEnvironmentController environmentController, Vector3 spawnPosition, Quaternion spawnRotation, AgentController agentController, int individualId, int teamId, int teamIndex)
        {
            // Instantiate and configure agent
            GameObject agentGameObject = Instantiate(environmentController.AgentPrefab, spawnPosition, spawnRotation, gameObject.transform);

            // Configure agent
            T agent = agentGameObject.GetComponent<T>();
            Soccer2DAgentComponent agentComponent = agent as Soccer2DAgentComponent;
            agentComponent.AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
            agentComponent.IndividualID = individualId;
            agentComponent.TeamIdentifier.TeamID = teamId;
            agentComponent.Team = teamIndex == 0 ? Soccer2DUtils.SoccerTeam.Blue : Soccer2DUtils.SoccerTeam.Purple;
            agentComponent.ResetVelocity();

            // Configure agent material
            if (teamIndex == 0)
            {
                agentGameObject.GetComponentInChildren<HeadComponent>()!.GetComponent<SpriteRenderer>().color = SoccerTeamColorBlue;
            }
            else
            {
                agentGameObject.GetComponentInChildren<HeadComponent>()!.GetComponent<SpriteRenderer>().color = SoccerTeamColorPurple;
            }

            // Update list
            return agent;
        }
    }
}