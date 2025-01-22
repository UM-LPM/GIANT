using AgentOrganizations;
using Base;
using Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evaluators.TournamentOrganizations
{
    public class DoubleEliminationTournament : TournamentOrganization
    {
        private int TeamsWhoGotBye;

        List<TournamentTeam> eliminatedTeams = new List<TournamentTeam>();
        List<Match> tournamentMatches = new List<Match>();

        public DoubleEliminationTournament(List<TournamentTeam> teams, int rounds)
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

            throw new NotImplementedException();
        }

        public override void UpdateTeamsScore(List<MatchFitness> tournamentMatchFitnesses)
        {
            List<MatchFitness> tournamentMatchFitnessesCopy = new List<MatchFitness>(tournamentMatchFitnesses);
            // Add played TournamentMatches to the list of played TournamentMatches (add only matchFitnesses that are not dummy)
            PlayedMatches.AddRange(tournamentMatchFitnessesCopy.FindAll(matchFitness => !matchFitness.IsDummy));

            MatchFitness matchFitness;
            List<MatchFitness> matchFitnesses = new List<MatchFitness>();
            List<MatchFitness> matchFitnessesSwaped = new List<MatchFitness>();
            while (tournamentMatchFitnessesCopy.Count > 0)
            {
                // 1. Get all match data
                matchFitness = new MatchFitness();
                MatchFitness.GetMatchFitness(tournamentMatchFitnessesCopy, matchFitness, matchFitnesses, matchFitnessesSwaped, Coordinator.Instance.SwapTournamentMatchTeams);

                throw new NotImplementedException();
            }
        }

        public override bool IsTournamentFinished()
        {
            throw new NotImplementedException();
        }

        private void ResetTeamByes()
        {
            foreach (var team in Teams)
            {
                team.HasBye = false;
            }
            TeamsWhoGotBye = 0;
        }
    }
}