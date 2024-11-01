using AgentOrganizations;
using Evaluators.RatingSystems;
using Evaluators.TournamentOrganizations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Evaluators
{
    public class ComplexEvaluator : Evaluator
    {
        public ComplexEvaluator(RatingSystem ratingSystem, TournamentOrganization tournamentOrganization)
        {
            RatingSystem = ratingSystem;
            TournamentOrganization = tournamentOrganization;
        }

        public override async Task<CoordinatorEvaluationResult> ExecuteEvaluation(CoordinatorEvalRequestData evalRequestData, Individual[] individuals)
        {
            // TODO Implement
            throw new NotImplementedException();
        }
    }
}