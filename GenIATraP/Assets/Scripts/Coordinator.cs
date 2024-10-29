using System;
using UnityEngine;
using static Unity.VisualScripting.Member;
using UnityEngine.SocialPlatforms;
using System.Threading;
using System.Net;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;

public class Coordinator : MonoBehaviour
{
    [HideInInspector] public static Coordinator Instance;

    [Header("HTTP Server Configuration")]
    [SerializeField] public string CoordinatorURI = "http://localhost:4000/";



    private HttpListener Listener;
    private Thread ListenerThread;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        ReadDataFromConfig();

        InitializeHttpServer();
    }

    void Start()
    {
        ListenerThread = new Thread(StartListener);
        ListenerThread.Start();
    }

    void ReadDataFromConfig()
    {
        if (MenuManager.Instance != null)
        {
            if (MenuManager.Instance.MainConfiguration != null)
            {
                CoordinatorURI = MenuManager.Instance.MainConfiguration.CoordinatorURI;
            }
            else
            {
                CoordinatorURI = MenuManager.Instance.CoordinatorURI;
            }
        }
    }

    void InitializeHttpServer()
    {
        try
        {
            Listener = new HttpListener();
            Listener.Prefixes.Add(CoordinatorURI);
            Listener.Start();

            Debug.Log("Coordinator HTTP server is running");

        }
        catch (Exception e)
        {
            Debug.Log("Coordinator HTTP server is already running");
        }
    }

    private void StartListener()
    {
        while (Listener.IsListening)
        {
            try
            {
                var context = Listener.GetContext(); // Block until a client connects.
                ThreadPool.QueueUserWorkItem(o => HandleRequest(context));
            }
            catch (HttpListenerException)
            {
                // The Listener was stopped, exit the loop
                break;
            }
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {
        UnityMainThreadDispatcher.Instance().Enqueue((CordinateEvaluation(context)));
    }

    public void StopListener()
    {
        Listener.Stop();
        Listener.Close();
        ListenerThread.Join();
        ListenerThread.Abort();


        Destroy(this.gameObject);
    }

    /// <summary>
    /// This method is called when the coordinator receives a request to evaluate a range of individuals. 
    /// It will distribute the evaluation of the individuals to the evaluation environments and return the final population fitnesses and BTS node call frequencies.
    /// </summary>
    IEnumerator CordinateEvaluation(HttpListenerContext context)
    {
        // Read body from the request
        if (!context.Request.HasEntityBody)
        {
            throw new Exception("No client data was sent with the request.");
        }
        System.IO.Stream body = context.Request.InputStream;
        System.Text.Encoding encoding = context.Request.ContentEncoding;
        System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);

        // Convert the data to a string and display it on the console.
        string s = reader.ReadToEnd();
        CoordinatorEvalRequestData coordinatorEvalRequestData = JsonConvert.DeserializeObject<CoordinatorEvalRequestData>(s);
        Debug.Log("Coordinator Cordinate Evaluation " + coordinatorEvalRequestData.evalRangeStart + "-" + coordinatorEvalRequestData.evalRangeEnd + ", EvalInstances: " + coordinatorEvalRequestData.evalEnvInstancesToString());

        int numOfIndividuals = coordinatorEvalRequestData.evalRangeEnd;
        int numOfInstances = coordinatorEvalRequestData.evalEnvInstances.Length;

        int numOfIndividualsPerInstance = numOfIndividuals / numOfInstances;
        int remainder = numOfIndividuals % numOfInstances;

        using (HttpClient client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromMinutes(120); // Set timeout to 120 minutes

            Task<HttpResponseMessage>[] tasks = new Task<HttpResponseMessage>[numOfInstances];
            for (int i = 0; i < numOfInstances; i++)
            {
                tasks[i] = client.PostAsync(coordinatorEvalRequestData.evalEnvInstances[i], new StringContent(JsonConvert.SerializeObject(new EvalRequestData() { evalRangeStart = i * numOfIndividualsPerInstance, evalRangeEnd = i * numOfIndividualsPerInstance + numOfIndividualsPerInstance + (i == numOfInstances - 1 ? remainder : 0) }), Encoding.UTF8, "application/json"));
            }

            while (!Task.WhenAll(tasks).IsCompleted)
            {
                yield return null; // Wait for tasks to complete
            }

            FitnessIndividual[] FinalPopFitnesses = new FitnessIndividual[numOfIndividuals];
            int[][] FinalBtsNodeCallFrequencies = new int[numOfIndividuals][];
            foreach (Task<HttpResponseMessage> task in tasks)
            {
                HttpResponseMessage response = task.Result;
                if (response.IsSuccessStatusCode)
                {
                    var taskContent = response.Content.ReadAsStringAsync();
                    while(!taskContent.IsCompleted)
                    {
                        yield return null; // Wait for task to complete
                    }

                    string result = taskContent.Result;

                    HttpServerResponse responseObject = JsonConvert.DeserializeObject<HttpServerResponse>(result);
                    if (responseObject == null)
                    {
                        Debug.LogError("Response object is null");
                        UnityUtils.WriteErrorToFile("Response object is null", "CoordinatorError.txt");
                    }

                    for (int i = responseObject.EvalRequestData.evalRangeStart; i < responseObject.EvalRequestData.evalRangeEnd; i++)
                    {
                        FinalPopFitnesses[i] = responseObject.PopFitness[i - responseObject.EvalRequestData.evalRangeStart];
                        FinalBtsNodeCallFrequencies[i] = responseObject.BtsNodeCallFrequencies[i - responseObject.EvalRequestData.evalRangeStart];
                    }
                }
                else
                {
                    Debug.LogError($"Request failed with status code: {response.StatusCode}");
                    UnityUtils.WriteErrorToFile("Response object is null", "CoordinatorError.txt");
                }
            }

            // Return the final population fitnesses and BTS node call frequencies
            HttpServerResponse finalResponse = new HttpServerResponse()
            {
                PopFitness = FinalPopFitnesses,
                BtsNodeCallFrequencies = FinalBtsNodeCallFrequencies,
                EvalRequestData = new EvalRequestData() { evalRangeStart = coordinatorEvalRequestData.evalRangeStart, evalRangeEnd = coordinatorEvalRequestData.evalRangeEnd }
            };

            string responseJson = JsonConvert.SerializeObject(finalResponse);

            byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        yield return null;
    }
}

public class CoordinatorEvalRequestData
{
    public string[] evalEnvInstances { get; set; }
    public int evalRangeStart { get; set; }
    public int evalRangeEnd { get; set; }

    public string evalEnvInstancesToString()
    {
        string evalEnvInstancesString = "";
        foreach (string instance in evalEnvInstances)
        {
            evalEnvInstancesString += instance + ", ";
        }
        return evalEnvInstancesString;
    }
}