using Evaluators.TournamentOrganizations;
using Fitnesses;
using System.Collections.Generic;

namespace Evaluators.RatingSystems
{
    public abstract class RatingSystem
    {
        public abstract void UpdateRatings(List<MatchFitness> tournamentMatchFitnesses);

        public abstract void DefinePlayers(List<TournamentTeam> teams, RatingSystemRating[] initialPlayerRaitings);

        public abstract void DisplayRatings();

        public abstract RatingSystemRating[] GetFinalRatings();

        /// <summary>
        /// Checks if every value in lastEvalPopRatings is set to 0 to see if the last evaluation population fitnesses were not set or provided.
        /// </summary>
        /// <param name="lastEvalPopRatings"></param>
        /// <returns>lastEvalPopRatings or NULL if every value in lastEvalPopRatings is 0</returns>
        public virtual RatingSystemRating[] PrepareLastEvalPopRatings(IndividualFitness[] lastEvalIndividualFitnesses)
        {
            if (lastEvalIndividualFitnesses == null || lastEvalIndividualFitnesses.Length == 0)
            {
                return null;
            }

            RatingSystemRating[] ratingSystemRatings = new RatingSystemRating[lastEvalIndividualFitnesses.Length];
            for (int i = 0; i < lastEvalIndividualFitnesses.Length; i++)
            {
                ratingSystemRatings[i] = new RatingSystemRating(lastEvalIndividualFitnesses[i].IndividualID, null, lastEvalIndividualFitnesses[i].AdditionalValues);
            }

            return ratingSystemRatings;
        }
    }

    public class RatingSystemRating
    {
        public int IndividualID { get; set; }

        public Dictionary<string, double> AdditionalValues{ get; set; } // Every rating system has its own rating values

        // TODO replace Mean and StandardDeviation with:
        //public Dictionary<string, float> RatingValues { get; set; }

        public List<IndividualMatchResult> IndividualMatchResults { get; set; }

        public RatingSystemRating(int individualId, List<IndividualMatchResult> individualMatchResults, Dictionary<string, double> additionalValues)
        {
            IndividualID = individualId;
            IndividualMatchResults = individualMatchResults;
            AdditionalValues = additionalValues;
        }

        public Dictionary<string, double> GetAdditionalValues()
        {
            // Create a copy of the dictionary and change the key "Rating" value to - value
            Dictionary<string, double> additionalValuesCopy = new Dictionary<string, double>(AdditionalValues);

            if(!additionalValuesCopy.ContainsKey("Rating"))
                throw new System.Exception("Rating key not found in AdditionalValues dictionary");

            additionalValuesCopy["Rating"] = -additionalValuesCopy["Rating"];

            return additionalValuesCopy;
        }

    }

    public enum RatingSystemType
    {
        TrueSkill,
        Glicko2,
        Elo
    }
}