using Fitnesses;
using System.Collections.Generic;

namespace Evaluators.RatingSystems
{
    public abstract class RatingPlayer // TODO: Rename this to CompetitionPlayer
    {
        public int IndividualID { get; set; }

        public List<IndividualMatchResult> IndividualMatchResults { get; set; }

        public RatingPlayer(int individualId)
        {
            IndividualID = individualId;
            IndividualMatchResults = new List<IndividualMatchResult>();
        }

        public abstract void DisplayRating(); // TODO: Rename this to DisplayPlayerScore

        public abstract double GetRating(); // TODO: Rename this to GetPlayerScore

        public void AddIndividualMatchResult(string matchName, IndividualFitness individualFitness, int[] opponentIDs)
        {
            IndividualMatchResults.Add(new IndividualMatchResult()
            {
                MatchName = matchName,
                Value = individualFitness.Value,
                IndividualValues = individualFitness.IndividualValues,
                OpponentsIDs = opponentIDs
            }); ;
        }
    }
}
