using System.Collections.Generic;
using UnityEngine;
using Spawners;

namespace Problems.Robostrike
{
    [DisallowMultipleComponent]
    public class RobostrikePowerUpSpawner : Spawner
    {
        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            throw new System.NotImplementedException();
        }

        public override List<T> Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> powerUps = new List<T>();
            RobostrikeEnvironmentController robostrikeEnvironmentController = environmentController as RobostrikeEnvironmentController;

            for (int i = 0; i < robostrikeEnvironmentController.HealthBoxSpawnAmount; i++)
            {
                powerUps.Add(SpawnPowerUp<T>(robostrikeEnvironmentController, robostrikeEnvironmentController.HealthBoxPrefab));
            }

            for (int i = 0; i < robostrikeEnvironmentController.ShieldBoxSpawnAmount; i++)
            {
                powerUps.Add(SpawnPowerUp<T>(robostrikeEnvironmentController, robostrikeEnvironmentController.ShieldBoxPrefab));
            }

            for (int i = 0; i < robostrikeEnvironmentController.AmmoBoxSpawnAmount; i++)
            {
                powerUps.Add(SpawnPowerUp<T>(robostrikeEnvironmentController, robostrikeEnvironmentController.AmmoBoxPrefab));
            }

            return powerUps;
        }

        public T SpawnPowerUp<T>(RobostrikeEnvironmentController environmentController, GameObject powerUpPrefab) where T :  Component
        {
            throw new System.NotImplementedException();
        }
    }
}