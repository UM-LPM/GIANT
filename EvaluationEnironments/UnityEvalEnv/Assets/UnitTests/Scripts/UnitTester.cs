using Base;
using Configuration;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Utils;

namespace UnitTests {
    public class UnitTester : MonoBehaviour
    {
        public static UnitTester Instance;

        [SerializeField] public UnitTest[] UnitTests;

        [HideInInspector] public int CurrentTestIndex = -1;
        [HideInInspector] public bool UnitTestRunning = false;
        [HideInInspector] public UnitTestResult[] TestResults;

        private bool UnitTestResultsOutputed = false;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
                Instance.TestResults = new UnitTestResult[UnitTests.Length];
                DontDestroyOnLoad(this);
            }

            // Subscribe to events
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDestroy()
        {
            // Clean up subscriptions if this object ever gets destroyed
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        void Update() {
            if (!Instance.UnitTestRunning && Instance.CurrentTestIndex < (UnitTests.Length - 1))
            {
                Instance.CurrentTestIndex += 1;
                var unitTest = UnitTests[Instance.CurrentTestIndex];
                RunTest(unitTest);
            }

            if(!Instance.UnitTestResultsOutputed && Instance.CurrentTestIndex == UnitTests.Length - 1 && !Instance.UnitTestRunning)
            {
                DebugSystem.Log("All UnitTests have been executed.");
                for (int i = 0; i < Instance.TestResults.Length; i++)
                {
                    if(Instance.TestResults[i] != null)
                    {
                        if (Instance.TestResults[i].Passed)
                        {
                            DebugSystem.LogSuccess($"{Instance.TestResults[i].TestName} Passed");
                        }
                        else
                        {
                            DebugSystem.LogError($"{Instance.TestResults[i].TestName} Failed. Error: {Instance.TestResults[i].ErrorMessage}");
                        }
                    }
                    else
                    {
                        DebugSystem.LogError($"{UnitTests[i].Name} did not run.");
                    }
                }
                Instance.UnitTestResultsOutputed = true;
            }
        }

        public void RunTest(UnitTest unitTest)
        {
            DebugSystem.Log($"Starting {Instance.CurrentTestIndex + 1}/{UnitTests.Length}: {unitTest.Name}");
            Instance.UnitTestRunning = true;

            string validationError = IsTestValid(unitTest);
            if (validationError != null)
            {
                Instance.TestResults[Instance.CurrentTestIndex] = new UnitTestResult(unitTest.Name, false, validationError);
                Instance.UnitTestRunning = false;
                DebugSystem.LogError(validationError);
                return;
            }
            else
            {
                SceneManager.LoadScene("BaseScene");
            }
        }

        public string IsTestValid(UnitTest unitTest)
        {
            if (string.IsNullOrEmpty(unitTest.Name))
            {
                return $"UnitTest Error: Name is required.";
            }
            if (string.IsNullOrEmpty(unitTest.ConfigFilePath))
            {
                return $"UnitTest Error: ConfigFilePath is required for test '{unitTest.Name}'.";
            }
            if (unitTest.Individuals == null || unitTest.Individuals.Length == 0)
            {
                return $"UnitTest Error: At least one Individual is required for test '{unitTest.Name}'.";
            }
            if (string.IsNullOrEmpty(unitTest.ExpectedOutputFilePath))
            {
                return $"UnitTest Error: ExpectedOutputFilePath is required for test '{unitTest.Name}'.";
            }
            return null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Check if scene.name corresponds to pattern "ProblemName + BaseScene"(e.g., "RoboStrikeBaseScene" or "SoccerBaseScene") but is not exactly "BaseScene"
            if (scene.name != "BaseScene" && scene.name.EndsWith("BaseScene"))
            {
                DebugSystem.Log("Scene Loaded: " + scene.name);

                // Send request to the Coordinator to evaluate the individuals
                SendEvaluationRequest();
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            // Check if scene.name corresponds to pattern "ProblemName + BaseScene"(e.g., "RoboStrikeBaseScene" or "SoccerBaseScene") but is not exactly "BaseScene"
            if (scene.name != "BaseScene" && scene.name.EndsWith("BaseScene"))
            {
                DebugSystem.Log("Scene Unloaded: " + scene.name);
                // Scene is unloaded
            }
        }

        public void SendEvaluationRequest()
        {
            // Send GET request to the Coordinator to evaluate the individuals
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                if (Coordinator.Instance != null && Coordinator.Instance.CoordinatorURI != null)
                {
                    DebugSystem.Log($"Sending evaluation request to Coordinator on URI: {Coordinator.Instance.CoordinatorURI}");

                    // Send post request to Coordinator.Instance.CoordinatorURI
                    string json = JsonConvert.SerializeObject(new
                        CoordinatorEvalRequestData
                    {
                        EvalEnvInstances = new string[] { MenuManager.Instance.MainConfiguration.StartCommunicatorURI },
                        EvalRangeStart = 0,
                        EvalRangeEnd = UnitTests[Instance.CurrentTestIndex].Individuals.Length,
                    }, MainConfiguration.JSON_SERIALIZATION_SETTINGS);

                    StartCoroutine(SendPostRequest(Coordinator.Instance.CoordinatorURI, json));

                }
            }
        }

        IEnumerator SendPostRequest(string url, string jsonBody)
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                DebugSystem.Log($"Evaluation for UnitTest: { UnitTests[Instance.CurrentTestIndex].Name } completed successfully.");

                string expectedOutputJson = System.IO.File.ReadAllText(UnitTests[Instance.CurrentTestIndex].ExpectedOutputFilePath);

                // Comparrison type 1 (Compare strings)
                // Remove guids from both JSON strings before comparison
                string guidPattern = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
                string outputWithoutGuids = System.Text.RegularExpressions.Regex.Replace(request.downloadHandler.text, guidPattern, "");
                string expectedOutputWithoutGuids = System.Text.RegularExpressions.Regex.Replace(expectedOutputJson, guidPattern, "");

                // Format both JSON strings to make sure they are comparable
                string output = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<CoordinatorEvaluationResult>(outputWithoutGuids), MainConfiguration.JSON_SERIALIZATION_SETTINGS);
                string expectedOutput = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<CoordinatorEvaluationResult>(expectedOutputWithoutGuids), MainConfiguration.JSON_SERIALIZATION_SETTINGS);

                if(output == expectedOutput)
                {
                    Instance.TestResults[Instance.CurrentTestIndex] = new UnitTestResult(UnitTests[Instance.CurrentTestIndex].Name, true);
                    DebugSystem.LogSuccess($"UnitTest { UnitTests[Instance.CurrentTestIndex].Name } Passed.");
                }
                else
                {
                    Instance.TestResults[Instance.CurrentTestIndex] = new UnitTestResult(UnitTests[Instance.CurrentTestIndex].Name, false, "Output does not match expected output.");
                    DebugSystem.LogError($"UnitTest { UnitTests[Instance.CurrentTestIndex].Name } Failed. Output does not match expected output.");
                }

                FinishUnitTest();
            }
            else
            {
                Instance.TestResults[CurrentTestIndex] = new UnitTestResult(UnitTests[CurrentTestIndex].Name, false, request.error);
                DebugSystem.LogError($"Error occured during the UnitTest: { UnitTests[CurrentTestIndex].Name }. Error: { request.error }");
            }
        }

        public void StopListeners()
        {
            if (Communicator.Instance != null)
            {
                Communicator.Instance.StopListener();
                Communicator.Instance = null;
            }

            if (Coordinator.Instance != null)
            {
                Coordinator.Instance.StopListener();
                Coordinator.Instance = null;
            }
        }

        public void FinishUnitTest()
        {
            StopListeners();
            if (MenuManager.Instance != null)
                Destroy(MenuManager.Instance.gameObject);

            Destroy(UIController.Instance.gameObject);

            UnityEngine.SceneManagement.SceneManager.LoadScene("UnitTesterScene");
            Instance.UnitTestRunning = false;
        }
    }

    public class UnitTestResult
    {
        public string TestName;
        public bool Passed;
        public string ErrorMessage;

        public UnitTestResult(string testName, bool passed, string errorMessage = null)
        {
            TestName = testName;
            Passed = passed;
            ErrorMessage = errorMessage;
        }
    }
}