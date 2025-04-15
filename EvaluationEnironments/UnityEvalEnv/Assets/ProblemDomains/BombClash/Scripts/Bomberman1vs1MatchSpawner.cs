using AgentControllers;
using AgentOrganizations;
using Base;
using Spawners;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Bombclash
{
    public class Bomberman1vs1MatchSpawner : MatchSpawner
    {
        [Header("Bomberman 1vs1 Match Configuration")]
        [SerializeField] public Transform[] AgentSpawnPoints;

        // TODO
        //[Header("Bomberman 1vs1 Match Agent Configuration")]

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            if (environmentController.AgentPrefab == null)
            {
                throw new System.Exception("AgentPrefab is not defined");
                // TODO add error reporting here
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

            if (AgentSpawnPoints == null || AgentSpawnPoints.Length != 2)
            {
                throw new System.Exception("Spawn points are not defined correctly");
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
                        // Instantiate and configure agent
                        GameObject agentGameObject = Instantiate(environmentController.AgentPrefab, AgentSpawnPoints[i].position, AgentSpawnPoints[i].rotation, gameObject.transform);

                        // Configure agent
                        T agent = agentGameObject.GetComponent<T>();
                        BombermanAgentComponent agentComponent = agent as BombermanAgentComponent;
                        agentComponent.AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
                        agentComponent.IndividualID = individual.IndividualId;
                        agentComponent.TeamIdentifier.TeamID = environmentController.Match.Teams[i].TeamId;

                        // TODO Configure agent material

                        // Update list
                        agents.Add(agent);
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