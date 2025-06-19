using AgentControllers;
using Base;
using Spawners;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.PlanetConquest
{
    public class BaseSpawner : Spawner
    {
        [SerializeField] public Transform[] BaseSpawnPoints;

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            PlanetConquestEnvironmentController PlanetConquestEnvironmentController = environmentController as PlanetConquestEnvironmentController;

            if (PlanetConquestEnvironmentController.BasePrefab == null)
            {
                throw new System.Exception("BasePrefab is not defined");
                // TODO Add error reporting here
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> bases = new List<T>();

            PlanetConquestEnvironmentController planetConquestEnvironmentController = environmentController as PlanetConquestEnvironmentController;
            // Spawn bases
            for (int i = 0; i < environmentController.Match.Teams.Length; i++)
            {
                GameObject baseGameObject = Instantiate(planetConquestEnvironmentController.BasePrefab, BaseSpawnPoints[i].position, BaseSpawnPoints[i].rotation, gameObject.transform);
                baseGameObject.layer = gameObject.layer;

                T _base = baseGameObject.GetComponent<T>();
                BaseComponent baseComponent = _base as BaseComponent;
                baseComponent.TeamIdentifier.TeamID = i;
                baseComponent.HealthComponent.Health = planetConquestEnvironmentController.BaseStartHealth;

                // Assign base sprite
                SpriteRenderer baseRenderer = baseGameObject.GetComponent<SpriteRenderer>();

                if(i < planetConquestEnvironmentController.TeamColors.Length)
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

            // Spawn base for fixed opponent if defined
            if (environmentController.Match.Teams.Length == 1)
            {
                if (planetConquestEnvironmentController.FixedOpponent == null || planetConquestEnvironmentController.FixedOpponent.AgentControllers.Length == 0)
                {
                    throw new System.Exception("Fixed opponent or FixedOpponent.AgentControllers is not defined");
                    // TODO add error reporting here
                }

                int teamIndex = 1; // Fixed opponent is always on the second team
                GameObject baseGameObject = Instantiate(planetConquestEnvironmentController.BasePrefab, BaseSpawnPoints[teamIndex].position, BaseSpawnPoints[teamIndex].rotation, gameObject.transform);
                baseGameObject.layer = gameObject.layer;

                T _base = baseGameObject.GetComponent<T>();
                BaseComponent baseComponent = _base as BaseComponent;
                baseComponent.TeamIdentifier.TeamID = teamIndex;
                baseComponent.HealthComponent.Health = planetConquestEnvironmentController.BaseStartHealth;

                // Assign base sprite
                SpriteRenderer baseRenderer = baseGameObject.GetComponent<SpriteRenderer>();

                if (teamIndex < planetConquestEnvironmentController.TeamColors.Length)
                {
                    baseRenderer.color = planetConquestEnvironmentController.TeamColors[teamIndex];
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