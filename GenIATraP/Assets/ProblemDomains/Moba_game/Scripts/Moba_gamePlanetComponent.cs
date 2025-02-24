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

        private int capturingTeam = -1;
        private float captureProgress = 0f;
        private float captureSpeed;
        private SpriteRenderer planetCircle;
        private List<AgentComponent> agentsInZone = new List<AgentComponent>();

        private readonly Color whiteColor = new Color(1, 1, 1, 0.2f);
        private readonly Color[] teamColors =
        {
            new Color(1, 0, 0, 0.6f),    // Team 1: Red
            new Color(0, 0, 1, 0.6f),   // Team 2: Blue
            new Color(0, 1, 0, 0.6f),  // Team 3: Green
            new Color(1, 0, 1, 0.6f)  // Team 4: Purple
        };

        protected override void DefineAdditionalDataOnAwake()
        {
            planetCircle = transform.Find("Circle").GetComponent<SpriteRenderer>();
            captureSpeed = 1 / captureTime;
            CapturedTeamID = -1;
        }

        private int GetStrongestTeam()
        {
            int[] teamCounts = new int[4];

            foreach (var agent in agentsInZone)
            {
                Moba_gameAgentComponent agentComponent = agent.GetComponent<Moba_gameAgentComponent>();
                if (agentComponent != null && agentComponent.TeamID >= 0 && agentComponent.TeamID <= 3)
                {
                    teamCounts[agentComponent.TeamID]++;
                }
            }

            int maxIndex = -1;
            int maxCount = 0;
            for (int i = 0; i < teamCounts.Length; i++)
            {
                if (teamCounts[i] > maxCount)
                {
                    maxCount = teamCounts[i];
                    maxIndex = i;
                }else if(teamCounts[i] == maxCount){
                    maxIndex = -1;
                }
            }
            return maxIndex;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Agent"))
            {
                agentsInZone.Add(other.GetComponent<Moba_gameAgentComponent>());
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Agent"))
            {
                agentsInZone.Remove(other.GetComponent<Moba_gameAgentComponent>());
            }
        }

        private void UpdateColor()
        {
            if (capturingTeam >= 0 && capturingTeam < teamColors.Length)
            {
                planetCircle.color = Color.Lerp(whiteColor, teamColors[capturingTeam], captureProgress);
            }
            else
            {
                planetCircle.color = whiteColor;
            }
        }

        private void Update()
        {
            if (agentsInZone.Count > 0)
            {
                int strongestTeam = GetStrongestTeam();
                if (strongestTeam == -1) return;

                if (capturingTeam == -1)
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
                if (captureProgress == 1 && capturingTeam != -1)
                {
                    CapturedTeamID = GetStrongestTeam();
                }
            }

            if (captureProgress < 1)
            {
                CapturedTeamID = -1;
            }
            UpdateColor();
        }
    }
}