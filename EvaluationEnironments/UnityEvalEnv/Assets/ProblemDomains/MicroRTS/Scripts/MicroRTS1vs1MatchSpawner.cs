using Base;
using Spawners;

namespace Problems.MicroRTS
{
    public class MicroRTS1vs1MatchSpawner : MatchSpawner
    {
        public override void validateSpawnConditions(EnvironmentControllerBase environmentController)
        {
            // TOOD
            throw new System.NotImplementedException();
        }

        public override T[] Spawn<T>(EnvironmentControllerBase environmentController)
        {
            // TOOD
            throw new System.NotImplementedException();
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent)
        {
            // Not required for this problem domain
            throw new System.NotImplementedException();
        }

        public override void Respawn<T>(EnvironmentControllerBase environmentController, T[] respawnComponents)
        {
            // Not required for this problem domain
            throw new System.NotImplementedException();
        }
    }
}