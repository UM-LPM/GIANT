using System.Collections.Generic;
using UnityEngine;
using Spawners;
using Base;

namespace Problems.Collector
{
    [DisallowMultipleComponent]
    public class CollectorTargetSpawner : Spawner
    {
        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            CollectorEnvironmentController collectorEnvironmentController = environmentController as CollectorEnvironmentController;

            if (collectorEnvironmentController.TargetPrefab == null)
            {
                throw new System.Exception("TargetPrefab is not defined");
                // TODO Add error reporting here
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> targets = new List<T>();
            List<Vector3> targetSpawnPositions = new List<Vector3>();

            CollectorEnvironmentController collectorEnvironmentController = environmentController as CollectorEnvironmentController;

            for (int i = 0; i < collectorEnvironmentController.StartNumberOfTargets; i++)
            {
                targets.Add(SpawnTarget<T>(collectorEnvironmentController, targetSpawnPositions));
                targetSpawnPositions.Add(targets[i].transform.position);
            }

            return targets.ToArray();
        }

        public T SpawnTarget<T>(EnvironmentControllerBase environmentController, List<Vector3> targetSpawnPositions) where T : Component
        {
            validateSpawnConditions(environmentController);

            Vector3 spawnPos;
            Quaternion rotation;
            bool isFarEnough;
            CollectorEnvironmentController collectorEnvironmentController = environmentController as CollectorEnvironmentController;

            do
            {
                isFarEnough = true;
                spawnPos = GetRandomSpawnPoint(
                                collectorEnvironmentController.Util,
                                collectorEnvironmentController.GameType,
                                collectorEnvironmentController.ArenaSize,
                                collectorEnvironmentController.ArenaRadius,
                                collectorEnvironmentController.ArenaCenterPoint,
                                collectorEnvironmentController.ArenaOffset);
                if (collectorEnvironmentController.SceneLoadMode == SceneLoadMode.GridMode)
                    spawnPos += collectorEnvironmentController.GridCell.GridCellPosition;

                rotation = GetRandomRotation(collectorEnvironmentController.Util, collectorEnvironmentController.GameType);

                if (!SpawnPointSuitable(collectorEnvironmentController.GameType,
                            spawnPos,
                            rotation,
                            targetSpawnPositions,
                            collectorEnvironmentController.TargetColliderExtendsMultiplier,
                            collectorEnvironmentController.TargetToTargetDistance,
                            true,
                            collectorEnvironmentController.gameObject.layer,
                            collectorEnvironmentController.DefaultLayer))
                {
                    isFarEnough = false;
                }

                // Check if current spawn point is far enough from the agents
                foreach (AgentComponent agent in collectorEnvironmentController.Agents)
                {
                    if (Vector3.Distance(agent.transform.position, spawnPos) < collectorEnvironmentController.TargetMinDistanceFromAgents)
                    {
                        isFarEnough = false;
                        break;
                    }
                }

            } while (!isFarEnough);

            // Instantiate target and set layer
            GameObject obj = Instantiate(collectorEnvironmentController.TargetPrefab, spawnPos, rotation, gameObject.transform);
            obj.layer = gameObject.layer;

            // Upate lists
            targetSpawnPositions.Add(obj.transform.position);

            return obj.GetComponent<T>();
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            throw new System.NotImplementedException();
        }
    }
}