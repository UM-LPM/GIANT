using Base;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Soccer2D
{
    public class Soccer2DAgentComponent : AgentComponent
    {
        public Soccer2DUtils.SoccerTeam Team;
        public float KickPower { get; set; }

        public Soccer2DEnvironmentController Soccer2DEnvironmentController { get; set; }

        public Vector3 Velocity { get; set; }

        List<SectorComponent> ExploredSectors;

        // Agent fitness variables
        public int SectorsExplored { get; set; }
        public int PassesToOponentGoal { get; set; }
        public int PassesToOwnGoal { get; set; }
        public int Passes { get; set; }
        public int GoalsScored { get; set; }
        public int AutoGoalsScored { get; set; }
        public float AgentToBallDistance { get; set; }
        public float MaxTimeWithoutGoal { get; set; }
        public int CurrentTimeWithoutGoal { get; set; }
        public float TimeLookingAtBall { get; set; }

        protected override void DefineAdditionalDataOnAwake()
        {
            StartPosition = transform.position;
            StartRotation = transform.rotation;
            Soccer2DEnvironmentController = GetComponentInParent<Soccer2DEnvironmentController>();
            ExploredSectors = new List<SectorComponent>();
        }

        public void KickSoccerBall(Soccer2DSoccerBallComponent soccerBall)
        {
            if (soccerBall != null)
            {
                soccerBall.LastTouchedAgent = this;
                var dir = soccerBall.transform.position - transform.position;
                dir = dir.normalized;
                var agentPower = Mathf.Max(0.05f, (Velocity.magnitude / Soccer2DEnvironmentController.AgentMaxAcceleration));
                soccerBall.AddForce(dir * (Soccer2DEnvironmentController.KickPower * agentPower));
                
                if(agentPower > 0.05f)
                    Soccer2DEnvironmentController.AgentTouchedSoccerBall(this);
            }
        }
        public void ResetTimeWithoutGoal()
        {
            if (CurrentTimeWithoutGoal > MaxTimeWithoutGoal)
            {
                MaxTimeWithoutGoal = CurrentTimeWithoutGoal;
            }
            CurrentTimeWithoutGoal = 0;
        }

        public void ResetVelocity()
        {
            Velocity = Vector3.zero;
        }
    }
}