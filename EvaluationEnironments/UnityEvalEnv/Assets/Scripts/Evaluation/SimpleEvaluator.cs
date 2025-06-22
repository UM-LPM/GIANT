using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using UnityEngine;
using System.Collections.Generic;
using AgentOrganizations;
using Fitnesses;
using Configuration;
using Base;

namespace Evaluators
{
    public class SimpleEvaluator : Evaluator
    {
        public SimpleEvaluator()
        {
        }

        public override async Task<CoordinatorEvaluationResult> ExecuteEvaluation(CoordinatorEvalRequestData evalRequestData, Individual[] individuals)
        {
            Match[] matches = GenerateMatches(individuals);

            int numOfMatches = matches.Length;
            int numOfInstances = evalRequestData.EvalEnvInstances.Length;

            int numOfMatchesPerInstance = numOfMatches / numOfInstances;
            int remainder = numOfMatches % numOfInstances;

            int numOfRequiredInstances = numOfInstances;

            while (numOfMatchesPerInstance == 0)
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

                    FinalIndividualFitnessWrapper finalIndividualFitnessWrapper = new FinalIndividualFitnessWrapper();

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

                            for (int i = 0; i < communicatorEvalResponseData.MatchFitnesses.Count; i++)
                            {
                                for (int j = 0; j < communicatorEvalResponseData.MatchFitnesses[i].TeamFitnesses.Count; j++)
                                {
                                    for (int z = 0; z < communicatorEvalResponseData.MatchFitnesses[i].TeamFitnesses[j].IndividualFitness.Count; z++)
                                    {
                                        finalIndividualFitnessWrapper.UpdateFinalIndividualFitnesses(communicatorEvalResponseData.MatchFitnesses[i].TeamFitnesses[j].IndividualFitness[z], communicatorEvalResponseData.MatchFitnesses[i].MatchName, new int[] { });
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new Exception($"Request failed with status code: {response.StatusCode}");
                            // TODO Add error reporting here
                        }
                    }

                    // Sort finalIndividualFitnessWrapper.FinalIndividualFitnesses by IndividualID
                    finalIndividualFitnessWrapper.FinalIndividualFitnesses.Sort((x, y) => x.IndividualID.CompareTo(y.IndividualID));

                    // Return the final population fitnesses and BTS node call frequencies
                    return new CoordinatorEvaluationResult()
                    {
                        IndividualFitnesses = finalIndividualFitnessWrapper.FinalIndividualFitnesses.ToArray(),
                    };
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

/// <summary>
/// For each individual, create a match with a team containing only that individual
/// </summary>
/// <param name="individuals"></param>
public Match[] GenerateMatches(Individual[] individuals)
        {
            Match[] matches = new Match[individuals.Length];

            for (int i = 0; i < individuals.Length; i++)
            {
                Team team = ScriptableObject.CreateInstance<Team>();
                team.Initialize(i, "Team_" + i, new Individual[] { individuals[i] });

                matches[i] = ScriptableObject.CreateInstance<Match>();
                matches[i].Initialize(i, new Team[] { team });
            }

            return matches;
        }
    }
}