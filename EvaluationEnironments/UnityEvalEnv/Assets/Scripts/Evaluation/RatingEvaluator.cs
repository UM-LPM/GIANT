using AgentOrganizations;
using Base;
using Evaluators.CompetitionOrganizations;
using Fitnesses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Evaluators
{
    public class RatingEvaluator : CompetitionEvaluator
    {
        protected RatingSystem RatingSystem { get; set; }

        public RatingEvaluator(RatingSystem ratingSystem, CompetitionOrganization competitionOrganization) : base (competitionOrganization)
        {
            RatingSystem = ratingSystem;
        }

        public override async Task<CoordinatorEvaluationResult> ExecuteEvaluation(CoordinatorEvalRequestData evalRequestData, Individual[] individuals)
        {
            while (!CompetitionOrganization.IsCompetitionFinished())
            {
                if (CompetitionOrganization.CreateNewTeamsEachRound)
                {
                    CompetitionOrganization.OrganizeTeams(RatingSystem.Players.ToArray());
                }

                Match[] competitionMatches = CompetitionOrganization.GenerateCompetitionMatches();
                if (competitionMatches.Length == 0)
                    break;

                List<MatchFitness> matchesFitnesses = await EvaluateCompetitionMatches(evalRequestData, competitionMatches);

                if (CompetitionOrganization is SimilarStrengthOpponentSelection)
                {
                    RatingSystem.UpdateRatings(matchesFitnesses);

                    CompetitionOrganization.UpdateTeamsScore(matchesFitnesses, RatingSystem.Players);
                }
                else
                {
                    CompetitionOrganization.UpdateTeamsScore(matchesFitnesses);

                    RatingSystem.UpdateRatings(matchesFitnesses);
                }
            }

            CompetitionOrganization.DisplayStandings();
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