using AgentControllers;
using Base;
using Spawners;
using System.Collections.Generic;
using AgentOrganizations;
using UnityEngine;

namespace  Problems.BoxTact
{
    public class BoxTactMatchSpawner : MatchSpawner
    {
        [SerializeField] private Transform AgentSpawnPoint;
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
                foreach (Individual individual in environmentController.Match.Teams[i].Individuals)
                {
                    foreach (AgentController agentController in individual.AgentControllers)
                    {
                        // Instantiate and configure agent
                        GameObject agentGameObject = Instantiate(environmentController.AgentPrefab, AgentSpawnPoint.position, AgentSpawnPoint.rotation, gameObject.transform);

                        // Configure agent
                        T agent = agentGameObject.GetComponent<T>();
                        BoxTactAgentComponent agentComponent = agent as BoxTactAgentComponent;
                        agentComponent.AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
                        agentComponent.IndividualID = individual.IndividualId;
                        agentComponent.TeamIdentifier.TeamID = environmentController.Match.Teams[i].TeamId;

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
                // TODO add error reporting here
            }

            if(environmentController.Match == null || environmentController.Match.Teams == null)
            {
                throw new System.Exception("Match is not defined");
                // TODO add error reporting here
            }

            if (environmentController.Match.Teams.Length != 1)
            {
                throw new System.Exception("Match should have 1 team");
                // TODO add error reporting here
            }

            if (environmentController.Match.Teams[0].Individuals.Length != 1)
            {
                throw new System.Exception("Each team should have 1 individual");
                // TODO add error reporting here
            }

            if (environmentController.Match.Teams[0].Individuals[0].AgentControllers.Length != 1)
            {
                throw new System.Exception("Each individual should have 1 agent controller");
                // TODO add error reporting here
            }

            if (AgentSpawnPoint == null)
            {
                throw new System.Exception("Spawn point is not defined");
                // TODO add error reporting here
            }
        }
    }
}
