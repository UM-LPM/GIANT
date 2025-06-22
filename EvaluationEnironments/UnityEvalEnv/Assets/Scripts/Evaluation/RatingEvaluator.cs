using AgentOrganizations;
using Base;
using Configuration;
using Evaluators.RatingSystems;
using Evaluators.TournamentOrganizations;
using Fitnesses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Evaluators
{
    public class RatingEvaluator : TournamentEvaluator
    {
        protected RatingSystem RatingSystem { get; set; }

        public RatingEvaluator(RatingSystem ratingSystem, TournamentOrganization tournamentOrganization) : base (tournamentOrganization)
        {
            RatingSystem = ratingSystem;
        }

        public override async Task<CoordinatorEvaluationResult> ExecuteEvaluation(CoordinatorEvalRequestData evalRequestData, Individual[] individuals)
        {
            while (!TournamentOrganization.IsTournamentFinished())
            {
                Match[] tournamentMatches = TournamentOrganization.GenerateTournamentMatches();
                if (tournamentMatches.Length == 0)
                    break;

                List<MatchFitness> matchesFitnesses = await EvaluateTournamentMatches(evalRequestData, tournamentMatches);

                if (TournamentOrganization is SimilarStrengthOpponentSelection)
                {
                    RatingSystem.UpdateRatings(matchesFitnesses);

                    TournamentOrganization.UpdateTeamsScore(matchesFitnesses, RatingSystem.Players);
                }
                else
                {
                    TournamentOrganization.UpdateTeamsScore(matchesFitnesses);

                    RatingSystem.UpdateRatings(matchesFitnesses);
                }
            }

            TournamentOrganization.DisplayStandings();
            RatingSystem.DisplayRatings();

            // Return the final population fitnesses and BTS node call frequencies
            return new CoordinatorEvaluationResult()
            {
                IndividualFitnesses = GetEvaluationResults()
            };
        }


        public override FinalIndividualFitness[] GetEvaluationResults()
        {
            RatingSystemRating[] finalRaitings = RatingSystem.GetFinalRatings();
            FinalIndividualFitnessWrapper finalIndividualFitnessWrapper = new FinalIndividualFitnessWrapper();

            for (int i = 0; i < finalRaitings.Length; i++)
            {
                // New Version
                FinalIndividualFitness finalIndividualFitness = new FinalIndividualFitness
                {
                    IndividualID = finalRaitings[i].IndividualID,
                    Value = (float)-finalRaitings[i].AdditionalValues["Rating"],
                    IndividualMatchResults = finalRaitings[i].IndividualMatchResults,
                    AdditionalValues = finalRaitings[i].GetAdditionalValues()
                };

                finalIndividualFitness.CalculateAvgMatchResultFitness();

                finalIndividualFitnessWrapper.AddFinalIndividualFitness(finalIndividualFitness);
            }

            return finalIndividualFitnessWrapper.FinalIndividualFitnesses.ToArray();
        }
    }
}