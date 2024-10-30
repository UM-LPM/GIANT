using UnityEngine;
using AgentOrganizations;
using System.Collections.Generic;
using Problems.Dummy;

/// <summary>
/// Class that randomly spawns the individuals in the environment for the Dummy problem domain
/// </summary>
public class RandomIndividualSpawner : IndividualSpawner
{
    public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
    {
        if(environmentController == null)
        {
            throw new System.Exception("EnvironmentController is not defined");
            // TODO Add error reporting here
        }
        if (!(environmentController is DummyEnvironmentController))
        {
            throw new System.Exception("EnvironmentController is not of type DummyEnvironmentController");
            // TODO Add error reporting here
        }
        if (environmentController.Match == null)
        {
            throw new System.Exception("Match is not defined");
            // TODO Add error reporting here
        }
        if (environmentController.AgentPrefab == null)
        {
            throw new System.Exception("AgentPrefab is not defined");
            // TODO Add error reporting here
        }
    }

    public override List<AgentComponent> SpawnIndividuals(EnvironmentControllerBase environmentController)
    {
        validateSpawnConditions(environmentController);

        List<AgentComponent> agents = new List<AgentComponent>();

        // Randomly spawn all agents in the environment
        List<Vector3> usedSpawnPoints = new List<Vector3>();
        Vector3 spawnPos;
        Quaternion rotation;

        foreach (var team in environmentController.Match.Teams)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(Random.value, Random.value, Random.value, 1.0f);

            foreach (var individual in team.Individuals)
            {
                foreach (var agentController in individual.AgentControllers)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        while (true)
                        {
                            spawnPos = GetRandomSpawnPoint(
                                environmentController.Util,
                                environmentController.GameType,
                                environmentController.ArenaSize,
                                environmentController.ArenaRadius,
                                environmentController.ArenaCenterPoint,
                                environmentController.ArenaOffset);

                            if (environmentController.SceneLoadMode == SceneLoadMode.GridMode)
                                spawnPos += environmentController.GridCell.GridCellPosition;

                            rotation = GetRandomRotation(environmentController.Util, environmentController.GameType);

                            if (!SpawnPointSuitable(
                                environmentController.GameType,
                                spawnPos,
                                rotation,
                                usedSpawnPoints,
                                environmentController.AgentColliderExtendsMultiplier,
                                environmentController.MinAgentDistance,
                                environmentController.gameObject.layer,
                                environmentController.DefaultLayer))
                            {
                                continue;
                            }

                            GameObject obj = Instantiate(environmentController.AgentPrefab, spawnPos, rotation, gameObject.transform);
                            if (environmentController.RandomTeamColor)
                            {
                                //Apply random color to the agent
                                obj.GetComponent<Renderer>().material = material;
                            }

                            AgentComponent agent = obj.GetComponent<AgentComponent>();
                            agent.AgentController = agentController.Clone(); // Clone the agent controller to prevent shared state between agents
                            agent.IndividualID = individual.IndividualId;
                            agent.TeamID = team.TeamId;
                            usedSpawnPoints.Add(spawnPos);

                            agents.Add(agent);
                            break;
                        }
                    }
                }
            }
        }



        return agents;
    }
}