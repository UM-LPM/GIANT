using Base;
using Spawners;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.PlanetConquest2
{
    public class PlanetConquest2PlanetSpawner : Spawner
    {
        [SerializeField] List<Vector3> LavaPlanetSpawnPositions = new List<Vector3> { new Vector3(-10, 0, 0), new Vector3(10, 0, 0), new Vector3(0, -10, 0), new Vector3(0, 10, 0) };
        [SerializeField] List<Vector3> IcePlanetSpawnPositions = new List<Vector3> { new Vector3(-5, 5, 0), new Vector3(5, 5, 0), new Vector3(-5, -5, 0), new Vector3(5, -5, 0) };

        Quaternion rotation;
        GameObject obj;

        PlanetConquest2EnvironmentController planetConquestEnvironmentController;

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            planetConquestEnvironmentController = environmentController as PlanetConquest2EnvironmentController;

            if (planetConquestEnvironmentController.LavaPlanetPrefab == null)
            {
                throw new System.Exception("LavaPlanetPrefab is not defined");
                // TODO Add error reporting here
            }
            if (planetConquestEnvironmentController.IcePlanetPrefab == null)
            {
                throw new System.Exception("IcePlanetPrefab is not defined");
                // TODO Add error reporting here
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> planets = new List<T>();

            PlanetConquest2EnvironmentController planetConquestEnvironmentController = environmentController as PlanetConquest2EnvironmentController;

            // Spawn Lava Planets
            for (int i = 0; i < planetConquestEnvironmentController.LavaPlanetSpawnAmount; i++)
            {
                T planet = SpawnPlanet<T>(planetConquestEnvironmentController, planetConquestEnvironmentController.LavaPlanetPrefab, LavaPlanetSpawnPositions[i]);
                PlanetComponent planetComponent = planet as PlanetComponent;
                planetComponent.PlanetType = PlanetType.Lava;
                planets.Add(planet);
            }
            // Spawn Ice Planets
            for (int i = 0; i < planetConquestEnvironmentController.IcePlanetSpawnAmount; i++)
            {
                T planet = SpawnPlanet<T>(planetConquestEnvironmentController, planetConquestEnvironmentController.IcePlanetPrefab, IcePlanetSpawnPositions[i]);
                PlanetComponent planetComponent = planet as PlanetComponent;
                planetComponent.PlanetType = PlanetType.Ice;
                planets.Add(planet);
            }

            return planets.ToArray();
        }

        public T SpawnPlanet<T>(PlanetConquest2EnvironmentController environmentController, GameObject planetPrefab, Vector3 spawnPos) where T : Component
        {
            // Instantiate planet and set layer
            rotation = planetPrefab.transform.rotation;
            obj = Instantiate(planetPrefab, spawnPos, rotation, gameObject.transform);
            obj.layer = gameObject.layer;

            return obj.GetComponent<T>();
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            throw new System.NotImplementedException();
        }
    }
}
