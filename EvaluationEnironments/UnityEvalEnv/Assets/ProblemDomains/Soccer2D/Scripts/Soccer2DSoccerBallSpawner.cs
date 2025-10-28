using System.Collections.Generic;
using UnityEngine;
using Spawners;
using Base;

namespace Problems.Soccer2D
{
    [DisallowMultipleComponent]
    public class Soccer2DBallSpawner : Spawner
    {
        [Header("Robostrike 1vs1 Match Configuration")]
        [SerializeField] public Transform SoccerBallSpawnPoint;

        Vector3 spawnPos;
        Quaternion rotation;

        private GameObject obj;

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            Soccer2DEnvironmentController soccerEnvironmentController = environmentController as Soccer2DEnvironmentController;

            if (soccerEnvironmentController.SoccerBallPrefab == null)
            {
                throw new System.Exception("SoccerBallPrefab is not defined");
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            Soccer2DEnvironmentController soccerEnvironmentController = environmentController as Soccer2DEnvironmentController;

            spawnPos = SoccerBallSpawnPoint.position;
            rotation = SoccerBallSpawnPoint.rotation;

            obj = Instantiate(soccerEnvironmentController.SoccerBallPrefab, spawnPos, rotation, gameObject.transform);

            obj.GetComponent<Soccer2DSoccerBallComponent>().NumOfSpawns++;

            obj.layer = gameObject.layer;

            return new T[] { obj.GetComponent<T>() };
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            respawnComponent.GetComponent<Soccer2DSoccerBallComponent>().Respawn(SoccerBallSpawnPoint.position, SoccerBallSpawnPoint.rotation);
        }

    }
}