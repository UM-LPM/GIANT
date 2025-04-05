using Base;
using Spawners;

namespace Problems.PlanetConquest
{
    public class BaseSpawner : Spawner
    {
        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            throw new System.NotImplementedException();
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            throw new System.NotImplementedException();
        }

        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            throw new System.NotImplementedException();
        }
    }
}