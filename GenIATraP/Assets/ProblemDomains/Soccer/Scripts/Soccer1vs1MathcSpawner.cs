using AgentControllers;
using AgentOrganizations;
using Base;
using Spawners;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Soccer
{
    public class Soccer1vs1MatchSpawner : MatchSpawner
    {
        [Header("Soccer 1vs1 Match Configuration")]
        [SerializeField] public Transform[] SpawnPointsBlue;
        [SerializeField] public Transform[] SpawnPointsPurple;

        [Header("Soccer 1vs1 Match Agent Configuration")]
        [SerializeField] public Material SoccerTeamMaterialBlue;
        [SerializeField] public Material SoccerTeamMaterialPurple;

        Transform[][] SpawnPoints;
        private Rigidbody rigidbody;

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

            if (SpawnPointsBlue == null || SpawnPointsBlue.Length != 2 || SpawnPointsPurple == null || SpawnPointsPurple.Length != 2)
            {
                throw new System.Exception("Spawn points are not defined correctly");
                // TODO add error reporting here
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
                    foreach (AgentController agentController in individual.AgentControllers)
                    {
                        for (int j = 0; j < SpawnPoints.Length; j++)
                        {
                            // Instantiate and configure agent
                            GameObject agentGameObject = Instantiate(environmentController.AgentPrefab, SpawnPoints[i][j].position, SpawnPoints[i][j].rotation, gameObject.transform);

                            // Configure agent
                            T agent = agentGameObject.GetComponent<T>();
                            AgentComponent agentComponent = agent as AgentComponent;
                            agentComponent.AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
                            agentComponent.IndividualID = individual.IndividualId;
                            agentComponent.TeamID = environmentController.Match.Teams[i].TeamId;

                            // Configure agent material
                            if (i == 0)
                            {
                                agentGameObject.GetComponent<Renderer>().material = SoccerTeamMaterialBlue;
                            }
                            else
                            {
                                agentGameObject.GetComponent<Renderer>().material = SoccerTeamMaterialPurple;
                            }

                            // Update list
                            agents.Add(agent);
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
            foreach(AgentComponent agent in environmentController.Agents)
            {
                agent.transform.position = agent.StartPosition;
                agent.transform.rotation = agent.StartRotation;

                rigidbody = agent.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    rigidbody.velocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                }
            }
        }
    }
}