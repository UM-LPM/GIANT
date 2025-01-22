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

        public void AddAgentFitness(AgentComponent agent)
        {
            TeamFitness teamFitness = TeamFitnesses.Find(tf => tf.TeamID == agent.TeamID);
            if(teamFitness == null)
            {
                teamFitness = new TeamFitness();
                teamFitness.TeamID = agent.TeamID;
                TeamFitnesses.Add(teamFitness);
            }

            teamFitness.AddAgentFitness(agent);
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

        public static void GetMatchFitness(List<MatchFitness> tournamentMatchFitnesses, MatchFitness matchFitness, List<MatchFitness> matchFitnesses, List<MatchFitness> matchFitnessesSwaped, bool swapTournamentMatchTeams)
        {
            matchFitnesses.Add(tournamentMatchFitnesses[0]);
            tournamentMatchFitnesses.RemoveAt(0);

            if (swapTournamentMatchTeams)
            {
                // Find all matchFitnesses with the same TeamIDs
                matchFitnessesSwaped = tournamentMatchFitnesses.FindAll(match => matchFitnesses[0].ContainsSameTeams(match));

                if (matchFitnessesSwaped.Count > 0)
                {
                    // Add all matchFitnesses with the same TeamIDs to the list of matchFitnesses
                    matchFitnesses.AddRange(matchFitnessesSwaped);

                    // Remove all matchFitnesses with the same TeamIDs from the list of tournament matchFitnesses
                    foreach (MatchFitness matchSwaped in matchFitnessesSwaped)
                    {
                        tournamentMatchFitnesses.Remove(matchSwaped);
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
                matchFitness = matchFitnesses[0];
            }

            // Clear current data
            matchFitnesses.Clear();
            matchFitnessesSwaped.Clear();
        }
    }
}