using Base;
using Spawners;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.PlanetConquest2
{
    public class PlanetConquest2BaseSpawner : Spawner
    {
        [SerializeField] public Transform[] BaseSpawnPoints;


        PlanetConquest2EnvironmentController planetConquestEnvironmentController;
        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            planetConquestEnvironmentController = environmentController as PlanetConquest2EnvironmentController;

            if (planetConquestEnvironmentController.BasePrefab == null)
            {
                throw new System.Exception("BasePrefab is not defined");
                // TODO Add error reporting here
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> bases = new List<T>();

            planetConquestEnvironmentController = environmentController as PlanetConquest2EnvironmentController;
            // Spawn bases
            for (int i = 0; i < environmentController.Match.Teams.Length; i++)
            {
                GameObject baseGameObject = Instantiate(planetConquestEnvironmentController.BasePrefab, BaseSpawnPoints[i].position, BaseSpawnPoints[i].rotation, gameObject.transform);
                baseGameObject.layer = gameObject.layer;

                T _base = baseGameObject.GetComponent<T>();
                BaseComponent baseComponent = _base as BaseComponent;
                baseComponent.TeamIdentifier.TeamID = environmentController.Match.Teams[i].TeamId;
                baseComponent.HealthComponent.Health = planetConquestEnvironmentController.BaseStartHealth;

                // Assign base sprite
                SpriteRenderer baseRenderer = baseGameObject.GetComponent<SpriteRenderer>();

                if (i < planetConquestEnvironmentController.TeamColors.Length)
                {
                    baseRenderer.color = planetConquestEnvironmentController.TeamColors[i];
                }
                else
                {
                    throw new System.Exception("Base color not defined");
                    // TODO Add error reporting here
                }

                bases.Add(_base);
            }

            return bases.ToArray();
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            throw new System.NotImplementedException();
        }
    }
}
