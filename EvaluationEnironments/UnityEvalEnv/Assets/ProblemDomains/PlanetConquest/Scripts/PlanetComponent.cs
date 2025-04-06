using Base;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.PlanetConquest
{
    public class PlanetComponent : MonoBehaviour
    {
        [SerializeField] public PlanetType PlanetType;

        public TeamIdentifier CapturedTeamIdentifier { get; set; }

        public SpriteRenderer planetOrbRenderer { get; set; }
        public List<PlanetConquestAgentComponent> AgentsInZone { get; set; }

        private int capturingTeamID = -1;
        private float captureProgress = 0f;

        PlanetConquestAgentComponent agent;

        private void Awake()
        {
            AgentsInZone = new List<PlanetConquestAgentComponent>();

            planetOrbRenderer = transform.Find("Orbit").GetComponent<SpriteRenderer>();

            CapturedTeamIdentifier = GetComponent<TeamIdentifier>();
        }

        public int GetStrongestTeam()
        {
            int[] teamCounts = new int[4];

            foreach (PlanetConquestAgentComponent agent in AgentsInZone)
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

            return isDraw ? -1 : maxIndex;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Agent"))
            {
                agent = other.GetComponent<PlanetConquestAgentComponent>();
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
            if (other.CompareTag("Agent"))
            {
                AgentsInZone.Remove(other.GetComponent<PlanetConquestAgentComponent>());
            }
        }

        public void UpdateCaptureProgress(PlanetConquestEnvironmentController environmentController)
        {
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

                    // Update the color to match the capturing progress
                    planetOrbRenderer.color = Color.Lerp(environmentController.PlanetOrbColor, environmentController.TeamColors[capturingTeamID], captureProgress);

                    // When fully captured, assign ownership
                    if (captureProgress >= 1)
                    {
                        if (capturingTeamID != CapturedTeamIdentifier.TeamID)
                        {
                            CapturedTeamIdentifier.TeamID = capturingTeamID;
                            foreach (PlanetConquestAgentComponent agent in AgentsInZone)
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

        private void UpdateCapturingProgress(PlanetConquestEnvironmentController environmentController)
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