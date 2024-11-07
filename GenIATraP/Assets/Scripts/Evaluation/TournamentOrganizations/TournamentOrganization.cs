using Fitnesses;
using System.Collections.Generic;
using AgentOrganizations;

namespace Evaluators.TournamentOrganizations
{
    public abstract class TournamentOrganization
    {
        public List<TournamentTeam> Teams { get; set; }
        public int Rounds { get; set; }
        public int ExecutedRounds { get; set; }
        public List<MatchFitness> PlayedMatches { get; set; }

        TeamFitness teamFitnessRes1;
        TeamFitness teamFitnessRes2;

        float teamFitness1;
        float teamFitness2;

        public abstract Match[] GenerateTournamentMatches();

        public virtual void UpdateTeamsScore(List<MatchFitness> tournamentMatchFitness)
        {
            // Add played matches to the list of played matches
            PlayedMatches.AddRange(tournamentMatchFitness);

            foreach (MatchFitness matchFitness in tournamentMatchFitness)
            {
                teamFitnessRes1 = matchFitness.TeamFitnesses[0];
                teamFitnessRes2 = matchFitness.TeamFitnesses[1];

                teamFitness1 = teamFitnessRes1.GetTeamFitness();
                teamFitness2 = teamFitnessRes2.GetTeamFitness();

                if (teamFitness1 > teamFitness2)
                {
                    Teams.Find(team => team.TeamId == teamFitnessRes1.TeamID).Score += 2;
                }
                else if (teamFitness1 < teamFitness2)
                {
                    Teams.Find(team => team.TeamId == teamFitnessRes2.TeamID).Score += 2;
                }
                else
                {
                    Teams.Find(team => team.TeamId == teamFitnessRes1.TeamID).Score += 1;
                    Teams.Find(team => team.TeamId == teamFitnessRes2.TeamID).Score += 1;
                }
            }

            // Increment the number of executed rounds
            ExecutedRounds++;
        }

        public abstract void ResetTournament();

        public bool IsTournamentFinished()
        {
            return ExecutedRounds >= Rounds;
        }

        public void DisplayStandings()
        {
            throw new System.NotImplementedException();
            // TODO Add error reporting
        }

        public void ClearTeams()
        {
            Teams.Clear();
        }

        public void AddTeam(TournamentTeam team)
        {
            Teams.Add(team);
        }

        public void AddTeams(List<TournamentTeam> teams)
        {
            Teams.AddRange(teams);
        }

        public void SetTeams(List<TournamentTeam> teams)
        {
            Teams = teams;
        }
    }

    public enum TournamentOrganizationType
    {
        RoundRobin,
        SwissSystem
    }
}