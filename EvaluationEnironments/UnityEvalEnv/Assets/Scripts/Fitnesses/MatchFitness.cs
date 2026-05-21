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
            int teamId = agent.TeamIdentifier.TeamID;

            TeamFitness teamFitness = null;

            for (int i = 0; i < TeamFitnesses.Count; i++)
            {
                if (TeamFitnesses[i].TeamID == teamId)
                {
                    teamFitness = TeamFitnesses[i];
                    break;
                }
            }

            if (teamFitness == null)
            {
                teamFitness = new TeamFitness
                {
                    TeamID = teamId
                };

                TeamFitnesses.Add(teamFitness);
            }

            teamFitness.AddAgentFitness(agent, includeNodeCallFrequencyCounts);
        }

        public float[] GetTeamFitnesses()
        {
            int count = TeamFitnesses.Count;

            float[] result = new float[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = TeamFitnesses[i].GetTeamFitness();
            }

            return result;
        }

        /// <summary>
        /// Fast deterministic key for grouping matches with same teams.
        /// </summary>
        public string GetTeamsKey()
        {
            int count = TeamFitnesses.Count;

            int[] ids = new int[count];

            for (int i = 0; i < count; i++)
            {
                ids[i] = TeamFitnesses[i].TeamID;
            }

            System.Array.Sort(ids);

            return string.Join("_", ids);
        }

        public static void GetMatchFitness(
            List<MatchFitness> competitionMatchFitnesses,
            MatchFitness matchFitness,
            List<MatchFitness> matchFitnesses,
            List<MatchFitness> matchFitnessesSwaped,
            bool swapCompetitionMatchTeams)
        {
            // Take first match
            MatchFitness firstMatch = competitionMatchFitnesses[0];

            matchFitnesses.Add(firstMatch);

            // Faster than Remove(object)
            competitionMatchFitnesses.RemoveAt(0);

            // No swap -> direct copy
            if (!swapCompetitionMatchTeams)
            {
                matchFitness.MatchId = firstMatch.MatchId;
                matchFitness.MatchName = firstMatch.MatchName;
                matchFitness.IsDummy = firstMatch.IsDummy;
                matchFitness.TeamFitnesses = firstMatch.TeamFitnesses;

                matchFitnesses.Clear();
                matchFitnessesSwaped.Clear();

                return;
            }

            // =========================================================
            // Build fast lookup for first match team IDs
            // =========================================================

            int teamCount = firstMatch.TeamFitnesses.Count;

            HashSet<int> firstMatchTeamIds = new HashSet<int>(teamCount);

            for (int i = 0; i < teamCount; i++)
            {
                firstMatchTeamIds.Add(firstMatch.TeamFitnesses[i].TeamID);
            }

            // =========================================================
            // Find matches with same teams
            // (iterate backwards so RemoveAt is safe)
            // =========================================================

            for (int i = competitionMatchFitnesses.Count - 1; i >= 0; i--)
            {
                MatchFitness candidate = competitionMatchFitnesses[i];

                if (candidate.TeamFitnesses.Count != teamCount)
                {
                    continue;
                }

                bool sameTeams = true;

                for (int t = 0; t < candidate.TeamFitnesses.Count; t++)
                {
                    if (!firstMatchTeamIds.Contains(candidate.TeamFitnesses[t].TeamID))
                    {
                        sameTeams = false;
                        break;
                    }
                }

                if (!sameTeams)
                {
                    continue;
                }

                matchFitnessesSwaped.Add(candidate);
                matchFitnesses.Add(candidate);

                // MUCH faster than Remove(object)
                competitionMatchFitnesses.RemoveAt(i);
            }

            // =========================================================
            // Merge all matches
            // =========================================================

            matchFitness.MatchId = firstMatch.MatchId;
            matchFitness.MatchName = firstMatch.MatchName;
            matchFitness.IsDummy = firstMatch.IsDummy;

            // Reuse existing list if possible
            if (matchFitness.TeamFitnesses == null)
            {
                matchFitness.TeamFitnesses = new List<TeamFitness>();
            }
            else
            {
                matchFitness.TeamFitnesses.Clear();
            }

            // TeamID -> merged TeamFitness
            Dictionary<int, TeamFitness> teamMap =
                new Dictionary<int, TeamFitness>();

            // TeamID -> (IndividualID -> IndividualFitness)
            Dictionary<int, Dictionary<int, IndividualFitness>> individualMaps =
                new Dictionary<int, Dictionary<int, IndividualFitness>>();

            for (int m = 0; m < matchFitnesses.Count; m++)
            {
                MatchFitness sourceMatch = matchFitnesses[m];

                for (int t = 0; t < sourceMatch.TeamFitnesses.Count; t++)
                {
                    TeamFitness sourceTeam = sourceMatch.TeamFitnesses[t];

                    if (!teamMap.TryGetValue(sourceTeam.TeamID, out TeamFitness mergedTeam))
                    {
                        mergedTeam = new TeamFitness
                        {
                            TeamID = sourceTeam.TeamID
                        };

                        teamMap.Add(sourceTeam.TeamID, mergedTeam);

                        individualMaps.Add(
                            sourceTeam.TeamID,
                            new Dictionary<int, IndividualFitness>());

                        matchFitness.TeamFitnesses.Add(mergedTeam);
                    }

                    Dictionary<int, IndividualFitness> individualMap =
                        individualMaps[sourceTeam.TeamID];

                    List<IndividualFitness> sourceIndividuals =
                        sourceTeam.IndividualFitness;

                    for (int j = 0; j < sourceIndividuals.Count; j++)
                    {
                        IndividualFitness sourceIndividual =
                            sourceIndividuals[j];

                        if (!individualMap.TryGetValue(
                            sourceIndividual.IndividualID,
                            out IndividualFitness mergedIndividual))
                        {
                            mergedIndividual = new IndividualFitness
                            {
                                IndividualID = sourceIndividual.IndividualID
                            };

                            individualMap.Add(
                                sourceIndividual.IndividualID,
                                mergedIndividual);

                            mergedTeam.IndividualFitness.Add(mergedIndividual);
                        }

                        mergedIndividual.AddIndividualFitness(sourceIndividual);
                    }
                }
            }

            // =========================================================
            // Cleanup
            // =========================================================

            matchFitnesses.Clear();
            matchFitnessesSwaped.Clear();
        }
    }
}