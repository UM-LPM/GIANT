using Base;
using UnityEngine;

namespace Problems.Mario
{
    public class MarioAgentComponent: AgentComponent
    {
        [HideInInspector] public BoxCollider2D BoxCollider2D;
        public int EnemiesKilled = 0;
        public int CoinsCollected = 0;
        public int MysteryBlocksDestroyed = 0;

        protected override void DefineAdditionalDataOnAwake()
        {
            BoxCollider2D = GetComponent<BoxCollider2D>();
        }
    }
}
