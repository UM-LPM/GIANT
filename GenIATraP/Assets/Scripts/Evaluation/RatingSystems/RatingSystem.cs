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
    }

    public class RatingSystemRating
    {
        public int IndividualID { get; set; }
        public double Mean { get; set; }
        public double StandardDeviation { get; set; }

        // TODO replace Mean and StandardDeviation with:
        //public Dictionary<string, float> RatingValues { get; set; }

        public List<IndividualMatchResult> IndividualMatchResults { get; set; }

        public RatingSystemRating(int individualId, double mean, double standardDeviation)
            : this(individualId, mean, standardDeviation, new List<IndividualMatchResult>())
        { }

        public RatingSystemRating(int individualId, double mean, double standardDeviation, List<IndividualMatchResult> individualMatchResults)
        {
            IndividualID = individualId;
            Mean = mean;
            StandardDeviation = standardDeviation;
            IndividualMatchResults = individualMatchResults;
        }

    }

    public enum RatingSystemType
    {
        TrueSkill,
        Glicko2
    }
}