using Base;
using System.Collections.Generic;

namespace Fitnesses
{
    public class MatchFitness
    {
        public int MatchId { get; set; }
        public string MatchName { get; set; }
        public List<TeamFitness> TeamFitnesses { get; set; }
        public bool IsDummy { get; set; }

        public MatchFitness()
        {
            TeamFitnesses = new List<TeamFitness>();
            IsDummy = false;
        }

        public void AddAgentFitness(AgentComponent agent, bool includeNodeCallFrequencyCounts)
        {
            TeamFitness teamFitness = TeamFitnesses.Find(tf => tf.TeamID == agent.TeamIdentifier.TeamID);
            if(teamFitness == null)
            {
                teamFitness = new TeamFitness();
                teamFitness.TeamID = agent.TeamIdentifier.TeamID;
                TeamFitnesses.Add(teamFitness);
            }

            teamFitness.AddAgentFitness(agent, includeNodeCallFrequencyCounts);
        }

        public float[] GetTeamFitnesses()
        {
            float[] teamFitnesses = new float[TeamFitnesses.Count];
            for(int i = 0; i < TeamFitnesses.Count; i++)
            {
                teamFitnesses[i] = TeamFitnesses[i].GetTeamFitness();
            }

            return teamFitnesses;
        }

        public bool ContainsSameTeams(MatchFitness matchFitness2)
        {
            if (TeamFitnesses.Count != matchFitness2.TeamFitnesses.Count)
            {
                return false;
            }

            foreach (var teamFitness1 in TeamFitnesses)
            {
                if (!matchFitness2.TeamFitnesses.Exists(tf => tf.TeamID == teamFitness1.TeamID))
                {
                    return false;
                }
            }

            return true;
        }

        public static void GetMatchFitness(List<MatchFitness> competitionMatchFitnesses, MatchFitness matchFitness, List<MatchFitness> matchFitnesses, List<MatchFitness> matchFitnessesSwaped, bool swapCompetitionMatchTeams)
        {
            matchFitnesses.Add(competitionMatchFitnesses[0]);
            competitionMatchFitnesses.RemoveAt(0);

            if (swapCompetitionMatchTeams)
            {
                // Find all matchFitnesses with the same TeamIDs
                matchFitnessesSwaped = competitionMatchFitnesses.FindAll(match => matchFitnesses[0].ContainsSameTeams(match));

                if (matchFitnessesSwaped.Count > 0)
                {
                    // Add all matchFitnesses with the same TeamIDs to the list of matchFitnesses
                    matchFitnesses.AddRange(matchFitnessesSwaped);

                    // Remove all matchFitnesses with the same TeamIDs from the list of competition matchFitnesses
                    foreach (MatchFitness matchSwaped in matchFitnessesSwaped)
                    {
                        competitionMatchFitnesses.Remove(matchSwaped);
                    }

                    // Join all matchFitnesses with the same TeamIDs
                    matchFitness.MatchName = matchFitnesses[0].MatchName;
                    matchFitness.IsDummy = matchFitnesses[0].IsDummy;
                    matchFitness.TeamFitnesses = new List<TeamFitness>();
                    foreach (MatchFitness matchJoined in matchFitnesses)
                    {
                        foreach (TeamFitness teamFitness in matchJoined.TeamFitnesses)
                        {
                            TeamFitness teamFitnessJoined = matchFitness.TeamFitnesses.Find(tf => tf.TeamID == teamFitness.TeamID);
                            if (teamFitnessJoined == null)
                            {
                                teamFitnessJoined = new TeamFitness();
                                teamFitnessJoined.TeamID = teamFitness.TeamID;
                                matchFitness.TeamFitnesses.Add(teamFitnessJoined);
                            }

                            foreach (IndividualFitness individualFitness in teamFitness.IndividualFitness)
                            {
                                teamFitnessJoined.IndividualFitness.Add(individualFitness);
                            }
                        }
                    }
                }
            }
            else
            {
                matchFitness.MatchName = matchFitnesses[0].MatchName;
                matchFitness.IsDummy = matchFitnesses[0].IsDummy;
                matchFitness.TeamFitnesses = matchFitnesses[0].TeamFitnesses;
            }

            // Clear current data
            matchFitnesses.Clear();
            matchFitnessesSwaped.Clear();
        }
    }
}