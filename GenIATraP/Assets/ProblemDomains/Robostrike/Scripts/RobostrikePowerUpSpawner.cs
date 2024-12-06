using System.Collections.Generic;
using UnityEngine;
using Spawners;
using Problems.Dummy;
using Unity.VisualScripting;
using Base;

namespace Problems.Robostrike
{
    [DisallowMultipleComponent]
    public class RobostrikePowerUpSpawner : Spawner
    {
        Vector3 spawnPos;
        Quaternion rotation;
        bool isFarEnough;

        int counter = 0;
        int maxSpawnPoints = 100;

        public int HealthBoxSpawned { get; set; }
        public int AmmoBoxSpawned { get; set; }
        public int ShieldBoxSpawned { get; set; }

        private GameObject obj;
        PowerUpComponent powerUpComponent;

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            RobostrikeEnvironmentController robostrikeEnvironmentController = environmentController as RobostrikeEnvironmentController;

            if (robostrikeEnvironmentController.HealthBoxPrefab == null)
            {
                throw new System.Exception("HealthBoxPrefab is not defined");
                // TODO Add error reporting here
            }

            if (robostrikeEnvironmentController.AmmoBoxPrefab == null)
            {
                throw new System.Exception("AmmoBoxPrefab is not defined");
                // TODO Add error reporting here
            }

            if (robostrikeEnvironmentController.ShieldBoxPrefab == null)
            {
                throw new System.Exception("ShieldBoxPrefab is not defined");
                // TODO Add error reporting here
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> powerUps = new List<T>();

            RobostrikeEnvironmentController robostrikeEnvironmentController = environmentController as RobostrikeEnvironmentController;

            powerUps.AddRange(SpawnPowerUpGroup<T>(robostrikeEnvironmentController, robostrikeEnvironmentController.HealthBoxPrefab, robostrikeEnvironmentController.HealthBoxSpawnAmount));
            powerUps.AddRange(SpawnPowerUpGroup<T>(robostrikeEnvironmentController, robostrikeEnvironmentController.AmmoBoxPrefab, robostrikeEnvironmentController.AmmoBoxSpawnAmount));
            powerUps.AddRange(SpawnPowerUpGroup<T>(robostrikeEnvironmentController, robostrikeEnvironmentController.ShieldBoxPrefab, robostrikeEnvironmentController.ShieldBoxSpawnAmount));

            return powerUps.ToArray();
        }

        public List<T> SpawnPowerUpGroup<T>(RobostrikeEnvironmentController environmentController, GameObject powerUpPrefab, int powerUpSpawnAmount) where T : Component
        {
            List<T> powerUpsGroup = new List<T>();
            List<Vector3> powerUpSpawnPositions = new List<Vector3>();

            for (int i = 0; i < powerUpSpawnAmount; i++)
            {
                powerUpsGroup.Add(SpawnPowerUp<T>(environmentController, powerUpPrefab, powerUpSpawnPositions));
                powerUpSpawnPositions.Add((powerUpsGroup[i]).transform.position);
            }

            return powerUpsGroup;
        }

        public T SpawnPowerUp<T>(RobostrikeEnvironmentController environmentController, GameObject powerUpPrefab, List<Vector3> existingPowerUpPOsitions) where T :  Component
        {
            counter = 0;
            do
            {
                isFarEnough = true;
                spawnPos = GetRandomSpawnPoint(
                                environmentController.Util,
                                environmentController.GameType,
                                environmentController.ArenaSize,
                                environmentController.ArenaRadius,
                                environmentController.ArenaCenterPoint,
                                environmentController.ArenaOffset);
                if (environmentController.SceneLoadMode == SceneLoadMode.GridMode)
                    spawnPos += environmentController.GridCell.GridCellPosition;

                rotation = powerUpPrefab.transform.rotation;

                if (!SpawnPointSuitable(environmentController.GameType,
                            spawnPos,
                            rotation,
                            existingPowerUpPOsitions,
                            environmentController.PowerUpColliderExtendsMultiplier,
                            environmentController.MinPowerUpDistance,
                            true,
                            environmentController.gameObject.layer,
                            environmentController.DefaultLayer))
                {
                    isFarEnough = false;
                }

                // Check if current spawn point is far enough from the agents
                foreach (AgentComponent agent in environmentController.Agents)
                {
                    if (Vector3.Distance(agent.transform.position, spawnPos) < environmentController.MinPowerUpDistanceFromAgents)
                    {
                        isFarEnough = false;
                        break;
                    }
                }

                if (counter >= maxSpawnPoints)
                    break;

                counter++;
            } while (!isFarEnough);

            // Instantiate powerup and set layer
            obj = Instantiate(powerUpPrefab, spawnPos, rotation, gameObject.transform);
            obj.layer = gameObject.layer;

            // Increase the count of the powerup type by getting the correct component
            powerUpComponent = obj.GetComponent<PowerUpComponent>();
            if(powerUpComponent != null)
            {
                switch (powerUpComponent.PowerUpType)
                {
                    case PowerUpType.Health:
                        HealthBoxSpawned++;
                        break;
                    case PowerUpType.Ammo:
                        AmmoBoxSpawned++;
                        break;
                    case PowerUpType.Shield:
                        ShieldBoxSpawned++;
                        break;
                }
            }

            return obj.GetComponent<T>();
        }
    }
}