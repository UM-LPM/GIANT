using Base;
using UnityEngine;

namespace Spawners
{
    [DisallowMultipleComponent]
    public abstract class MatchSpawner : Spawner
    {
        public abstract void Respawn<T>(EnvironmentControllerBase environmentController, T respawnComponent) where T : Component;

        public abstract void SwitchSpawnPlaces<T>(EnvironmentControllerBase environmentController) where T : Component;
    }
}