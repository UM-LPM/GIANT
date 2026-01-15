using AgentControllers;
using Base;
using Spawners;
using System.Collections.Generic;
using AgentOrganizations;
using UnityEngine;

namespace Problems.DodgeBall
{
    public class DodgeBall2v2MatchSpawner : MatchSpawner
    {
        [Header("DodgeBall 2vs2 Match Configuration")]
        [SerializeField] public Transform[] SpawnPointsTeamA;
        [SerializeField] public Transform[] SpawnPointsTeamB;

        [Header("DodgeBall 2vs2 Match Agent Configuration")]
        [SerializeField] public Color ColorTeamA;
        [SerializeField] public Color ColorTeamB;

        Transform[][] SpawnPoints;

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> agents = new List<T>();

            SpawnPoints = new Transform[][] { SpawnPointsTeamA, SpawnPointsTeamB };

            for (int i = 0; i < environmentController.Match.Teams.Length; i++)
            {
                if (environmentController.Match.Teams[i].Individuals[0].AgentControllers.Length == 1 && environmentController.Match.Teams[i].Individuals[1].AgentControllers.Length == 1)
                {
                    // Spawn first agent
                    agents.Add(SpawnAgent<T>(environmentController as DodgeBallEnvironmentController, SpawnPoints[i][0].position, SpawnPoints[i][0].rotation, environmentController.Match.Teams[i].Individuals[0].AgentControllers[0], environmentController.Match.Teams[i].Individuals[0].IndividualId, environmentController.Match.Teams[i].TeamId, i));
                    // Spawn second agent
                    agents.Add(SpawnAgent<T>(environmentController as DodgeBallEnvironmentController, SpawnPoints[i][1].position, SpawnPoints[i][1].rotation, environmentController.Match.Teams[i].Individuals[1].AgentControllers[0], environmentController.Match.Teams[i].Individuals[1].IndividualId, environmentController.Match.Teams[i].TeamId, i));
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
            DodgeBallAgentComponent agent = respawnComponent as DodgeBallAgentComponent;

            if (agent)
            {
                agent.ResetAgent();
                agent.NumOfSpawns++;
            }
            else
            {
                throw new System.Exception("Respawn component is not of type DodgeBallAgentComponent");
            }
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T[] respawnComponents)
        {
            throw new System.NotImplementedException();
        }

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            if (!environmentController.AgentPrefab)
            {
                throw new System.Exception("AgentPrefab is not defined");
            }

            if (environmentController.Match.Teams.Length == 0)
            {
                    throw new System.Exception("No teams defined in the match");
            }

            if (environmentController.Match.Teams.Length != 2)
            {
                throw new System.Exception("DodgeBall2v2MatchSpawner requires exactly 2 teams");
            }

            if (environmentController.Match.Teams[0].Individuals.Length != 2 || environmentController.Match.Teams[1].Individuals.Length != 2)
            {
                throw new System.Exception("Each team must have 2 individual");
            }

            if (SpawnPointsTeamA == null || SpawnPointsTeamA.Length != 2 || SpawnPointsTeamB == null || SpawnPointsTeamB.Length != 2)
            {
                throw new System.Exception("Spawn points are not defined correctly");
            }
        }

        private T SpawnAgent<T>(DodgeBallEnvironmentController environmentController, Vector3 spawnPosition, Quaternion spawnRotation, AgentController agentController, int individualId, int teamId, int teamIndex)
        {
            // Instantiate and configure agent
            GameObject agentGameObject = Instantiate(environmentController.AgentPrefab, spawnPosition, spawnRotation, gameObject.transform);

            // Configure agent
            T agent = agentGameObject.GetComponent<T>();
            DodgeBallAgentComponent agentComponent = agent as DodgeBallAgentComponent;
            agentComponent.AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
            agentComponent.IndividualID = individualId;
            agentComponent.TeamIdentifier.TeamID = teamId;

            agentComponent.NumOfSpawns++;

            // Configure agent material
            if (teamIndex == 0)
            {
                agentGameObject.GetComponentInChildren<HeadComponent>()!.GetComponent<Renderer>().material.color = ColorTeamA;
            }
            else
            {
                agentGameObject.GetComponentInChildren<HeadComponent>()!.GetComponent<Renderer>().material.color = ColorTeamB;
            }

            // Update list
            return agent;
        }
    }
}
