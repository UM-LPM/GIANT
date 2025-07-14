using System.Collections.Generic;
using UnityEngine;
using Spawners;
using Base;

namespace Problems.Soccer
{
    [DisallowMultipleComponent]
    public class SoccerBallSpawner : Spawner
    {
        [Header("Robostrike 1vs1 Match Configuration")]
        [SerializeField] public Transform SoccerBallSpawnPoint;

        Vector3 spawnPos;
        Quaternion rotation;

        private GameObject obj;
        private Rigidbody rigidbody; 

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            SoccerEnvironmentController soccerEnvironmentController = environmentController as SoccerEnvironmentController;

            if (soccerEnvironmentController.SoccerBallPrefab == null)
            {
                throw new System.Exception("SoccerBallPrefab is not defined");
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            SoccerEnvironmentController soccerEnvironmentController = environmentController as SoccerEnvironmentController;

            spawnPos = SoccerBallSpawnPoint.position;
            rotation = SoccerBallSpawnPoint.rotation;

            obj = Instantiate(soccerEnvironmentController.SoccerBallPrefab, spawnPos, rotation, gameObject.transform);

            obj.GetComponent<SoccerBallComponent>().NumOfSpawns++;

            obj.layer = gameObject.layer;

            return new T[] { obj.GetComponent<T>() };
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            spawnPos = SoccerBallSpawnPoint.position;
            rotation = SoccerBallSpawnPoint.rotation;

            respawnComponent.transform.position = spawnPos;
            respawnComponent.transform.rotation = rotation;

            rigidbody = respawnComponent.GetComponent<Rigidbody>();
            if(rigidbody != null)
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }

            respawnComponent.GetComponent<SoccerBallComponent>().NumOfSpawns++;
        }

    }
}