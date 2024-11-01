using AgentOrganizations;
using Evaluators.RatingSystems;
using Evaluators.TournamentOrganizations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Evaluators
{
    public abstract class Evaluator
    {
        public RatingSystem RatingSystem { get; set; }
        public TournamentOrganization TournamentOrganization { get; set; }

        public virtual async Task<CoordinatorEvaluationResult> ExecuteEvaluation(CoordinatorEvalRequestData evalRequestData, Individual[] individuals)
        {
            throw new System.NotImplementedException();
        }
    }

    public enum EvaluatiorType
    {
        Simple,
        Complex
    }
}