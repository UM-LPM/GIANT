using AgentControllers;
using AgentOrganizations;
using Base;
using Spawners;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Shooter
{
    public class Shooter3v3MatchSpawner : MatchSpawner
    {
        [Header("Shooter 3vs3 Match Configuration")]
        [SerializeField] public Transform[] SpawnPoints_Team1;
        [SerializeField] public Transform[] SpawnPoints_Team2;

        Transform[][] SpawnPoints;

        // Temp variables
        Vector3 respawnPos = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        List<Vector3> agentPositions;

        Color teamColor;
        Color[] teamColors = new Color[] {
            new Color(0.2352941f, 0.5485078f, 0.9058824f), // Blue tones
            new Color(0.8256133f, 0.2352941f, 0.7211952f), // Pink tones
            new Color(0.9058824f, 0.5485078f, 0.2352941f), // Orange tones
            new Color(0.9058824f, 0.2352941f, 0.2352941f), // Red tones
            };

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T[] respawnComponents)
        {
            throw new System.NotImplementedException();
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            throw new System.NotImplementedException();
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> agents = new List<T>();

            SpawnPoints = new Transform[][] { SpawnPoints_Team1, SpawnPoints_Team2 };
            // Spawn agents
            for (int i = 0; i < environmentController.Match.Teams.Length; i++)
            {
                if (environmentController.Match.Teams[i].Individuals[0].AgentControllers.Length == 1 && environmentController.Match.Teams[i].Individuals[1].AgentControllers.Length == 1)
                {
                    // Spawn first agent
                    agents.Add(SpawnAgent<T>(environmentController as ShooterEnvironmentController, SpawnPoints[i][0].position, SpawnPoints[i][0].rotation, environmentController.Match.Teams[i].Individuals[0].AgentControllers[0], environmentController.Match.Teams[i].Individuals[0].IndividualId, environmentController.Match.Teams[i].TeamId, i));
                    // Spawn second agent
                    agents.Add(SpawnAgent<T>(environmentController as ShooterEnvironmentController, SpawnPoints[i][1].position, SpawnPoints[i][1].rotation, environmentController.Match.Teams[i].Individuals[1].AgentControllers[0], environmentController.Match.Teams[i].Individuals[1].IndividualId, environmentController.Match.Teams[i].TeamId, i));
                    // Spawn third agent
                    agents.Add(SpawnAgent<T>(environmentController as ShooterEnvironmentController, SpawnPoints[i][2].position, SpawnPoints[i][2].rotation, environmentController.Match.Teams[i].Individuals[2].AgentControllers[0], environmentController.Match.Teams[i].Individuals[2].IndividualId, environmentController.Match.Teams[i].TeamId, i));
                }
                else
                {
                    throw new System.Exception("Each individual should have 1 agent controller");
                }
            }

            return agents.ToArray();
        }

        public T SpawnAgent<T>(ShooterEnvironmentController environmentController, Vector3 spawnPosition, Quaternion spawnRotation, AgentController agentController, int individualId, int teamId, int teamIndex) where T : Component
        {
            // Instantiate and configure agent
            GameObject agentGameObject = Instantiate(environmentController.AgentPrefab, spawnPosition, spawnRotation, gameObject.transform);

            T agent = agentGameObject.GetComponent<T>();

            // Configure agent
            ShooterAgentComponent agentComponent = agent as ShooterAgentComponent;
            agentComponent.AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
            agentComponent.IndividualID = individualId;
            agentComponent.TeamIdentifier.TeamID = teamId;

            agentComponent.SetTeamColor(teamColors[teamIndex]);

            return agent;
        }


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

            if (environmentController.Match.Teams[0].Individuals.Length != 3 || environmentController.Match.Teams[1].Individuals.Length != 3)
            {
                throw new System.Exception("Each team should have 3 individual");
            }

            if (environmentController.Match.Teams[0].Individuals[0].AgentControllers.Length > 1 || environmentController.Match.Teams[1].Individuals[0].AgentControllers.Length > 1)
            {
                throw new System.Exception("Each individual should have 1 controllers");
            }

            if (SpawnPoints_Team1 == null || SpawnPoints_Team1.Length != 3 || SpawnPoints_Team2 == null || SpawnPoints_Team2.Length != 3)
            {
                throw new System.Exception("Spawn points are not defined correctly");
            }
        }
    }
}