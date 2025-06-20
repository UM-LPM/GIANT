using AgentControllers;
using AgentOrganizations;
using Base;
using Problems.PlanetConquest;
using Problems.Robostrike;
using Spawners;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Problems.PlanetConquest2
{
    public class PlanetConquest2MatchSpawner : MatchSpawner
    {
        [Header("1 vs Fixed Opponent Match Configuration")]
        [SerializeField] public Transform[] SpawnPoints;


        // Temporary variables
        private PlanetConquest2EnvironmentController planetConquestEnvironmentController;
        bool isFixedOpponent = false;

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
            if ((environmentController as PlanetConquest2EnvironmentController).LavaAgentPrefab == null)
            {
                throw new System.Exception("LavaAgentPrefab is not defined");
                // TODO Add error reporting here
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            planetConquestEnvironmentController = environmentController as PlanetConquest2EnvironmentController;

            List<T> agents = new List<T>();

            // Spawn agents
            for (int i = 0; i < environmentController.Match.Teams.Length; i++)
            {

                foreach (Individual individual in environmentController.Match.Teams[i].Individuals)
                {
                    foreach (AgentController agentController in individual.AgentControllers)
                    {
                        // Instantiate and configure agent
                        GameObject agentGameObject = Instantiate(planetConquestEnvironmentController.LavaAgentPrefab, SpawnPoints[i].position, SpawnPoints[i].rotation, gameObject.transform);

                        T agent = agentGameObject.GetComponent<T>();
                        PlanetConquest2AgentComponent agentComponent = agent as PlanetConquest2AgentComponent;
                        agentComponent.AgentController = agentController.Clone();
                        agentComponent.IndividualID = individual.IndividualId;
                        agentComponent.TeamIdentifier.TeamID = environmentController.Match.Teams[i].TeamId;
                        agentComponent.HealthComponent.Health = planetConquestEnvironmentController.AgentStartHealth;
                        agentComponent.EnergyComponent.Energy = planetConquestEnvironmentController.AgentStartEnergy;

                        // Set agent color based on team
                        if (i < planetConquestEnvironmentController.TeamColors.Length)
                        {
                            SpriteRenderer sp = agentGameObject.GetComponent<SpriteRenderer>();
                            sp.color = planetConquestEnvironmentController.TeamColors[i];
                        }

                        (agent as PlanetConquest2AgentComponent).NumOfSpawns++;

                        // Update list
                        agents.Add(agent);
                    }
                }
            }

            return agents.ToArray();
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T[] respawnComponents)
        {
            throw new System.NotImplementedException();
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            throw new System.NotImplementedException();
        }
    }
}
