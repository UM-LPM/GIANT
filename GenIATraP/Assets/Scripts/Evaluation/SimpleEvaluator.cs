using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using UnityEngine;
using System.Collections.Generic;
using AgentOrganizations;

namespace Evaluators
{
    public class SimpleEvaluator : Evaluator
    {
        public SimpleEvaluator()
        {
        }

        public override async Task<CoordinatorEvaluationResult> ExecuteEvaluation(CoordinatorEvalRequestData evalRequestData, Individual[] individuals)
        {
            // TODO Implement
            throw new NotImplementedException();
        }
    }
}