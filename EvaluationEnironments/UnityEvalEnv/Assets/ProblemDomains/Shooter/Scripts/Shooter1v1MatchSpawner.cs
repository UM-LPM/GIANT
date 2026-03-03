using AgentControllers;
using AgentOrganizations;
using Base;
using Spawners;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Shooter
{
    public class Shooter1v1MatchSpawner : MatchSpawner
    {
        [Header("Shooter 1vs1 Match Configuration")]
        [SerializeField] public Transform[] SpawnPoints;

        // Respawn variables
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

            // Spawn agents
            for (int i = 0; i < environmentController.Match.Teams.Length; i++)
            {
                teamColor = teamColors[environmentController.Util.NextInt(0, teamColors.Length - 1)];

                foreach (Individual individual in environmentController.Match.Teams[i].Individuals)
                {
                    foreach (AgentController agentController in individual.AgentControllers)
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
                        (agent as ShooterAgentComponent).SetTeamColor(teamColor);

                        // Update list
                        agents.Add(agent);
                    }
                }
            }

            return agents.ToArray();
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

            if (environmentController.Match.Teams[0].Individuals.Length != 1 || environmentController.Match.Teams[1].Individuals.Length != 1)
            {
                throw new System.Exception("Each team should have 1 individual");
            }

            if (environmentController.Match.Teams[0].Individuals[0].AgentControllers.Length != 1 || environmentController.Match.Teams[1].Individuals[0].AgentControllers.Length != 1)
            {
                throw new System.Exception("Each individual should have 1 agent controller");
            }

            if (SpawnPoints == null || SpawnPoints.Length != 2)
            {
                throw new System.Exception("Spawn points are not defined correctly");
            }
        }
    }
}