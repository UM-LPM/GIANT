using Fitnesses;
using System.Collections.Generic;

namespace Evaluators.RatingSystems
{
    public abstract class RatingPlayer
    {
        public int IndividualID { get; set; }

        public List<IndividualMatchResult> IndividualMatchResults { get; set; }

        public RatingPlayer(int individualId)
        {
            IndividualID = individualId;
            IndividualMatchResults = new List<IndividualMatchResult>();
        }

        public abstract void DisplayRating();

        public abstract double GetRating();

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
