using UnityEngine;
using AgentOrganizations;
using System.Collections.Generic;
using Problems.Dummy;

/// <summary>
/// Class that randomly spawns the individuals in the environment for the Dummy problem domain
/// </summary>
public class DummyRandomIndividualSpawner : IndividualSpawner
{
    public override List<AgentComponent> SpawnIndividuals(EnvironmentControllerBase environmentController)
    {
        if(!(environmentController is DummyEnvironmentController))
        {
            throw new System.Exception("EnvironmentController is not of type DummyEnvironmentController");
            // TODO Add error reporting here
        }
        if(environmentController.Match == null)
        {
            throw new System.Exception("Match is not defined");
            // TODO Add error reporting here
        }
        if(environmentController.AgentPrefab == null)
        {
            throw new System.Exception("AgentPrefab is not defined");
            // TODO Add error reporting here
        }

        List<AgentComponent> agents = new List<AgentComponent>();
        Debug.Log("Spawning individuals...");

        // Randomly spawn all agents in the environment
        List<Vector3> usedSpawnPoints = new List<Vector3>();
        Vector3 spawnPos;
        Quaternion rotation;

        foreach (var team in environmentController.Match.Teams)
        {
            foreach (var individual in team.Individuals)
            {
                foreach (var agentController in individual.AgentControllers)
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
                        AgentComponent agent = obj.GetComponent<AgentComponent>();
                        agent.AgentController = agentController;
                        agent.IndividualID = individual.IndividualId;
                        agent.TeamID = team.TeamId;
                        usedSpawnPoints.Add(spawnPos);

                        break;
                    }
                }
            }
        }



        return agents;
    }
}