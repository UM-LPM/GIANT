using AgentOrganizations;
using Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Evaluators.TournamentOrganizations
{
    public class SwissSystemTournament : TournamentOrganization
    {
        private int TeamsWhoGotBye;

        List<Match> matches = new List<Match>();
        List<TournamentTeam> unpairedTeams = new List<TournamentTeam>();
        int currentMatchID;
        List<int> opponentTeamIDs;

        public SwissSystemTournament(List<TournamentTeam> teams, int rounds)
        {
            Teams = teams;
            Rounds = rounds < 1 ? (int)Math.Ceiling(Math.Log(teams.Count, 2)) : rounds;
            ExecutedRounds = 0;
            PlayedMatches = new List<MatchFitness>();
        }

        public override void ResetTournament()
        {
            Teams.Clear();
            ExecutedRounds = 0;
            PlayedMatches.Clear();
            TeamsWhoGotBye = 0;
        }

        public override Match[] GenerateTournamentMatches()
        {
            if (IsTournamentFinished())
                return new Match[] { };
            if (TeamsWhoGotBye == Teams.Count)
            {
                ResetTeamByes();
            }

            // 1. Sort teams by score
            List<TournamentTeam> teamsSorted = new List<TournamentTeam>(Teams);
            teamsSorted.Sort((team1, team2) => team2.Score.CompareTo(team1.Score));

            // 2. Pair teams with the closest score (starting from the top). If there's an odd number of players, one player gets a bye (no opponent)
            return PairTeams(teamsSorted);
        }

        private Match[] PairTeams(List<TournamentTeam> teams)
        {
            matches.Clear();
            unpairedTeams = new List<TournamentTeam>(teams);
            currentMatchID = 0;

            // If there's an odd number of players, one player gets a bye
            if (unpairedTeams.Count % 2 != 0)
            {
                TournamentTeam byeTeam = unpairedTeams.FirstOrDefault(p => !p.HasBye);
                if (byeTeam != null)
                {
                    byeTeam.HasBye = true;
                    TeamsWhoGotBye++;
                    unpairedTeams.Remove(byeTeam);
                    matches.Add(new Match(currentMatchID++, new Team[] { byeTeam, new Team(-1) })); // Add a bye pairing with dummy team
                }
            }

            // Pair remaining players by closest score difference
            while (unpairedTeams.Count > 1)
            {
                TournamentTeam t1 = unpairedTeams[0];
                unpairedTeams.RemoveAt(0);

                opponentTeamIDs = PlayedMatches.Where(match => match.TeamFitnesses.Any(team => team.TeamID == t1.TeamId))
                    .Select(PlayedMatches => PlayedMatches.TeamFitnesses.First(team => team.TeamID != t1.TeamId).TeamID)
                    .ToList();

                // Try to find an opponent who hasn't played with t1 yet and has a similar score
                TournamentTeam t2 = unpairedTeams
                    .Where(t => !opponentTeamIDs.Contains(t.TeamId)) 
                    .OrderBy(t => Math.Abs(t1.Score - t.Score))  // Closest score
                    .FirstOrDefault();

                // If no unplayed opponents with similar scores are found, pick the next available
                if (t2 == null)
                {
                    t2 = unpairedTeams[0];
                }
                unpairedTeams.Remove(t2);

                if (Coordinator.Instance.Random.NextDouble() > 0.5)
                    matches.Add(new Match(currentMatchID++, new Team[] { t1, t2 }));
                else
                    matches.Add(new Match(currentMatchID++, new Team[] { t2, t1 }));
            }

            return matches.ToArray();
        }

        private void ResetTeamByes()
        {
            UnityEngine.Debug.Log("All teams have received a bye, resetting byes.");
            foreach (var team in Teams)
            {
                team.HasBye = false;
            }
            TeamsWhoGotBye = 0;
        }
    }
}