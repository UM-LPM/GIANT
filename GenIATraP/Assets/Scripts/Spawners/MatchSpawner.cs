using Base;
using UnityEngine;

namespace Spawners
{
    [DisallowMultipleComponent]
    public abstract class MatchSpawner : Spawner
    {
        public abstract void Respawn<T>(EnvironmentControllerBase environmentController, T[] respawnComponents) where T : Component;
    }
}