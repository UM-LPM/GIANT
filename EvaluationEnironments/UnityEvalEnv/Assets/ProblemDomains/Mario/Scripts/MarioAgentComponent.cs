using Base;
using UnityEngine;

namespace Problems.Mario
{
    public class MarioAgentComponent: AgentComponent
    {
        public BoxCollider2D BoxCollider2D;

        protected override void DefineAdditionalDataOnAwake()
        {
            BoxCollider2D = GetComponent<BoxCollider2D>();
        }
    }
}
