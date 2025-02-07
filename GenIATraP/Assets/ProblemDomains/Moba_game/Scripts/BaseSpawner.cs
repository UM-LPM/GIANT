using System.Collections.Generic;
using UnityEngine;
using Spawners;
using Base;

namespace Problems.Moba_game
{
    [DisallowMultipleComponent]
    public class BaseSpawner : Spawner
    {
        [SerializeField] public Transform[] BaseSpawnPoints;
        [SerializeField] public Sprite[] Bases;

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            Moba_gameEnvironmentController Moba_gameEnvironmentController = environmentController as Moba_gameEnvironmentController;

            if (Moba_gameEnvironmentController.BasePrefab == null)
            {
                throw new System.Exception("BasePrefab is not defined");
                // TODO Add error reporting here
            }
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> bases = new List<T>();

            Moba_gameEnvironmentController Moba_gameEnvironmentController = environmentController as Moba_gameEnvironmentController;
            // Spawn bases
            for (int i = 0; i < environmentController.Match.Teams.Length; i++)
            {
                GameObject baseGameObject = Instantiate(Moba_gameEnvironmentController.BasePrefab, BaseSpawnPoints[i].position, BaseSpawnPoints[i].rotation, gameObject.transform);

                T _base = baseGameObject.GetComponent<T>();
                BaseComponent baseComponent = _base as BaseComponent;
                baseComponent.TeamID = environmentController.Match.Teams[i].TeamId;

                // Assign base sprite
                SpriteRenderer baseRenderer = baseGameObject.GetComponent<SpriteRenderer>();
                if (baseRenderer != null && Bases.Length > i)
                {
                    baseRenderer.sprite = Bases[i];
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