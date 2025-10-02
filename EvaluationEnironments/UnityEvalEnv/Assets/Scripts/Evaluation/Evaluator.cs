using AgentOrganizations;
using Base;
using Evaluators.CompetitionOrganizations;
using Evaluators.CompetitionOrganizations;
using Fitnesses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Evaluators
{
    public abstract class Evaluator
    {

        public virtual async Task<CoordinatorEvaluationResult> ExecuteEvaluation(CoordinatorEvalRequestData evalRequestData, Individual[] individuals)
        {
            throw new System.NotImplementedException();
        }

        public virtual async Task<List<MatchFitness>> EvaluateCompetitionMatches(CoordinatorEvalRequestData evalRequestData, Match[] matches)
        {
            throw new System.NotImplementedException();
        }
    }

    public enum EvaluatiorType
    {
        Simple,
        Competition,
        Rating
    }
}