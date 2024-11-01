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
using AgentOrganizations;

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
        // TODO Implement
        yield return null;
    }
}