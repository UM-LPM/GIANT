using System.Collections.Generic;
using UnityEngine;
using Spawners;
using Base;

namespace Problems.PlanetConquest
{
    [DisallowMultipleComponent]
    public class PlanetSpawner : Spawner
    {
        [SerializeField] List<Vector3> LavaPlanetSpawnPositions = new List<Vector3> { new Vector3(-15, 15, 0), new Vector3(15, 15, 0), new Vector3(-15, -15, 0), new Vector3(15, -15, 0) };
        [SerializeField] List<Vector3> IcePlanetSpawnPositions = new List<Vector3> { new Vector3(-5, 5, 0), new Vector3(5, 5, 0), new Vector3(-5, -5, 0), new Vector3(5, -5, 0) };

        Quaternion rotation;
        GameObject obj;

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            PlanetConquestEnvironmentController planetConquestEnvironmentController = environmentController as PlanetConquestEnvironmentController;

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
            
            PlanetConquestEnvironmentController planetConquestEnvironmentController = environmentController as PlanetConquestEnvironmentController;

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

        public T SpawnPlanet<T>(PlanetConquestEnvironmentController environmentController, GameObject planetPrefab, Vector3 spawnPos/*List<Vector3> existingPlanetPositions*/) where T : Component
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