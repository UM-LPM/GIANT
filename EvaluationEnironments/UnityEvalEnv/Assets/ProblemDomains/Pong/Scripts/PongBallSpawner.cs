using System.Collections.Generic;
using UnityEngine;
using Spawners;
using Base;

namespace Problems.Pong
{
    [DisallowMultipleComponent]
    public class PongBallSpawner : Spawner
    {
        [Header("Robostrike 1vs1 Match Configuration")]
        [SerializeField] public Transform PongBallSpawnPoint;

        Vector3 spawnPos;
        Quaternion rotation;

        private GameObject obj;

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            PongEnvironmentController pongEnvironmentController = environmentController as PongEnvironmentController;

            if (pongEnvironmentController.PongBallPrefab == null)
            {
                throw new System.Exception("PongBallPrefab is not defined");
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            PongEnvironmentController soccerEnvironmentController = environmentController as PongEnvironmentController;

            spawnPos = PongBallSpawnPoint.position;
            rotation = PongBallSpawnPoint.rotation;

            obj = Instantiate(soccerEnvironmentController.PongBallPrefab, spawnPos, rotation, gameObject.transform);

            obj.GetComponent<PongBallComponent>().NumOfSpawns++;

            obj.layer = gameObject.layer;

            return new T[] { obj.GetComponent<T>() };
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            respawnComponent.GetComponent<PongBallComponent>().Respawn(PongBallSpawnPoint.position, PongBallSpawnPoint.rotation);
        }

    }
}