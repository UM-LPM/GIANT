using AgentOrganizations;
using Base;
using Configuration;
using Evaluators.RatingSystems;
using Evaluators.TournamentOrganizations;
using Fitnesses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Evaluators
{
    public class TournamentEvaluator : Evaluator
    {
        protected TournamentOrganization TournamentOrganization { get; set; }
        protected List<MatchFitness> MatchFitnesses { get; set; }

        public TournamentEvaluator(TournamentOrganization tournamentOrganization)
        {
            TournamentOrganization = tournamentOrganization;
            MatchFitnesses = new List<MatchFitness>();
        }

        public override async Task<CoordinatorEvaluationResult> ExecuteEvaluation(CoordinatorEvalRequestData evalRequestData, Individual[] individuals)
        {
            while (!TournamentOrganization.IsTournamentFinished())
            {
                Match[] tournamentMatches = TournamentOrganization.GenerateTournamentMatches();
                if (tournamentMatches.Length == 0)
                    break;

                List<MatchFitness> matchesFitnesses = await EvaluateTournamentMatches(evalRequestData, tournamentMatches);
                TournamentOrganization.UpdateTeamsScore(matchesFitnesses);
            }

            TournamentOrganization.DisplayStandings();

            // Return the final population fitnesses and BTS node call frequencies
            return new CoordinatorEvaluationResult()
            {
                IndividualFitnesses = GetEvaluationResults()
            };
        }

        public override async Task<List<MatchFitness>> EvaluateTournamentMatches(CoordinatorEvalRequestData evalRequestData, Match[] matches)
        {
            MatchFitnesses.Clear();

            int numOfDistinctIndividualsInTournament = matches.SelectMany(match => match.Teams).Select(team => team.Individuals).SelectMany(individuals => individuals).Distinct().Count();

            int numOfMatches = matches.Length;
            int numOfInstances = evalRequestData.EvalEnvInstances.Length;

            int numOfMatchesPerInstance = numOfMatches / numOfInstances;
            int remainder = numOfMatches % numOfInstances;

            int numOfRequiredInstances = numOfInstances;

            while(numOfMatchesPerInstance == 0)
            {
                numOfRequiredInstances--;

                numOfMatchesPerInstance = numOfMatches / numOfRequiredInstances;
                remainder = numOfMatches % numOfRequiredInstances;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(120); // Set timeout to 120 minutes

                    // Send request to the EvalEnvInstances
                    Task<HttpResponseMessage>[] tasks = new Task<HttpResponseMessage>[numOfRequiredInstances];
                    for (int i = 0; i < numOfRequiredInstances; i++)
                    {
                        // To Each EvalEnvInstance, send a request with the specified range of the TournamentMatches that need to be evaluated
                        int start = i * numOfMatchesPerInstance;
                        int end = start + numOfMatchesPerInstance + (i == numOfRequiredInstances - 1 ? remainder : 0);

                        string json = JsonConvert.SerializeObject(new
                        CommunicatorEvalRequestData()
                        {
                            Matches = matches[start..end]
                        }, MainConfiguration.JSON_SERIALIZATION_SETTINGS);

                        tasks[i] = client.PostAsync(evalRequestData.EvalEnvInstances[i], new StringContent(json, Encoding.UTF8, "application/json"));
                    }

                    // Wait for all tasks to complete
                    await Task.WhenAll(tasks);

                    // Parse the responses from the EvalEnvInstances
                    foreach (Task<HttpResponseMessage> task in tasks)
                    {
                        HttpResponseMessage response = task.Result;
                        if (response.IsSuccessStatusCode)
                        {
                            string result = await response.Content.ReadAsStringAsync();

                            CommunicatorEvalResponseData communicatorEvalResponseData = JsonConvert.DeserializeObject<CommunicatorEvalResponseData>(result);
                            if (communicatorEvalResponseData == null || communicatorEvalResponseData.MatchFitnesses == null || communicatorEvalResponseData.MatchFitnesses.Count == 0)
                            {
                                throw new Exception("Response object is null or no match results are present");
                                // TODO Add error reporting here
                            }

                            MatchFitnesses.AddRange(communicatorEvalResponseData.MatchFitnesses);
                        }
                        else
                        {
                            throw new Exception($"Request failed with status code: {response.StatusCode}");
                            // TODO Add error reporting here
                        }
                    }

                    return MatchFitnesses;
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                // TODO Add error reporting here (Timeout)
            }
            catch (Exception ex)
            {
                // TODO Add error reporting here
            }

            throw new Exception("No match fitnesses were returned");
            // TODO Add error reporting here
        }

        public virtual FinalIndividualFitness[] GetEvaluationResults()
        {
            FinalIndividualFitnessWrapper finalIndividualFitnessWrapper = new FinalIndividualFitnessWrapper();

            foreach(TournamentTeam team in TournamentOrganization.Teams)
            {
                foreach(Individual individual in team.Individuals)
                {
                    // Check if individual already exist in finalIndividualFitnessWrapper
                    if (finalIndividualFitnessWrapper.IndividualAlreadyAdded(individual.IndividualId))
                    {
                        throw new Exception("Duplicate individual Id in finalIndividualFitnessWrapper!");
                    }

                    // If no add new individual
                    FinalIndividualFitness finalIndividualFitness = new FinalIndividualFitness
                    {
                        IndividualID = individual.IndividualId,
                        Value = -(float)team.Score,
                        IndividualMatchResults = team.IndividualMatchResults,
                        AdditionalValues = null
                    };

                    finalIndividualFitness.CalculateAvgMatchResultFitness();

                    finalIndividualFitnessWrapper.AddFinalIndividualFitness(finalIndividualFitness);
                }
            }

            return finalIndividualFitnessWrapper.FinalIndividualFitnesses.ToArray();
        }
    }
}