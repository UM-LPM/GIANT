using Base;
using UnityEngine;

namespace Problems.Pong
{
    public class PongAgentComponent : AgentComponent
    {
        public PongUtils.PongSide Side;

        public BoxCollider2D BoxCollider2D { get; private set; }

        public Vector3 Forward { get; set; } = Vector3.right;

        // Agent fitness variables
        public int BallBounces { get; set; }
        public int PointsScored { get; set; } // Number of points scored by the agent

        protected override void DefineAdditionalDataOnAwake()
        {
            BoxCollider2D = GetComponent<BoxCollider2D>();
        }


        public void PongBallBounce(PongBallComponent pongBall)
        {
            if (pongBall != null)
            {
                BallBounces++;
            }
        }

    }
}