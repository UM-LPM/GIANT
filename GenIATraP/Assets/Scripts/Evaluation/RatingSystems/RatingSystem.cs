using Evaluators.TournamentOrganizations;
using Fitnesses;
using System.Collections.Generic;

namespace Evaluators.RatingSystems
{
    public abstract class RatingSystem
    {
        public abstract void UpdateRatings(List<MatchFitness> tournamentMatchFitnesses);

        public abstract void DefinePlayers(List<TournamentTeam> teams, RatingSystemRating[] initialPlayerRaiting);

        public abstract void DisplayRatings();

        public abstract RatingSystemRating[] GetFinalRatings();
    }

    public class RatingSystemRating
    {
        public int IndividualID { get; set; }
        public double Mean { get; set; }
        public double StandardDeviation { get; set; }

        public RatingSystemRating(int individualId, double mean, double standardDeviation)
        {
            IndividualID = individualId;
            Mean = mean;
            StandardDeviation = standardDeviation;
        }

    }

    public enum RatingSystemType
    {
        TrueSkill,
        Glicko2
    }
}