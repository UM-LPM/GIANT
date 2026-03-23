using Base;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.Soccer2D
{
    public class Soccer2DAgentComponent : AgentComponent
    {
        public Soccer2DUtils.SoccerTeam Team;
        public int NextKickTime { get; set; }

        public Soccer2DEnvironmentController Soccer2DEnvironmentController { get; set; }

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
            NextKickTime = 0;

            StartPosition = transform.position;
            StartRotation = transform.rotation;
            Soccer2DEnvironmentController = GetComponentInParent<Soccer2DEnvironmentController>();
            ExploredSectors = new List<SectorComponent>();
        }

        public void ResetTimeWithoutGoal()
        {
            if (CurrentTimeWithoutGoal > MaxTimeWithoutGoal)
            {
                MaxTimeWithoutGoal = CurrentTimeWithoutGoal;
            }
            CurrentTimeWithoutGoal = 0;
        }
    }
}