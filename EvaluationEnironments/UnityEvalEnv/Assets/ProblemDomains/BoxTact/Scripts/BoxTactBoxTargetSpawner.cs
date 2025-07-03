using System.Collections;
using System.Collections.Generic;
using Base;
using Spawners;
using UnityEngine;

namespace Problems.BoxTact
{
    public class BoxTactBoxTargetSpawner : Spawner
    {
        [SerializeField] private Transform[] BoxTargetSpawnPositions;

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            throw new System.NotImplementedException();
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            T[] boxTargets = new T[BoxTargetSpawnPositions.Length];

            for (int i = 0; i < BoxTargetSpawnPositions.Length; i++)
            {
                // Instantiate and configure box target
                GameObject boxTargetGameObject = Instantiate((environmentController as BoxTactEnvironmentController).BoxTargetPrefab, BoxTargetSpawnPositions[i].position, BoxTargetSpawnPositions[i].rotation, gameObject.transform);
                boxTargetGameObject.layer = gameObject.layer;
                // Configure box target
                T boxTarget = boxTargetGameObject.GetComponent<T>();
                boxTargets[i] = boxTarget;
            }
            return boxTargets;
        }

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            if ((environmentController as BoxTactEnvironmentController).BoxTargetPrefab == null)
            {
                throw new System.Exception("BoxTactEnvironmentController BoxTargetPrefab is not defined");
            }

            if (BoxTargetSpawnPositions == null || BoxTargetSpawnPositions.Length == 0)
            {
                throw new System.Exception("BoxTactEnvironmentController BoxTargetSpawnPositions are not defined");
            }
        }
    }
}