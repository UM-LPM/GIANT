using UnityEngine;

namespace Problems.MicroRTS
{
    public class UnitSpawnPositions : MonoBehaviour
    {
        [Header("Unit Prefabs")]
        public GameObject ResourcePrefab;
        public GameObject BasePrefab;
        public GameObject BarracksPrefab;
        public GameObject WorkerPrefab;

        [Header("Resource Spawn Points")]
        public Transform Resource0_Spawn;
        public Transform Resource1_Spawn;

        [Header("Base Spawn Points")]
        public Transform Player0_BaseSpawn;
        public Transform Player1_BaseSpawn;

        [Header("Worker Spawn Points")]
        public Transform Player0_WorkerSpawn;
        public Transform Player1_WorkerSpawn;

        [Header("Dummy Agent Spawn Point")]
        public Transform DummyAgentSpawn;
    }
}
