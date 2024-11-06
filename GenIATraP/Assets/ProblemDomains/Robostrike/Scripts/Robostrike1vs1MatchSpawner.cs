using AgentControllers;
using AgentOrganizations;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Spawners;

namespace Problems.Robostrike
{
    public class Robostrike1vs1MatchSpawner : MatchSpawner
    {
        [SerializeField] public Sprite[] Hulls;
        [SerializeField] public Sprite[] Turrets;
        [SerializeField] public Sprite[] Tracks;
        [SerializeField] public Sprite[] Guns;


        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            if (Hulls == null || Hulls.Length == 0 || Turrets == null || Turrets.Length == 0 || Tracks == null || Tracks.Length == 0 || Guns == null || Guns.Length == 0)
            {
                throw new System.Exception("Sprites are missing");
                // odo add error reporting here
            }
        }

        public override List<T> Spawn<T>(EnvironmentControllerBase environmentController)
        {
            validateSpawnConditions(environmentController);

            List<T> agents = new List<T>();

            throw new System.NotImplementedException();

            return agents;
        }
    }
}