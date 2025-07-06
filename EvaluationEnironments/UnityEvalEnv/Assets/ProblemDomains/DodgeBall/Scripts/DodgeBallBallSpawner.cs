using Base;
using Spawners;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.DodgeBall
{
    public class DodgeBallBallSpawner : Spawner
    {
        [SerializeField] private Transform[] BallSpawnPositions;
        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            throw new System.NotImplementedException();
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            T[] boxes = new T[BallSpawnPositions.Length];
            for (int i = 0; i < BallSpawnPositions.Length; i++)
            {
                // Instantiate and configure box
                GameObject ballGameObject = Instantiate((environmentController as DodgeBallEnvironmentController).BallPrefab, BallSpawnPositions[i].position, BallSpawnPositions[i].rotation, gameObject.transform);
                ballGameObject.layer = gameObject.layer;

                // Configure box
                T box = ballGameObject.GetComponent<T>();
                boxes[i] = box;
            }

            return boxes;
        }

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            if ((environmentController as DodgeBallEnvironmentController).BallPrefab == null)
            {
                throw new System.Exception("DodgeBallEnvironmentController is not defined");
            }

            if (BallSpawnPositions == null || BallSpawnPositions.Length == 0)
            {
                throw new System.Exception("DodgeBallEnvironmentController BallSpawnPositions are not defined");
            }
        }
    }
}
