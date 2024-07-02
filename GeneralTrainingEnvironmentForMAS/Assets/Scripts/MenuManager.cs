using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ProblemDomains {
    Collector,
    Robostrike,
    Soccer,
    Bomberman,
    DodgeBall
}

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField] TMP_Dropdown ProblemDropdown;
    [SerializeField] TMP_InputField URIInputField;

    [SerializeField] ProblemDomains[] ProblemDomains;
    [SerializeField] string baseURI = "http://localhost:4444/";

    public string URI { get; private set; }
    public MainConfiguration MainConfiguration { get; set; }

    private void Awake() {
        // Set the target frame rate to 60fps
        Application.targetFrameRate = 60;

        // Singleton pattern
        if (Instance != null) {
            Destroy(this);
        }
        else {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        // Populate the dropdown with the problem domains and set default URL
        ProblemDropdown.ClearOptions();
        ProblemDropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(ProblemDomains))));

        URIInputField.text = baseURI;

        ReadConfigurationFromFile();
    }

    private void ReadConfigurationFromFile()
    {
        // Read configuration file
        string path = Application.dataPath + "/conf.json";
        MainConfiguration = ReadConfigurationFromFile(path);

        if (MainConfiguration != null && MainConfiguration.AutoStart)
        {
            Play();
        }
    }

    public void Play() {
        string problemDomain = ProblemDropdown.options[ProblemDropdown.value].text;
        if(MainConfiguration != null)
        {
            problemDomain = MainConfiguration.ProblemDomain;
        }

        URI = URIInputField.text;
        if (string.IsNullOrEmpty(problemDomain)) {
            Debug.LogError("Problem domain is empty");
            return;
        }

        if (string.IsNullOrEmpty(URI)) {
            Debug.LogError("URL is empty");
            return;
        }

        // Load the selected problem domain
        switch (problemDomain) {
            case "Collector":
                UnityEngine.SceneManagement.SceneManager.LoadScene("CollectorBaseScene");
                break;
            case "Robostrike":
                UnityEngine.SceneManagement.SceneManager.LoadScene("RobostrikeBaseScene");
                break;
            case "Soccer":
                UnityEngine.SceneManagement.SceneManager.LoadScene("SoccerBaseScene");
                break;
            case "Bomberman":
                UnityEngine.SceneManagement.SceneManager.LoadScene("BombermanBaseScene");
                break;
            case "DodgeBall":
                UnityEngine.SceneManagement.SceneManager.LoadScene("DodgeBallBaseScene");
                break;
            default:
                Debug.LogError("Problem domain not found");
                break;
        }
    }

    public void Quit() {
        Application.Quit();
    }

    public MainConfiguration ReadConfigurationFromFile(string path)
    {
        return MainConfiguration.Deserialize(path);
    }

    public void SaveConfigurationToFile(string path)
    {
        MainConfiguration.Serialize(MainConfiguration, path);
    }

}
