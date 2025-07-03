using System.Collections;
using System.Collections.Generic;
using Base;
using Spawners;
using UnityEngine;

namespace Problems.BoxTact
{
    public class BoxTactBoxSpawner : Spawner
    {
        [SerializeField] private Transform[] BoxSpawnPositions;
        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            throw new System.NotImplementedException();
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            T[] boxes = new T[BoxSpawnPositions.Length];
            for (int i = 0; i < BoxSpawnPositions.Length; i++)
            {
                // Instantiate and configure box
                GameObject boxGameObject = Instantiate((environmentController as BoxTactEnvironmentController).BoxPrefab, BoxSpawnPositions[i].position, BoxSpawnPositions[i].rotation, gameObject.transform);
                boxGameObject.layer = gameObject.layer;

                // Configure box
                T box = boxGameObject.GetComponent<T>();
                boxes[i] = box;
            }

            return boxes;
        }

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            if((environmentController as BoxTactEnvironmentController).BoxPrefab == null)
            {
                throw new System.Exception("BoxTactEnvironmentController is not defined");
            }

            if (BoxSpawnPositions == null || BoxSpawnPositions.Length == 0)
            {
                throw new System.Exception("BoxTactEnvironmentController BoxSpawnPositions are not defined");
            }
        }
    }
}