using System.Collections.Generic;
using UnityEngine;
using Spawners;

namespace Problems.Dummy
{
    [DisallowMultipleComponent]
    public class DummyTargetSpawner : Spawner
    {
        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            DummyEnvironmentController dummyEnvironmentController = environmentController as DummyEnvironmentController;

            if (dummyEnvironmentController.TargetPrefab == null)
            {
                throw new System.Exception("TargetPrefab is not defined");
                // TODO Add error reporting here
            }
        }

        public override List<T> Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> targets = new List<T>();
            List<Vector3> targetSpawnPositions = new List<Vector3>();

            DummyEnvironmentController dummyEnvironmentController = environmentController as DummyEnvironmentController;

            for (int i = 0; i < dummyEnvironmentController.StartNumberOfTargets; i++)
            {
                targets.Add(SpawnTarget<T>(dummyEnvironmentController, targetSpawnPositions));
            }

            return targets;
        }

        public T SpawnTarget<T>(EnvironmentControllerBase environmentController, List<Vector3> targetSpawnPosition) where T : Component
        {
            validateSpawnConditions(environmentController);

            Vector3 spawnPos;
            Quaternion rotation;
            bool isFarEnough;
            DummyEnvironmentController dummyEnvironmentController = environmentController as DummyEnvironmentController;

            do
            {
                isFarEnough = true;
                spawnPos = GetRandomSpawnPoint(
                                dummyEnvironmentController.Util,
                                dummyEnvironmentController.GameType,
                                dummyEnvironmentController.ArenaSize,
                                dummyEnvironmentController.ArenaRadius,
                                dummyEnvironmentController.ArenaCenterPoint,
                                dummyEnvironmentController.ArenaOffset);
                if (dummyEnvironmentController.SceneLoadMode == SceneLoadMode.GridMode)
                    spawnPos += dummyEnvironmentController.GridCell.GridCellPosition;

                rotation = GetRandomRotation(dummyEnvironmentController.Util, dummyEnvironmentController.GameType);

                if (!SpawnPointSuitable(dummyEnvironmentController.GameType,
                            spawnPos,
                            rotation,
                            targetSpawnPosition,
                            dummyEnvironmentController.TargetColliderExtendsMultiplier,
                            dummyEnvironmentController.TargetToTargetDistance,
                            true,
                            dummyEnvironmentController.gameObject.layer,
                            dummyEnvironmentController.DefaultLayer))
                {
                    isFarEnough = false;
                }

                // Check if current spawn point is far enough from the agents
                foreach (AgentComponent agent in dummyEnvironmentController.Agents)
                {
                    if (Vector3.Distance(agent.transform.position, spawnPos) < dummyEnvironmentController.TargetMinDistanceFromAgents)
                    {
                        isFarEnough = false;
                        break;
                    }
                }

            } while (!isFarEnough);

            // Instantiate target and set layer
            GameObject obj = Instantiate(dummyEnvironmentController.TargetPrefab, spawnPos, rotation, gameObject.transform);
            obj.layer = gameObject.layer;

            // Upate lists
            targetSpawnPosition.Add(obj.transform.position);

            return obj.GetComponent<T>();
        }
    }
}