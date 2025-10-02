using Fitnesses;
using System.Collections.Generic;

namespace Evaluators.CompetitionOrganizations
{
    public abstract class CompetitionPlayer
    {
        public int IndividualID { get; set; }

        public List<IndividualMatchResult> IndividualMatchResults { get; set; }

        public CompetitionPlayer(int individualId)
        {
            IndividualID = individualId;
            IndividualMatchResults = new List<IndividualMatchResult>();
        }

        public abstract void DisplayScore();

        public abstract double GetScore();

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
