using AgentControllers;
using Base;
using Spawners;
using System.Collections;
using System.Collections.Generic;
using AgentOrganizations;
using UnityEngine;

namespace Problems.DodgeBall
{
    public class DodgeBall1v1MatchSpawner : MatchSpawner
    {
        [SerializeField] private Transform[] AgentSpawnPoints;

        Color teamColor;
        Color[] teamColors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.white, Color.black, Color.gray, Color.grey };


        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> agents = new List<T>();

            // Spawn agents
            for (int i = 0; i < environmentController.Match.Teams.Length; i++)
            {
                // Define team color (Random color from (0,0,0) to (255,255,255))
                teamColor = teamColors[environmentController.Util.NextInt(0, teamColors.Length - 1)];

                foreach (Individual individual in environmentController.Match.Teams[i].Individuals)
                {
                    foreach (AgentController agentController in individual.AgentControllers)
                    {
                        // Instantiate and configure agent
                        GameObject agentGameObject = Instantiate(environmentController.AgentPrefab, AgentSpawnPoints[i].position, AgentSpawnPoints[i].rotation, gameObject.transform);

                        // Configure agent
                        T agent = agentGameObject.GetComponent<T>();
                        DodgeBallAgentComponent agentComponent = agent as DodgeBallAgentComponent;
                        agentComponent.AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
                        agentComponent.IndividualID = individual.IndividualId;
                        agentComponent.TeamIdentifier.TeamID = environmentController.Match.Teams[i].TeamId;

                        agentGameObject.GetComponentInChildren<HeadComponent>()!.GetComponent<Renderer>().material.color = teamColor;

                        agentComponent.NumOfSpawns++;

                        // Update list
                        agents.Add(agent);
                    }
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
                throw new System.Exception("DodgeBall1v1MatchSpawner requires exactly 2 teams");
            }

            if (environmentController.Match.Teams[0].Individuals.Length != 1 || environmentController.Match.Teams[1].Individuals.Length != 1)
            {
                throw new System.Exception("Each team must have at one individual");
            }
        }

    }
}
