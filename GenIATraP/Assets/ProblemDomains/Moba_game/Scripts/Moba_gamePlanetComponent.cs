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
        private float captureProgress = 0f;
        private float captureSpeed;
        private SpriteRenderer planetCircle;
        private List<AgentComponent> agentsInZone = new List<AgentComponent>();
        private int capturingTeamID = -1; // Team currently capturing

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
            bool isDraw = false; // Flag to track if there's a tie

            for (int i = 0; i < teamCounts.Length; i++)
            {
                if (teamCounts[i] > maxCount)
                {
                    maxCount = teamCounts[i];
                    maxIndex = i;
                    isDraw = false; // Reset draw flag when a new max is found
                }
                else if (teamCounts[i] == maxCount && maxCount > 0)
                {
                    isDraw = true; // A tie occurs
                }
            }

            return isDraw ? -1 : maxIndex;
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



        private void Update()
        {
            int strongestTeam = GetStrongestTeam();
            if (agentsInZone.Count != 0)
            {
                if (strongestTeam != -1)
                {
                    if (capturingTeamID == -1)
                    {
                        capturingTeamID = strongestTeam;
                    }

                    if (strongestTeam == capturingTeamID) // If the same team is capturing
                    {
                        captureProgress = Mathf.Min(1, captureProgress + captureSpeed * Time.deltaTime);
                    }
                    else // If a new team is contesting
                    {
                        captureProgress = Mathf.Max(0, captureProgress - captureSpeed * Time.deltaTime);
                        CapturedTeamID = -1;
                        if (captureProgress == 0)
                        {
                            capturingTeamID = strongestTeam;
                        }
                    }

                    // Update the color to match the capturing progress
                    planetCircle.color = Color.Lerp(whiteColor, teamColors[capturingTeamID], captureProgress);

                    // When fully captured, assign ownership
                    if (captureProgress >= 1)
                    {
                        CapturedTeamID = capturingTeamID;
                    }
                }
                else
                {
                    CapturedTeamID = -1;
                    captureProgress = Mathf.Max(0, captureProgress - captureSpeed * Time.deltaTime);
                    if (capturingTeamID != -1)
                    {
                        planetCircle.color = Color.Lerp(whiteColor, teamColors[capturingTeamID], captureProgress);
                    }
                }
            }
            else
            {
                if (CapturedTeamID == -1)
                {
                    captureProgress = Mathf.Max(0, captureProgress - captureSpeed * Time.deltaTime);
                    if (capturingTeamID != -1)
                    {
                        planetCircle.color = Color.Lerp(whiteColor, teamColors[capturingTeamID], captureProgress);
                    }
                }
            }
        }
    }
}