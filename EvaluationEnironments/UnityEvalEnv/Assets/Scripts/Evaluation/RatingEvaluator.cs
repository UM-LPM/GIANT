using AgentOrganizations;
using Base;
using Evaluators.CompetitionOrganizations;
using Fitnesses;
using System.Collections.Generic;
using System.Linq;
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

        public override async Task<CoordinatorEvaluationResult> ExecuteEvaluation(CoordinatorEvalRequestData evalRequestData, Individual[][] individuals)
        {
            while (!CompetitionOrganization.IsCompetitionFinished())
            {
                if (CompetitionOrganization.CreateNewTeamsEachRound)
                {
                    CompetitionOrganization.OrganizeTeams(RatingSystem.Players);
                }

                Match[] competitionMatches = CompetitionOrganization.GenerateCompetitionMatches();
                if (competitionMatches.Length == 0)
                    break;

                List<MatchFitness> matchesFitnesses = await EvaluateCompetitionMatches(evalRequestData, competitionMatches);


                if(CompetitionOrganization is MatrixFactorizationInteractionScheme)
                {
                    throw new System.NotImplementedException("MatrixFactorizationInteractionScheme is not supported in RatingEvaluator!");
                }

                // 1. Update Ratings based on match results
                RatingSystem.UpdateRatings(matchesFitnesses);

                // 2. Update team scores based on match results
                CompetitionOrganization.UpdateTeamsScore(matchesFitnesses, RatingSystem.AllPlayers);
            }

            CompetitionOrganization.DisplayStandings();
            RatingSystem.DisplayRatings();

            // Return the final population fitnesses and BTS node call frequencies
            return new CoordinatorEvaluationResult()
            {
                IndividualFitnesses = GetEvaluationResults()
            };
        }


        public override FinalIndividualFitness[][] GetEvaluationResults()
        {
            RatingSystemRating[][] finalRaitings = RatingSystem.GetFinalRatings();
            FinalIndividualFitnessWrapper[] finalIndividualFitnessWrapper = new FinalIndividualFitnessWrapper[finalRaitings.Length];

            for (int i = 0; i < finalRaitings.Length; i++)
            {
                for (int j = 0; j < finalRaitings[i].Length; j++)
                {
                    // New Version
                    FinalIndividualFitness finalIndividualFitness = new FinalIndividualFitness
                    {
                        IndividualID = finalRaitings[i][j].IndividualID,
                        Value = (float)-finalRaitings[i][j].AdditionalValues["Rating"],
                        IndividualMatchResults = finalRaitings[i][j].IndividualMatchResults,
                        AdditionalValues = finalRaitings[i][j].GetAdditionalValues()
                    };
                    finalIndividualFitness.CalculateAvgMatchResultFitness();
                    if (finalIndividualFitnessWrapper[i] == null)
                        finalIndividualFitnessWrapper[i] = new FinalIndividualFitnessWrapper();
                    finalIndividualFitnessWrapper[i].AddFinalIndividualFitness(finalIndividualFitness);
                }
            }

            return finalIndividualFitnessWrapper.Select(wrapper => wrapper.FinalIndividualFitnesses.ToArray()).ToArray();
        }
    }
}