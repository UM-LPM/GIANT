using Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.DodgeBall
{
    public class DodgeBallAgentComponent : AgentComponent
    {
        public Rigidbody Rigidbody { get; set; }
        public DodgeBallBallPositionComponent BallPosition { get; private set; }
        public DodgeBallBallComponent BallInHand { get; set; }
        public float NextThrowTime { get; set; }
        public DodgeBallEnvironmentController DodgeBallEnvironmentController { get; private set; }

        private List<SectorComponent> ExploredSectors;
        
        // Agent fitness variables
        public int SectorsExplored { get; set; }

        public int BallsPickedUp { get; set; }
        public int BallsThrown { get; set; }
        public int BallsThrownAtOpponent { get; set; }
        public int BallsIntercepted { get; set; }

        public int OpponentsHit { get; set; }
        public int BallsHitBy { get; set; }

        public int NumOfSpawns { get; set; }

        public int MaxSurvivalTime { get; set; }
        public int CurrentSurvivalTime { get; set; }

        public float AgentToBallDistance { get; set; }

        protected override void DefineAdditionalDataOnAwake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            StartPosition = transform.position;
            StartRotation = transform.rotation;
            ExploredSectors = new List<SectorComponent>();
            BallPosition = GetComponentInChildren<DodgeBallBallPositionComponent>();
            BallInHand = null;
            DodgeBallEnvironmentController = GetComponentInParent<DodgeBallEnvironmentController>();
        }

        public void ResetAgent()
        {
            // Reset agent state
            if (Rigidbody == null)
            {
                throw new System.Exception("Rigidbody is null");
            }
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;

            Rigidbody.position = StartPosition;
            Rigidbody.rotation = StartRotation;

            transform.position = StartPosition;
            transform.rotation = StartRotation;

            ResetSurvivalTime();

            // Reset ball state
            if (BallInHand)
            {
                BallInHand.ResetBall();
                BallInHand.transform.SetParent(transform.parent);
                BallInHand = null;
            }
        }

        public void PickUpBall(DodgeBallBallComponent ball)
        {
            if (ball && ball.Parent != this)
            {
                if (ball.Parent)
                {
                    BallsIntercepted++;
                }
                else
                {
                    BallsPickedUp++;
                }

                BallInHand = ball;
                BallInHand.SetBallActive(false);

                BallInHand.transform.position = BallPosition.transform.position;
                BallInHand.transform.SetParent(transform);
                
                BallInHand.Parent = this;
            }
        }

        public void HitByBall(DodgeBallBallComponent ball)
        {
            BallsHitBy++;

            if (!ball.Parent)
            {
                throw new System.Exception("Ball parent is null when agent is hit by ball");
            }
            ball.Parent.OpponentsHit++;
        }

        void OnTriggerEnter(Collider c)
        {
            SectorComponent sector = c.gameObject.GetComponent<SectorComponent>();
            if (sector)
            {
                if (!ExploredSectors.Contains(sector))
                {
                    ExploredSectors.Add(sector);
                    SectorsExplored++;
                }
            }
        }

        public void ResetSurvivalTime()
        {
            if (CurrentSurvivalTime > MaxSurvivalTime)
            {
                MaxSurvivalTime = CurrentSurvivalTime;
            }
            CurrentSurvivalTime = 0;
        }
    }
}
