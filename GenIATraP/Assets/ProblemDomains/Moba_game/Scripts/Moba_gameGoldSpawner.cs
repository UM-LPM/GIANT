using System.Collections.Generic;
using UnityEngine;
using Spawners;
using Base;

namespace Problems.Moba_game
{
    [DisallowMultipleComponent]
    public class Moba_gameGoldSpawner : Spawner
    {
        Vector3 spawnPos;
        Quaternion rotation;
        bool isFarEnough;

        int counter = 0;
        int maxSpawnPoints = 100;

        public int GoldBoxSpawned { get; set; }

        private GameObject obj;
        GoldComponent goldComponent;

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            Moba_gameEnvironmentController Moba_gameEnvironmentController = environmentController as Moba_gameEnvironmentController;

            if (Moba_gameEnvironmentController.GoldPrefab == null)
            {
                throw new System.Exception("GoldPrefab is not defined");
                // TODO Add error reporting here
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> golds = new List<T>();
            List<Vector3> goldSpawnPositions = new List<Vector3>();
            Moba_gameEnvironmentController Moba_gameEnvironmentController = environmentController as Moba_gameEnvironmentController;
            
            // Spawn golds
            for (int i = 0; i < Moba_gameEnvironmentController.GoldSpawnAmount; i++)
            {
                golds.Add(SpawnGold<T>(Moba_gameEnvironmentController, Moba_gameEnvironmentController.GoldPrefab, goldSpawnPositions));
                goldSpawnPositions.Add(golds[i].transform.position);
            }
            
            return golds.ToArray();

            // List<T> golds = new List<T>();

            // Moba_gameEnvironmentController Moba_gameEnvironmentController = environmentController as Moba_gameEnvironmentController;

            // golds.AddRange(SpawnGoldGroup<T>(Moba_gameEnvironmentController, Moba_gameEnvironmentController.GoldPrefab, Moba_gameEnvironmentController.GoldSpawnAmount));
        
            // return golds.ToArray();
        }

        public T SpawnGold<T>(Moba_gameEnvironmentController environmentController, GameObject goldPrefab, List<Vector3> existingGoldPositions) where T :  Component
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

                rotation = goldPrefab.transform.rotation;

                if (!SpawnPointSuitable(environmentController.GameType,
                            spawnPos,
                            rotation,
                            existingGoldPositions,
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
            obj = Instantiate(goldPrefab, spawnPos, rotation, gameObject.transform);
            //obj.layer = gameObject.layer;

            // Increase the count of the powerup type by getting the correct component
            // goldComponent = obj.GetComponent<GoldComponent>();
            // if(goldComponent != null)
            // {
            //     GoldBoxSpawned++;
            // }
            return obj.GetComponent<T>();
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            throw new System.NotImplementedException();
        }
    }
}