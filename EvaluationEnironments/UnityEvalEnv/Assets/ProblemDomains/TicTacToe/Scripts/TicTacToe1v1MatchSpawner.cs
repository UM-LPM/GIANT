using AgentControllers;
using AgentOrganizations;
using Base;
using Problems;
using Problems.TicTacToe;
using Spawners;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.TicTacToe
{
    public class TicTacToe1v1MatchSpawner : MatchSpawner
    {
        [Header("Tic Tac Toe Match Configuration")]
        [SerializeField] public Transform[] SpawnPositions;

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> agents = new List<T>();

            // Spawn agents
            for (int i = 0; i < environmentController.Match.Teams.Length; i++)
            {
                foreach (Individual individual in environmentController.Match.Teams[i].Individuals)
                {
                    agents.Add(SpawnAgent<T>(environmentController as TicTacToeEnvironmentController, SpawnPositions[i].position, SpawnPositions[i].rotation, individual.AgentControllers[0], individual.IndividualId, environmentController.Match.Teams[i].TeamId, i));
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

            if (environmentController.Match.Teams[0].Individuals[0].AgentControllers.Length > 1 || environmentController.Match.Teams[1].Individuals[0].AgentControllers.Length > 1)
            {
                throw new System.Exception("Each individual should have 1 agent controllers");
            }

            if (SpawnPositions == null || SpawnPositions.Length != 2)
            {
                throw new System.Exception("Spawn points are not defined correctly");
            }
        }

        private T SpawnAgent<T>(TicTacToeEnvironmentController environmentController, Vector3 spawnPosition, Quaternion spawnRotation, AgentController agentController, int individualId, int teamId, int teamIndex)
        {
            // Instantiate and configure agent
            GameObject agentGameObject = Instantiate(environmentController.AgentPrefab, spawnPosition, spawnRotation, gameObject.transform);

            // Configure agent
            T agent = agentGameObject.GetComponent<T>();
            TicTacToeAgentComponent agentComponent = agent as TicTacToeAgentComponent;
            agentComponent.AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
            agentComponent.IndividualID = individualId;
            agentComponent.TeamIdentifier.TeamID = teamId;

            // Update list
            return agent;
        }
    }
}
