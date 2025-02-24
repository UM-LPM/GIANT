using System.Collections.Generic;
using System.Xml.Serialization;
using Base;
using UnityEngine;

namespace Problems.Moba_game
{
    public class Moba_gamePlanetComponent : PlanetComponent
    {
        public int CapturedTeamID { get; set; }
        public string Type { get; set; }
        public float captureTime = 5f;


        private enum Team { NONE, BLUE, RED }
        private Team capturingTeam = Team.NONE;
        private float captureProgress = 0f;
        private float captureSpeed;
        private Color whiteColor = new Color(1,1,1,0.2f);
        private Color blueColor = new Color(0,0,1,0.6f);
        private Color redColor = new Color(1,0,0,0.6f);
        private SpriteRenderer planetCircle;
        private List<AgentComponent> agentsInZone = new List<AgentComponent>();

        protected override void DefineAdditionalDataOnAwake()
        {
            planetCircle = transform.Find("Circle").GetComponent<SpriteRenderer>();
            captureSpeed = 1 / captureTime;
            CapturedTeamID = -1;
        }
        Team GetStrongestTeam()
        {
            int blueCount = 0, redCount = 0;

            foreach (var agent in agentsInZone)
            {
                Moba_gameAgentComponent agentComponent = agent.GetComponent<Moba_gameAgentComponent>();
                if (agentComponent.TeamID == 0) redCount++;
                if (agentComponent.TeamID == 1) blueCount++;
            }

            if (blueCount > redCount) return Team.BLUE;
            if (redCount > blueCount) return Team.RED;
            return Team.NONE;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Agent"))
            {
                agentsInZone.Add(other.GetComponent<Moba_gameAgentComponent>());
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Agent"))
            {
                agentsInZone.Remove(other.GetComponent<Moba_gameAgentComponent>());
            }
        }

        void UpdateColor()
        {
            if (capturingTeam == Team.BLUE)
            {
                planetCircle.color = Color.Lerp(whiteColor, blueColor, captureProgress);
            }
            else if (capturingTeam == Team.RED)
            {
                planetCircle.color = Color.Lerp(whiteColor, redColor, captureProgress);
            }
            else
            {
                planetCircle.color = whiteColor;
            }
        }
        void Update()
        {
            if (agentsInZone.Count > 0)
            {
                Team strongestTeam = GetStrongestTeam();

                if (capturingTeam == Team.NONE)
                {
                    capturingTeam = strongestTeam;
                }

                if (strongestTeam == capturingTeam)
                {
                    captureProgress = Mathf.Min(1, captureProgress + captureSpeed * Time.deltaTime);
                }
                else
                {
                    captureProgress = Mathf.Max(0, captureProgress - captureSpeed * Time.deltaTime);
                    if (captureProgress == 0)
                    {
                        capturingTeam = strongestTeam;
                    }
                }
            }

            if (captureProgress == 1 && capturingTeam != Team.NONE)
            {
                CapturedTeamID = capturingTeam == Team.RED ? 0 : 1;
            }
            if (captureProgress < 1)
            {
                CapturedTeamID = -1;
            }
            UpdateColor();
        }
    }
}