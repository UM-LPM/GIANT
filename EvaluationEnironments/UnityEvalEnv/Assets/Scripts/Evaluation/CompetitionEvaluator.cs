using AgentOrganizations;
using Base;
using Configuration;
using Evaluators.CompetitionOrganizations;
using Fitnesses;
using Moserware.Skills;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace Evaluators
{
    public class CompetitionEvaluator : Evaluator
    {
        protected CompetitionOrganization CompetitionOrganization { get; set; }
        protected List<MatchFitness> MatchFitnesses { get; set; }

        public CompetitionEvaluator(CompetitionOrganization competitionOrganization)
        {
            CompetitionOrganization = competitionOrganization;
            MatchFitnesses = new List<MatchFitness>();
        }

        public override async Task<CoordinatorEvaluationResult> ExecuteEvaluation(CoordinatorEvalRequestData evalRequestData, Individual[][] individuals)
        {
            while (!CompetitionOrganization.IsCompetitionFinished())
            {
                if (CompetitionOrganization.CreateNewTeamsEachRound)
                {
                    CompetitionOrganization.OrganizeTeams(null);
                }

                Match[] competitionMatches = CompetitionOrganization.GenerateCompetitionMatches();
                if (competitionMatches.Length == 0)
                    break;

                List<MatchFitness> matchesFitnesses = await EvaluateCompetitionMatches(evalRequestData, competitionMatches);
                CompetitionOrganization.UpdateTeamsScore(matchesFitnesses);
            }

            CompetitionOrganization.DisplayStandings();

            // Return the final population fitnesses and BTS node call frequencies
            return new CoordinatorEvaluationResult()
            {
                IndividualFitnesses = GetEvaluationResults()
            };
        }

        public override async Task<List<MatchFitness>> EvaluateCompetitionMatches(CoordinatorEvalRequestData evalRequestData, Match[] matches)
        {
            MatchFitnesses.Clear();

            int numOfDistinctIndividualsInCompetition = matches.SelectMany(match => match.Teams).Select(team => team.Individuals).SelectMany(individuals => individuals).Distinct().Count();

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
                        // To Each EvalEnvInstance, send a request with the specified range of the CompetitionMatches that need to be evaluated
                        int start = i * numOfMatchesPerInstance;
                        int end = start + numOfMatchesPerInstance + (i == numOfRequiredInstances - 1 ? remainder : 0);

                        var matchesForInstance = matches.Skip(start).Take(end - start).ToArray();
                        string json = JsonConvert.SerializeObject(new
                        CommunicatorEvalRequestData()
                        {
                            Matches = matchesForInstance
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
                            }

                            MatchFitnesses.AddRange(communicatorEvalResponseData.MatchFitnesses);
                        }
                        else
                        {
                            throw new Exception($"Request failed with status code: {response.StatusCode}");
                        }
                    }

                    return MatchFitnesses;
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                DebugSystem.LogError("Evaluation request timed out. Please check the EvalEnvInstances and ensure they are running correctly.");
            }
            catch (Exception ex)
            {
                DebugSystem.LogError($"An error occurred while evaluating competition matches: {ex.Message}\n{ex.StackTrace}");
            }

            throw new Exception("No match fitnesses were returned");
        }

        public virtual FinalIndividualFitness[][] GetEvaluationResults()
        {
            FinalIndividualFitnessWrapper[] finalIndividualFitnessWrapper = new FinalIndividualFitnessWrapper[CompetitionOrganization.Teams.Length];

            for (int i = 0; i < CompetitionOrganization.Teams.Length; i++)
            {
                for (int j = 0; j < CompetitionOrganization.Teams[i].Length; j++)
                {
                    foreach (Individual individual in CompetitionOrganization.Teams[i][j].Individuals)
                    {
                        // Check if individual already exist in finalIndividualFitnessWrapper[i]
                        if (finalIndividualFitnessWrapper[i] == null)
                        {
                            finalIndividualFitnessWrapper[i] = new FinalIndividualFitnessWrapper();
                        }
                        else if (finalIndividualFitnessWrapper[i].IndividualAlreadyAdded(individual.IndividualId))
                        {
                            throw new Exception("Duplicate individual Id in finalIndividualFitnessWrapper!");
                        }
                        // If no add new individual
                        FinalIndividualFitness finalIndividualFitness = new FinalIndividualFitness
                        {
                            IndividualID = individual.IndividualId,
                            Value = -(float)CompetitionOrganization.Teams[i][j].Score,
                            IndividualMatchResults = CompetitionOrganization.Teams[i][j].IndividualMatchResults,
                            AdditionalValues = null
                        };
                        finalIndividualFitness.CalculateAvgMatchResultFitness();
                        finalIndividualFitnessWrapper[i].AddFinalIndividualFitness(finalIndividualFitness);
                    }
                }
            }

            return finalIndividualFitnessWrapper.Select(wrapper => wrapper.FinalIndividualFitnesses.ToArray()).ToArray();
        }
    }
}