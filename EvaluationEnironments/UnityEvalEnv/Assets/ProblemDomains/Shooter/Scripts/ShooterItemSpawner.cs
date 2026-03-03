using AgentControllers;
using AgentOrganizations;
using Base;
using Problems.Collector;
using Spawners;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Problems.Shooter
{
    [DisallowMultipleComponent]
    public class ShooterItemSpawner : Spawner
    {
        [SerializeField] public Transform[] WeaponItemSpawnPoints;
        [SerializeField] public Transform[] HealthItemSpawnPoints;
        [SerializeField] public Transform[] ShieldItemSpawnPoints;

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            throw new System.NotImplementedException();
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            ShooterEnvironmentController shooterEnvironmentController = environmentController as ShooterEnvironmentController;

            List<T> items = new List<T>();

            if (WeaponItemSpawnPoints != null)
            {
                for (int i = 0; i < WeaponItemSpawnPoints.Length; i++)
                {
                    GameObject obj = Instantiate(shooterEnvironmentController.WeaponItemPrefab, WeaponItemSpawnPoints[i].transform.position, Quaternion.identity, gameObject.transform);
                    obj.layer = gameObject.layer;

                    items.Add(obj.GetComponent<T>());
                }
            }

            if(HealthItemSpawnPoints != null)
            {
                for (int i = 0; i < HealthItemSpawnPoints.Length; i++)
                {
                    GameObject obj = Instantiate(shooterEnvironmentController.HealthItemPrefab, HealthItemSpawnPoints[i].transform.position, Quaternion.identity, gameObject.transform);
                    obj.layer = gameObject.layer;
                    items.Add(obj.GetComponent<T>());
                }
            }

            if(ShieldItemSpawnPoints != null)
            {
                for (int i = 0; i < ShieldItemSpawnPoints.Length; i++)
                {
                    GameObject obj = Instantiate(shooterEnvironmentController.ShieldItemPrefab, ShieldItemSpawnPoints[i].transform.position, Quaternion.identity, gameObject.transform);
                    obj.layer = gameObject.layer;
                    items.Add(obj.GetComponent<T>());
                }
            }

            return items.ToArray();
        }

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            ShooterEnvironmentController shooterEnvironmentController = environmentController as ShooterEnvironmentController;

            if (shooterEnvironmentController.WeaponItemPrefab == null)
            {
                throw new System.Exception("WeaponItemPrefab is not defined");
            }

            if(WeaponItemSpawnPoints == null || WeaponItemSpawnPoints.Length == 0)
            {
                DebugSystem.LogWarning("WeaponItemSpawnPoints is not defined or empty. No weapon items will be spawned.");
            }
        }
    }
}