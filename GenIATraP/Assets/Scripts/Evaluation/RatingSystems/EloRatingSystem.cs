using AgentOrganizations;
using Evaluators.TournamentOrganizations;
using Fitnesses;
using Moserware.Skills;
using System.Collections.Generic;

namespace Evaluators.RatingSystems
{
    public class EloRatingSystem : RatingSystem
    {
        public List<EloPlayer> Players;

        public decimal DefaultRating;
        public int KFactorBellow2100;
        public int KFactorBetween2100And2400;
        public int KFactorAbove2400;

        public EloRatingSystem(decimal defaultRating = 1000, int kFactorBellow2100 = 32, int kFactorBetween2100And2400 = 24, int kFactorAbove2400 = 16)
        {
            Players = new List<EloPlayer>();
            DefaultRating = defaultRating;
            KFactorBellow2100 = kFactorBellow2100;
            KFactorBetween2100And2400 = kFactorBetween2100And2400;
            KFactorAbove2400 = kFactorAbove2400;
        }

        public override void DefinePlayers(List<TournamentTeam> teams, RatingSystemRating[] initialPlayerRaiting)
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateRatings(List<MatchFitness> tournamentMatchFitnesses)
        {
            throw new System.NotImplementedException();
        }

        public override void DisplayRatings()
        {
            throw new System.NotImplementedException();
        }

        public override RatingSystemRating[] GetFinalRatings()
        {
            throw new System.NotImplementedException();
        }
    }

    public class EloPlayer
    {
        public int IndividualID { get; set; }
        public decimal Rating { get; set; }
        public int KFactor { get; set; }

        public List<IndividualMatchResult> IndividualMatchResults { get; set; }

        public EloPlayer(int IndividualId, decimal initialRating, int kFactor)
        {
            IndividualID = IndividualId;
            Rating = initialRating;
            KFactor = kFactor;
            IndividualMatchResults = new List<IndividualMatchResult>();
        }

        public void UpdateRating(decimal newRating)
        {
            Rating = newRating;
        }

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