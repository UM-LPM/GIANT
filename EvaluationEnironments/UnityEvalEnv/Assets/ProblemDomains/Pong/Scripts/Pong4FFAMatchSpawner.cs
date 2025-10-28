using AgentControllers;
using AgentOrganizations;
using Base;
using Spawners;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Pong
{
    public class Pong4v4MatchSpawner : MatchSpawner
    {
        [Header("Pong 1vs1 Match Configuration")]
        [SerializeField] public Transform SpawnPointLeft;
        [SerializeField] public Transform SpawnPointRight;
        [SerializeField] public Transform SpawnPointTop;
        [SerializeField] public Transform SpawnPointBottom;

        Transform[] SpawnPoints;

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

            if (environmentController.Match.Teams.Length != 4)
            {
                throw new System.Exception("Match should have 4 teams");
            }

            if (environmentController.Match.Teams[0].Individuals.Length != 1 || environmentController.Match.Teams[1].Individuals.Length != 1)
            {
                throw new System.Exception("Each team should have 1 individual");
            }

            if (environmentController.Match.Teams[0].Individuals[0].AgentControllers.Length > 1 || environmentController.Match.Teams[1].Individuals[0].AgentControllers.Length > 1)
            {
                throw new System.Exception("Each individual should have 1 agent controller");
            }

            if (SpawnPointLeft == null || SpawnPointRight == null)
            {
                throw new System.Exception("Spawn points are not defined correctly");
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> agents = new List<T>();

            SpawnPoints = new Transform[] { SpawnPointLeft, SpawnPointRight, SpawnPointTop, SpawnPointBottom };
            // Spawn agents
            for (int i = 0; i < environmentController.Match.Teams.Length; i++)
            {
                foreach (Individual individual in environmentController.Match.Teams[i].Individuals)
                {
                    agents.Add(SpawnAgent<T>(environmentController as PongEnvironmentController, SpawnPoints[i].position, SpawnPoints[i].rotation, individual.AgentControllers[0], individual.IndividualId, environmentController.Match.Teams[i].TeamId, i));
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
            foreach (PongAgentComponent agent in environmentController.Agents)
            {
                agent.transform.position = agent.StartPosition;
                agent.transform.rotation = agent.StartRotation;

            }
        }

        private T SpawnAgent<T>(PongEnvironmentController environmentController, Vector3 spawnPosition, Quaternion spawnRotation, AgentController agentController, int individualId, int teamId, int sideIndex)
        {
            // Instantiate and configure agent
            GameObject agentGameObject = Instantiate(environmentController.AgentPrefab, spawnPosition, spawnRotation, gameObject.transform);

            // Configure agent
            T agent = agentGameObject.GetComponent<T>();
            PongAgentComponent agentComponent = agent as PongAgentComponent;
            agentComponent.AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
            agentComponent.IndividualID = individualId;
            agentComponent.TeamIdentifier.TeamID = teamId;
            agentComponent.Side = (PongUtils.PongSide)sideIndex;

            if(agentComponent.Side == PongUtils.PongSide.Left || agentComponent.Side == PongUtils.PongSide.Right)
            {
                agentComponent.Forward = Vector3.down;
            }
            else if (agentComponent.Side == PongUtils.PongSide.Top)
            {
                agentComponent.Forward = Vector3.left;
            }
            else
            {
                agentComponent.Forward = Vector3.right;
            }

            // Update list
            return agent;
        }
    }
}