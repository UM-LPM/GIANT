using AgentControllers;
using AgentOrganizations;
using Base;
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
        private Vector3 respawnPos = Vector3.zero;
        private Quaternion rotation = Quaternion.identity;

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
                        T agent = Spawn<T>(environmentController, environmentController.Match.Teams[i].TeamId, AgentType.Lava);

                        /*// Instantiate and configure agent
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

                        (agent as PlanetConquest2AgentComponent).NumOfSpawns++;*/

                        // Update list
                        agents.Add(agent);
                    }
                }
            }

            // Spawn fixed opponent if defined
            if (environmentController.Match.Teams.Length == 1)
            {
                if (planetConquestEnvironmentController.FixedOpponent == null || planetConquestEnvironmentController.FixedOpponent.AgentControllers.Length == 0)
                {
                    throw new System.Exception("Fixed opponent or FixedOpponent.AgentControllers is not defined");
                    // TODO add error reporting here
                }

                int teamId = environmentController.Match.Teams[0].TeamId + planetConquestEnvironmentController.FixedOpponentTeamId;
                int i = 1;
                foreach (AgentController agentController in planetConquestEnvironmentController.FixedOpponent.AgentControllers)
                {
                    T agent = Spawn<T>(environmentController, teamId, AgentType.Lava);

                    /*// Instantiate and configure agent
                    GameObject agentGameObject = Instantiate(planetConquestEnvironmentController.LavaAgentPrefab, SpawnPoints[i].position, SpawnPoints[i].rotation, gameObject.transform);

                    T agent = agentGameObject.GetComponent<T>();
                    PlanetConquest2AgentComponent agentComponent = agent as PlanetConquest2AgentComponent;
                    agentComponent.AgentController = agentController.Clone();
                    agentComponent.IndividualID = planetConquestEnvironmentController.FixedOpponent.IndividualId;
                    agentComponent.TeamIdentifier.TeamID = teamId;
                    agentComponent.HealthComponent.Health = planetConquestEnvironmentController.AgentStartHealth;
                    agentComponent.EnergyComponent.Energy = planetConquestEnvironmentController.AgentStartEnergy;

                    // Set agent color based on team
                    if (i < planetConquestEnvironmentController.TeamColors.Length)
                    {
                        SpriteRenderer sp = agentGameObject.GetComponent<SpriteRenderer>();
                        sp.color = planetConquestEnvironmentController.TeamColors[i];
                    }

                    (agent as PlanetConquest2AgentComponent).IsFixedOpponent = true;

                    (agent as PlanetConquest2AgentComponent).NumOfSpawns++;*/

                    // Update list
                    agents.Add(agent);
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
            if(!(respawnComponent is AgentComponent))
            {
                throw new System.Exception("Invalid respawn component");
                // TODO add error reporting here
            }

            planetConquestEnvironmentController = environmentController as PlanetConquest2EnvironmentController;
            AgentComponent agent = respawnComponent as AgentComponent;

            respawnPos = Vector3.zero;
            rotation = Quaternion.identity;

            respawnPos = agent.StartPosition;
            rotation = agent.StartRotation;

            agent.transform.position = respawnPos;
            agent.transform.rotation = rotation;

            (agent as PlanetConquest2AgentComponent).NumOfSpawns++;
        }

        public T Spawn<T>(EnvironmentControllerBase environmentController, int teamId, AgentType agentType)
        {
            Individual individual;

            int positionIndex = 0;
            if (teamId >= planetConquestEnvironmentController.FixedOpponentTeamId)
            {
                positionIndex = 1;
                individual = planetConquestEnvironmentController.FixedOpponent;
            }
            else {                 
                individual = environmentController.Match.Teams[positionIndex].Individuals[0];
            }

            // Instantiate and configure agent
            GameObject agentPrefab = agentType == AgentType.Lava ? planetConquestEnvironmentController.LavaAgentPrefab : planetConquestEnvironmentController.IceAgentPrefab;
            GameObject agentGameObject = Instantiate(agentPrefab, SpawnPoints[positionIndex].position, SpawnPoints[positionIndex].rotation, gameObject.transform);

            T agent = agentGameObject.GetComponent<T>();
            PlanetConquest2AgentComponent agentComponent = agent as PlanetConquest2AgentComponent;
            agentComponent.AgentController = individual.AgentControllers[0].Clone();
            agentComponent.IndividualID = individual.IndividualId;
            agentComponent.TeamIdentifier.TeamID = teamId;
            agentComponent.HealthComponent.Health = planetConquestEnvironmentController.AgentStartHealth;
            agentComponent.EnergyComponent.Energy = planetConquestEnvironmentController.AgentStartEnergy;

            environmentController.ConfigureAgentController(agentComponent);

            // Set agent color based on team
            if (positionIndex < planetConquestEnvironmentController.TeamColors.Length)
            {
                SpriteRenderer sp = agentGameObject.GetComponent<SpriteRenderer>();
                sp.color = planetConquestEnvironmentController.TeamColors[positionIndex];
            }

            (agent as PlanetConquest2AgentComponent).IsFixedOpponent = positionIndex == 1;

            (agent as PlanetConquest2AgentComponent).NumOfSpawns++;

            // Update list
            return agent;
        }
    }
}
