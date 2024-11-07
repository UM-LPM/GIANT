using AgentOrganizations;
using Evaluators.RatingSystems;
using Evaluators.TournamentOrganizations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Evaluators
{
    public class ComplexEvaluator : Evaluator
    {
        public ComplexEvaluator(RatingSystem ratingSystem, TournamentOrganization tournamentOrganization)
        {
            RatingSystem = ratingSystem;
            TournamentOrganization = tournamentOrganization;
        }

        public override async Task<CoordinatorEvaluationResult> ExecuteEvaluation(CoordinatorEvalRequestData evalRequestData, Individual[] individuals)
        {
            // TODO Implement
            throw new NotImplementedException();

            /*while (!TournamentOrganization.IsTournamentFinished())
            {
                List<TournamentMatch> tournamentMatches = TournamentOrganization.GenerateTournamentMatches();
                if (tournamentMatches.Count == 0)
                    break;

                evalRequestData.tournamentMatches = tournamentMatches;

                TournamentOutcome tournamentOutcome = await EvaluateTournamentMatches(evalRequestData);
                TournamentOrganization.UpdateTeamsScore(tournamentOutcome);

                RatingSystem.UpdateRatings(tournamentOutcome);
            }

            TournamentOrganization.DisplayStandings();
            RatingSystem.DisplayRatings();

            return GetEvaluationResults();*/
        }

        /*public override async Task<TournamentOutcome> EvaluateTournamentMatches(CoordinatorEvalRequestData evalRequestData)
        {
            // TODO Implement
            throw new NotImplementedException();
        }*/
    }
}