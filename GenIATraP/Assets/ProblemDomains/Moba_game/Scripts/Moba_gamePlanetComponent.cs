using System.Collections.Generic;
using System.Xml.Serialization;
using Base;
using UnityEngine;

namespace Problems.Moba_game
{
    public class Moba_gamePlanetComponent : PlanetComponent
    {
        public enum Team { NONE, BLUE, RED }

        public Team capturingTeam = Team.NONE;
        public float captureProgress = 0f;
        public float captureTime = 5f;  // Čas zajetja v sekundah
        private float captureSpeed;
        public int CapturedTeamID = -1;
        public Color whiteColor = Color.white;
        public Color blueColor = Color.blue;
        public Color redColor = Color.red;

        private SpriteRenderer planetCircle;
        private List<AgentComponent> agentsInZone = new List<AgentComponent>();
        protected override void DefineAdditionalDataOnAwake()
        {
            planetCircle = transform.Find("Circle").GetComponent<SpriteRenderer>();
            captureSpeed = 1 / captureTime; 
        }
        Team GetStrongestTeam()
        {
            int blueCount = 0, redCount = 0;

            foreach (var agent in agentsInZone)
            {
                Moba_gameAgentComponent agentComponent = agent.GetComponent<Moba_gameAgentComponent>();  // Predpostavimo, da agenti imajo komponento
                if (agentComponent.TeamID == 0) blueCount++;
                if (agentComponent.TeamID == 1) redCount++;
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
            if (captureProgress == 1){
                CapturedTeamID = 0;
            }
            UpdateColor();
        }
    }
}