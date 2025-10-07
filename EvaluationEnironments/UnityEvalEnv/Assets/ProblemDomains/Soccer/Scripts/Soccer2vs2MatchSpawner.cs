using AgentControllers;
using AgentControllers.AIAgentControllers;
using AgentOrganizations;
using Base;
using Spawners;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Soccer
{
    public class Soccer2vs2MatchSpawner : MatchSpawner
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
            }

            if (environmentController.Match == null || environmentController.Match.Teams == null)
            {
                throw new System.Exception("Match is not defined");
            }

            if (environmentController.Match.Teams.Length != 2)
            {
                throw new System.Exception("Match should have 2 teams");
            }

            if (environmentController.Match.Teams[0].Individuals.Length != 2 || environmentController.Match.Teams[1].Individuals.Length != 2)
            {
                throw new System.Exception("Each team should have 2 individuals");
            }

            if (environmentController.Match.Teams[0].Individuals[0].AgentControllers.Length > 1 || environmentController.Match.Teams[1].Individuals[0].AgentControllers.Length > 1)
            {
                throw new System.Exception("Each individual should have 1 agent controller");
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
                //foreach (Individual individual in environmentController.Match.Teams[i].Individuals)
                if (environmentController.Match.Teams[i].Individuals[0].AgentControllers.Length == 1 && environmentController.Match.Teams[i].Individuals[1].AgentControllers.Length == 1)
                {
                    // Spawn first agent
                    agents.Add(SpawnAgent<T>(environmentController as SoccerEnvironmentController, SpawnPoints[i][0].position, SpawnPoints[i][0].rotation, environmentController.Match.Teams[i].Individuals[0].AgentControllers[0], environmentController.Match.Teams[i].Individuals[0].IndividualId, environmentController.Match.Teams[i].TeamId, i));
                    // Spawn second agent
                    agents.Add(SpawnAgent<T>(environmentController as SoccerEnvironmentController, SpawnPoints[i][1].position, SpawnPoints[i][1].rotation, environmentController.Match.Teams[i].Individuals[1].AgentControllers[0], environmentController.Match.Teams[i].Individuals[1].IndividualId, environmentController.Match.Teams[i].TeamId, i));
                }
                else
                {
                    throw new System.Exception("Each individual should have 1 agent controller");
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
            foreach(SoccerAgentComponent agent in environmentController.Agents)
            {
                agent.transform.position = agent.StartPosition;
                agent.transform.rotation = agent.StartRotation;

                rigidbody = agent.Rigidbody;
                if (rigidbody != null)
                {
                    rigidbody.velocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;

                    rigidbody.position = agent.StartPosition;
                    rigidbody.rotation = agent.StartRotation;
                }
                else
                {
                    throw new System.Exception("Rigidbody not found on agent");
                }
            }
        }

        private T SpawnAgent<T>(SoccerEnvironmentController environmentController, Vector3 spawnPosition, Quaternion spawnRotation, AgentController agentController, int individualId, int teamId, int teamIndex)
        {
            // Instantiate and configure agent
            GameObject agentGameObject = Instantiate(environmentController.AgentPrefab, spawnPosition, spawnRotation, gameObject.transform);

            // Configure agent
            T agent = agentGameObject.GetComponent<T>();
            SoccerAgentComponent agentComponent = agent as SoccerAgentComponent;
            agentComponent.AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
            agentComponent.IndividualID = individualId;
            agentComponent.TeamIdentifier.TeamID = teamId;
            agentComponent.Team = teamIndex == 0 ? SoccerUtils.SoccerTeam.Blue : SoccerUtils.SoccerTeam.Purple;

            // Configure agent material
            if (teamIndex == 0)
            {
                agentGameObject.GetComponentInChildren<HeadComponent>()!.GetComponent<Renderer>().material = SoccerTeamMaterialBlue;
            }
            else
            {
                agentGameObject.GetComponentInChildren<HeadComponent>()!.GetComponent<Renderer>().material = SoccerTeamMaterialPurple;
            }

            // Update list
            return agent;
        }
    }
}