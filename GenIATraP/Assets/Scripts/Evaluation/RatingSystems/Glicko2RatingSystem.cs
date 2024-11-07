using AgentOrganizations;
using Evaluators.TournamentOrganizations;
using Fitnesses;
using System.Collections.Generic;

namespace Evaluators.RatingSystems
{
    public class Glicko2RatingSystem : RatingSystem
    {
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
}