using Base;
using static Problems.Soccer.SoccerUtils;
using UnityEngine;
using System.Collections.Generic;

namespace Problems.Soccer
{
    public class SoccerAgentComponent : AgentComponent
    {
        public Rigidbody Rigidbody { get; set; }
        public SoccerTeam Team;
        public float KickPower { get; set; }

        SoccerEnvironmentController SoccerEnvironmentController { get; set; }

        List<SectorComponent> ExploredSectors;
        // Agent fitness variables
        public int SectorsExplored { get; set; }
        public int PassesToOponentGoal { get; set; }
        public int PassesToOwnGoal { get; set; }
        public int Passes { get; set; }
        public int GoalsScored { get; set; }
        public int AutoGoalsScored { get; set; }


        protected override void DefineAdditionalDataOnAwake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            StartPosition = transform.position;
            StartRotation = transform.rotation;
            SoccerEnvironmentController = GetComponentInParent<SoccerEnvironmentController>();
            ExploredSectors = new List<SectorComponent>();
        }

        void OnCollisionEnter(Collision c)
        {
            SoccerBallComponent soccerBall = c.gameObject.GetComponent<SoccerBallComponent>();

            if (soccerBall != null)
            {
                soccerBall.LastTouchedAgent = this;

                var force = SoccerEnvironmentController.KickPower * KickPower;
                var dir = c.contacts[0].point - transform.position;
                dir = dir.normalized;
                soccerBall.Rigidbody.AddForce(dir * force);

                SoccerEnvironmentController.AgentTouchedSoccerBall(this);

                return;
            }
        }

        void OnTriggerEnter(Collider c)
        {
            SectorComponent sector = c.gameObject.GetComponent<SectorComponent>();
            if (sector != null)
            {
                if (!ExploredSectors.Contains(sector))
                {
                    ExploredSectors.Add(sector);
                    SectorsExplored++;
                }
            }
        }
    }
}