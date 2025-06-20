using Base;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Problems.PlanetConquest2
{
    public class PlanetComponent : MonoBehaviour
    {
        [SerializeField] public PlanetType PlanetType;

        public TeamIdentifier CapturedTeamIdentifier { get; set; }

        public SpriteRenderer planetOrbRenderer { get; set; }
        public List<PlanetConquest2AgentComponent> AgentsInZone { get; set; }

        private int capturingTeamID = -1;
        private float captureProgress = 0f;

        PlanetConquest2AgentComponent agent;

        // Temporary variables
        PlanetConquest2AgentComponent planetConquest2AgentComponent;
        SpriteRenderer agentSpriteRenderer;

        private void Awake()
        {
            AgentsInZone = new List<PlanetConquest2AgentComponent>();

            planetOrbRenderer = transform.Find("Orbit").GetComponent<SpriteRenderer>();

            CapturedTeamIdentifier = GetComponent<TeamIdentifier>();
        }

        public int GetStrongestTeam()
        {
            // Find the team with the most agents in the zone (if two teams have the same number of agents, return -1 for a draw)
            if (AgentsInZone.Count == 0)
            {
                return -1; // No agents in the zone
            }

            Dictionary<int, int> teamCounts = new Dictionary<int, int>();

            foreach (PlanetConquest2AgentComponent agent in AgentsInZone)
            {
                if (agent != null && agent.TeamIdentifier.TeamID >= 0 && agent.TeamIdentifier.TeamID <= 3)
                {
                    if (!teamCounts.ContainsKey(agent.TeamIdentifier.TeamID))
                    {
                        teamCounts[agent.TeamIdentifier.TeamID] = 0;
                    }
                    teamCounts[agent.TeamIdentifier.TeamID]++;
                }
            }

            int maxCount = 0;
            int strongestTeamID = -1;
            bool isDraw = false; // Flag to track if there's a tie

            foreach (var kvp in teamCounts)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    strongestTeamID = kvp.Key;
                    isDraw = false; // Reset draw flag when a new max is found
                }
                else if (kvp.Value == maxCount && maxCount > 0)
                {
                    isDraw = true; // A tie occurs
                }
            }

            if (isDraw) {
                return -1; // Return -1 for a draw
            }
            return strongestTeamID; // Return the ID of the strongest team

            /*int[] teamCounts = new int[4];

            foreach (PlanetConquest2AgentComponent agent in AgentsInZone)
            {
                if (agent != null && agent.TeamIdentifier.TeamID >= 0 && agent.TeamIdentifier.TeamID <= 3)
                {
                    teamCounts[agent.TeamIdentifier.TeamID]++;
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

            return isDraw ? -1 : maxIndex;*/
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            planetConquest2AgentComponent = other.GetComponent<PlanetConquest2AgentComponent>();
            if (planetConquest2AgentComponent != null)
            {
                agent = other.GetComponent<PlanetConquest2AgentComponent>();
                if (PlanetType == PlanetType.Lava)
                {
                    agent.EnteredLavaPlanetOrbit++;
                }
                else
                {
                    agent.EnteredIcePlanetOrbit++;
                }
                AgentsInZone.Add(agent);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            planetConquest2AgentComponent = other.GetComponent<PlanetConquest2AgentComponent>();
            if(planetConquest2AgentComponent != null)
            {
                AgentsInZone.Remove(other.GetComponent<PlanetConquest2AgentComponent>());
            }
        }

        public void UpdateCaptureProgress(PlanetConquest2EnvironmentController environmentController)
        {
            if(CapturedTeamIdentifier.TeamID != -1)
            {
                return; // If the planet is already captured, no other team can capture it
            }

            int strongestTeam = GetStrongestTeam();
            if (AgentsInZone.Count != 0)
            {
                if (strongestTeam != -1)
                {
                    if (capturingTeamID == -1)
                    {
                        capturingTeamID = strongestTeam;
                    }

                    if (strongestTeam == capturingTeamID) // If the same team is capturing
                    {
                        captureProgress = Mathf.Min(1, captureProgress + environmentController.PlanetCaptureSpeed * Time.fixedDeltaTime);
                    }
                    else // If a new team is contesting
                    {
                        captureProgress = Mathf.Max(0, captureProgress - environmentController.PlanetCaptureSpeed * Time.fixedDeltaTime);
                        CapturedTeamIdentifier.TeamID = -1;
                        if (captureProgress == 0)
                        {
                            capturingTeamID = strongestTeam;
                        }
                    }

                    // Find agent with the strongest team in the zone
                    agent = AgentsInZone.FirstOrDefault(a => a.TeamIdentifier.TeamID == strongestTeam);
                    agentSpriteRenderer = agent.GetComponent<SpriteRenderer>();

                    // Update the color to match the capturing 
                    planetOrbRenderer.color = Color.Lerp(environmentController.PlanetOrbColor, agentSpriteRenderer.color, captureProgress);

                    // When fully captured, assign ownership
                    if (captureProgress >= 1)
                    {
                        if (capturingTeamID != CapturedTeamIdentifier.TeamID)
                        {
                            CapturedTeamIdentifier.TeamID = capturingTeamID;
                            foreach (PlanetConquest2AgentComponent agent in AgentsInZone)
                            {
                                if (agent.TeamIdentifier.TeamID == CapturedTeamIdentifier.TeamID)
                                {
                                    if (PlanetType == PlanetType.Lava)
                                    {
                                        agent.CapturedLavaPlanet++;
                                    }
                                    else
                                    {
                                        agent.CapturedIcePlanet++;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    CapturedTeamIdentifier.TeamID = -1;
                    UpdateCapturingProgress(environmentController);
                }
            }
            else
            {
                if (CapturedTeamIdentifier.TeamID == -1)
                {
                    UpdateCapturingProgress(environmentController);
                }
            }
        }

        private void UpdateCapturingProgress(PlanetConquest2EnvironmentController environmentController)
        {
            captureProgress = Mathf.Max(0, captureProgress - environmentController.PlanetCaptureSpeed * Time.fixedDeltaTime);
            if (capturingTeamID != -1)
            {
                planetOrbRenderer.color = Color.Lerp(environmentController.PlanetOrbColor, environmentController.TeamColors[capturingTeamID], captureProgress);
            }
        }
    }

    public enum PlanetType
    {
        Lava,
        Ice
    }
}
