using System.Collections.Generic;
using UnityEngine;
using Spawners;
using Base;

namespace Problems.Moba_game
{
    [DisallowMultipleComponent]
    public class Moba_gamePlanetSpawner : Spawner
    {
        Vector3 spawnPos;
        Quaternion rotation;
        bool isFarEnough;

        int counter = 0;
        int maxSpawnPoints = 100;

        private GameObject obj;

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            Moba_gameEnvironmentController Moba_gameEnvironmentController = environmentController as Moba_gameEnvironmentController;

            if (Moba_gameEnvironmentController.LavaPlanetPrefab == null)
            {
                throw new System.Exception("LavaPlanetPrefab is not defined");
                // TODO Add error reporting here
            }
            if (Moba_gameEnvironmentController.IcePlanetPrefab == null)
            {
                throw new System.Exception("IcePlanetPrefab is not defined");
                // TODO Add error reporting here
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> planets = new List<T>();
            List<Vector3> planetSpawnPositions = new List<Vector3>();
            Moba_gameEnvironmentController Moba_gameEnvironmentController = environmentController as Moba_gameEnvironmentController;

            // Spawn Lava Planets
            for (int i = 0; i < Moba_gameEnvironmentController.LavaPlanetSpawnAmount; i++)
            {
                T planet = SpawnPlanet<T>(Moba_gameEnvironmentController, Moba_gameEnvironmentController.LavaPlanetPrefab, planetSpawnPositions);
                Moba_gamePlanetComponent planetComponent = planet as Moba_gamePlanetComponent;
                planetComponent.Type = "lava";
                planets.Add(planet);
                planetSpawnPositions.Add(planets[i].transform.position);
            }
            // Spawn Ice Planets
            for (int i = 0; i < Moba_gameEnvironmentController.IcePlanetSpawnAmount; i++)
            {
                T planet = SpawnPlanet<T>(Moba_gameEnvironmentController, Moba_gameEnvironmentController.IcePlanetPrefab, planetSpawnPositions);
                Moba_gamePlanetComponent planetComponent = planet as Moba_gamePlanetComponent;
                planetComponent.Type = "ice";
                planets.Add(planet);
                planetSpawnPositions.Add(planets[i].transform.position);
            }

            return planets.ToArray();
        }

        public T SpawnPlanet<T>(Moba_gameEnvironmentController environmentController, GameObject planetPrefab, List<Vector3> existingPlanetPositions) where T : Component
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

                rotation = planetPrefab.transform.rotation;

                if (!SpawnPointSuitable(environmentController.GameType,
                            spawnPos,
                            rotation,
                            existingPlanetPositions,
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

            // Instantiate planet and set layer
            obj = Instantiate(planetPrefab, spawnPos, rotation, gameObject.transform);
            return obj.GetComponent<T>();
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            throw new System.NotImplementedException();
        }
    }
}