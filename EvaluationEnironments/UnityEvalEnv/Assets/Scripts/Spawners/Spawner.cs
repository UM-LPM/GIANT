using UnityEngine;
using System.Collections.Generic;
using Utils;
using Base;

namespace Spawners
{
    public abstract class Spawner : MonoBehaviour
    {
        /// <summary>
        /// Checks if all conditions for spawning are meet.
        /// </summary>
        /// <returns>True if all conditions for spawning agents are meet and False if the aren't.</returns>
        public abstract void validateSpawnConditions(EnvironmentControllerBase environmentController);

        /// <summary>
        /// Spawns gameobjects in the environment.
        /// </summary>
        /// <param name="environmentController"></param>
        /// <returns>Returns list of AgentComponents from spawned agents.</returns>
        public abstract T[] Spawn<T>(EnvironmentControllerBase environmentController) where T: Component;

        public abstract void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent) where T : Component;

        public static Vector3 GetRandomSpawnPoint(Util util, GameType gameType, Vector3 arenaSize, float arenaRadius, Vector3 arenaCenterPoint, float arenaOffset)
        {
            if (arenaSize != Vector3.zero)
            {
                if (gameType == GameType._3D)
                {
                    return new Vector3
                    {
                        x = util.NextFloat((-(arenaSize.x / 2)) + arenaOffset, (arenaSize.x / 2) - arenaOffset),
                        y = arenaSize.y,
                        z = util.NextFloat((-(arenaSize.z / 2)) + arenaOffset, (arenaSize.z / 2) - arenaOffset),
                    };
                }
                else
                {
                    return new Vector3
                    {
                        x = util.NextFloat((-(arenaSize.x / 2)) + arenaOffset, (arenaSize.x / 2) - arenaOffset),
                        y = util.NextFloat((-(arenaSize.y / 2)) + arenaOffset, (arenaSize.y / 2) - arenaOffset),
                        z = arenaSize.z,
                    };
                }
            }
            else
            {
                return GetRandomSpawnPointInRadius(util, arenaRadius, arenaCenterPoint, arenaOffset);
            }
        }

        public static Vector3 GetRandomSpawnPointInRadius(Util util, float radius, Vector3 arenaCenterPoint, float offset)
        {
            // Generate a random angle in radians
            float angle = util.NextFloat(0f, Mathf.PI * 2f);

            // Generate a random distance within the radius
            float distance = util.NextFloat(0f, radius) + offset;

            // Calculate the x and z coordinates based on the angle and distance
            float x = arenaCenterPoint.x + distance * Mathf.Cos(angle);
            float z = arenaCenterPoint.z + distance * Mathf.Sin(angle);

            // Create a Vector3 with the calculated coordinates
            Vector3 randomLocation = new Vector3(x, arenaCenterPoint.y, z);

            return randomLocation;
        }

        public static Quaternion GetRandomRotation(Util util, GameType gameType)
        {
            if (gameType == GameType._3D)
                return Quaternion.AngleAxis(util.NextFloat(0, 360), new Vector3(0, 1, 0));
            else
                return Quaternion.Euler(0, 0, util.NextFloat(0, 360));
        }

        public static bool SpawnPointSuitable(GameType gameType, Vector3 newSpawnPos, Quaternion newRotation, List<Vector3> occupiedSpawnPoints, Vector3 halfExtends, float minObjectDistance, bool ignoreTriggerGameObjs, int layer, int defaultLayer)
        {
            if (PhysicsUtil.PhysicsOverlapObject(gameType, null, newSpawnPos, 0, halfExtends, newRotation, PhysicsOverlapType.OverlapBox, ignoreTriggerGameObjs, layer, defaultLayer))
                return false;

            if (occupiedSpawnPoints != null && occupiedSpawnPoints.Count > 0)
            {
                foreach (var usedSpawnPoint in occupiedSpawnPoints)
                {
                    if (Vector3.Distance(newSpawnPos, usedSpawnPoint) < minObjectDistance)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

    }
}