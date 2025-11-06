using Base;
using Spawners;
using UnityEngine;
using System.Collections.Generic;
using AgentControllers;
using AgentOrganizations;
using UnityEngine.Tilemaps;

namespace Problems.MicroRTS
{
    public class MicroRTS1vs1MatchSpawner : MatchSpawner
    {
        [Header("MicroRTS 1vs1 Match Spawn Points")]
        private Transform[] resourceSpawnPoints;
        private Transform[] baseSpawnPoints;
        private Transform[] workerSpawnPoints;
        private Transform dummyAgentSpawnPoint;
        private UnitSpawnPositions spawnPositions;

        private void FindSpawnPoints(EnvironmentControllerBase environmentController)
        {
            // Find UnitSpawnPositions component in the environment controller or its children
            spawnPositions = environmentController.GetComponentInChildren<UnitSpawnPositions>();
            if (spawnPositions == null)
            {
                throw new System.Exception("UnitSpawnPositions component not found in environment controller");
            }

            // Get spawn points from the component
            resourceSpawnPoints = new Transform[]
            {
                spawnPositions.Resource0_Spawn,
                spawnPositions.Resource1_Spawn
            };

            baseSpawnPoints = new Transform[]
            {
                spawnPositions.Player0_BaseSpawn,
                spawnPositions.Player1_BaseSpawn
            };

            workerSpawnPoints = new Transform[]
            {
                spawnPositions.Player0_WorkerSpawn,
                spawnPositions.Player1_WorkerSpawn
            };

            dummyAgentSpawnPoint = spawnPositions.DummyAgentSpawn;
            if (dummyAgentSpawnPoint == null)
            {
                // Create a default off-board spawn point if not found
                GameObject dummySpawn = new GameObject("DummyAgentSpawn");
                dummySpawn.transform.SetParent(spawnPositions.transform);
                dummySpawn.transform.position = Vector3.zero;
                dummyAgentSpawnPoint = dummySpawn.transform;
            }
        }

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            if (environmentController == null)
            {
                throw new System.Exception("EnvironmentController is not defined");
            }

            if (!(environmentController is MicroRTSEnvironmentController))
            {
                throw new System.Exception("EnvironmentController is not of type MicroRTSEnvironmentController");
            }

            if (environmentController.Match == null || environmentController.Match.Teams == null)
            {
                throw new System.Exception("Match is not defined");
            }

            if (environmentController.Match.Teams.Length != 1)
            {
                throw new System.Exception("Match should have 1 team");
            }

            if (environmentController.AgentPrefab == null)
            {
                throw new System.Exception("AgentPrefab is not defined");
            }

            // Find spawn points automatically
            FindSpawnPoints(environmentController);

            // Validate spawn points were found
            if (resourceSpawnPoints == null || resourceSpawnPoints[0] == null || resourceSpawnPoints[1] == null)
            {
                throw new System.Exception("Resource spawn points not found (expected Resource0_Spawn and Resource1_Spawn)");
            }

            if (baseSpawnPoints == null || baseSpawnPoints[0] == null || baseSpawnPoints[1] == null)
            {
                throw new System.Exception("Base spawn points not found (expected Player0_BaseSpawn and Player1_BaseSpawn)");
            }

            if (workerSpawnPoints == null || workerSpawnPoints[0] == null || workerSpawnPoints[1] == null)
            {
                throw new System.Exception("Worker spawn points not found (expected Player0_WorkerSpawn and Player1_WorkerSpawn)");
            }

            // Validate prefabs are assigned
            if (spawnPositions.ResourcePrefab == null)
            {
                throw new System.Exception("ResourcePrefab is not assigned in UnitSpawnPositions component");
            }

            if (spawnPositions.BasePrefab == null)
            {
                throw new System.Exception("BasePrefab is not assigned in UnitSpawnPositions component");
            }

            if (spawnPositions.WorkerPrefab == null)
            {
                throw new System.Exception("WorkerPrefab is not assigned in UnitSpawnPositions component");
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> agents = new List<T>();

            // Spawn Resources
            SpawnResources();

            // Spawn Bases
            SpawnBases();

            // Spawn Workers
            SpawnWorkers();

            // Spawn 1 dummy agent for Player 0 that controls all Player 0 units
            var team0 = environmentController.Match.Teams[0];
            if (team0.Individuals != null && team0.Individuals.Length > 0)
            {
                var individual = team0.Individuals[0];
                if (individual.AgentControllers != null && individual.AgentControllers.Length > 0)
                {
                    var agentController = individual.AgentControllers[0];

                    // Position dummy agent off-board
                    Vector3 dummyAgentPosition = dummyAgentSpawnPoint != null
                        ? dummyAgentSpawnPoint.position
                        : Vector3.zero;
                    Quaternion dummyAgentRotation = dummyAgentSpawnPoint != null
                        ? dummyAgentSpawnPoint.rotation
                        : Quaternion.identity;

                    // Instantiate
                    GameObject dummyAgentObj = Instantiate(
                        environmentController.AgentPrefab,
                        dummyAgentPosition,
                        dummyAgentRotation,
                        gameObject.transform
                    );

                    // Configure
                    T agent = dummyAgentObj.GetComponent<T>();
                    MicroRTSAgentComponent agentComponent = agent as MicroRTSAgentComponent;
                    if (agentComponent != null)
                    {
                        agentComponent.AgentController = agentController.Clone();
                        agentComponent.IndividualID = individual.IndividualId;
                        if (agentComponent.TeamIdentifier != null)
                        {
                            agentComponent.TeamIdentifier.TeamID = team0.TeamId;
                        }
                    }

                    agents.Add(agent);
                }
            }

            return agents.ToArray();
        }

        private void SpawnResources()
        {
            GameObject resourcePrefab = spawnPositions.ResourcePrefab;

            for (int i = 0; i < resourceSpawnPoints.Length; i++)
            {
                Vector3 centeredPosition = CenterPositionOnTile(resourceSpawnPoints[i].position);
                GameObject resourceObj = Instantiate(resourcePrefab, centeredPosition, resourceSpawnPoints[i].rotation, gameObject.transform);
                resourceObj.name = $"Resource_{i}";
            }
        }

        private void SpawnBases()
        {
            GameObject basePrefab = spawnPositions.BasePrefab;
            Color[] playerColors = new Color[] { Color.blue, Color.red };

            for (int i = 0; i < baseSpawnPoints.Length; i++)
            {
                Vector3 centeredPosition = CenterPositionOnTile(baseSpawnPoints[i].position);
                GameObject baseObj = Instantiate(basePrefab, centeredPosition, baseSpawnPoints[i].rotation, gameObject.transform);
                baseObj.name = $"Base_Player{i}";
                SetColor(baseObj, playerColors[i]);
            }
        }

        private void SpawnWorkers()
        {
            GameObject workerPrefab = spawnPositions.WorkerPrefab;
            Color[] playerColors = new Color[] { Color.blue, Color.red };

            for (int i = 0; i < workerSpawnPoints.Length; i++)
            {
                Vector3 centeredPosition = CenterPositionOnTile(workerSpawnPoints[i].position);
                GameObject workerObj = Instantiate(workerPrefab, centeredPosition, workerSpawnPoints[i].rotation, gameObject.transform);
                workerObj.name = $"Worker_Player{i}";
                SetColor(workerObj, playerColors[i]);
            }
        }

        private Vector3 CenterPositionOnTile(Vector3 position)
        {
            Tilemap tilemap = FindFirstObjectByType<Tilemap>();
            Vector3 cellSize = tilemap.cellSize;
            Vector3 cellPosition = new Vector3(
                Mathf.Floor(position.x / cellSize.x) * cellSize.x + cellSize.x * 0.5f,
                Mathf.Floor(position.y / cellSize.y) * cellSize.y + cellSize.y * 0.5f,
                position.z
            );
            return cellPosition;
        }

        private void SetColor(GameObject obj, Color color)
        {
            SpriteRenderer[] spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>();
            if (spriteRenderers.Length > 0)
            {
                foreach (SpriteRenderer sr in spriteRenderers)
                {
                    sr.color = color;
                }
            }
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            // Not required for this problem domain
            throw new System.NotImplementedException();
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T[] respawnComponents)
        {
            // Not required for this problem domain
            throw new System.NotImplementedException();
        }
    }
}