using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;
using Utils;
using UnitTests;

namespace Configuration
{
    public enum ProblemDomains
    {
        Collector,
        Robostrike,
        Soccer,
        BombClash,
        DodgeBall
    }

    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance;

        [SerializeField] TMP_Dropdown ProblemDropdown;
        [SerializeField] TMP_InputField CoordinatorURIInputField;
        [SerializeField] TMP_InputField CommunicatorURIInputField;

        [SerializeField] ProblemDomains[] ProblemDomains;
        [SerializeField] string baseCoordinatorURI = "http://localhost:4000/";
        [SerializeField] string baseCommunicatorURI = "http://localhost:4444/";

        public string CoordinatorURI { get; private set; }
        public string CommunicatorURI { get; private set; }
        public MainConfiguration MainConfiguration { get; set; }

        private string ConfigPath = Application.dataPath + "/conf.json";

        private async void Awake()
        {
            // Set the target frame rate to 60fps
            Application.targetFrameRate = 60;

            // Singleton pattern
            if (Instance != null)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }

            // Populate the dropdown with the problem domains and set default URL
            ProblemDropdown.ClearOptions();
            ProblemDropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(ProblemDomains))));

            CoordinatorURIInputField.text = baseCoordinatorURI;
            CommunicatorURIInputField.text = baseCommunicatorURI;

            await MqttNetLogger.Connect();

            if(UnitTester.Instance != null && UnitTester.Instance.CurrentTestIndex > -1)
            {
                ConfigPath = UnitTester.Instance.UnitTests[UnitTester.Instance.CurrentTestIndex].ConfigFilePath;
            }

            ReadConfigurationFromFile(ConfigPath);
        }

        public void ReadConfigurationFromFile(string path)
        {
            // Read configuration file
            MainConfiguration = MainConfiguration.Deserialize(path);

            if (MainConfiguration != null && MainConfiguration.AutoStart)
            {
                Play();
            }
        }

        public void Play()
        {
            string problemDomain = ProblemDropdown.options[ProblemDropdown.value].text;
            if (MainConfiguration != null)
            {
                problemDomain = MainConfiguration.ProblemDomain;
            }

            CoordinatorURI = CoordinatorURIInputField.text;
            CommunicatorURI = CommunicatorURIInputField.text;

            if (string.IsNullOrEmpty(problemDomain))
            {
                Debug.LogError("Problem domain is empty");
                return;
            }

            if (string.IsNullOrEmpty(CoordinatorURI) || string.IsNullOrEmpty(CommunicatorURI))
            {
                Debug.LogError("CoordinatorURI or CommunicatorURI is empty");
                return;
            }

            // Load the selected problem domain
            switch (problemDomain)
            {
                case "Collector":
                    UnityEngine.SceneManagement.SceneManager.LoadScene("CollectorBaseScene");
                    break;
                case "Robostrike":
                    UnityEngine.SceneManagement.SceneManager.LoadScene("RobostrikeBaseScene");
                    break;
                case "Soccer":
                    UnityEngine.SceneManagement.SceneManager.LoadScene("SoccerBaseScene");
                    break;
                case "BombClash":
                    UnityEngine.SceneManagement.SceneManager.LoadScene("BombClashBaseScene");
                    break;
                case "BoxTact": 
                    UnityEngine.SceneManagement.SceneManager.LoadScene("BoxTactBaseScene");
                    break;
                case "DodgeBall":
                    UnityEngine.SceneManagement.SceneManager.LoadScene("DodgeBallBaseScene");
                    break;
                default:
                    Debug.LogError("Problem domain not found");
                    break;
            }
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void SaveConfigurationToFile(string path)
        {
            MainConfiguration.Serialize(MainConfiguration, path);
        }

        void OnDestroy()
        {
            MqttNetLogger.Disconnect();
        }
    }
}