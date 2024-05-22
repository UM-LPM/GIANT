using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController: MonoBehaviour {

    public static UIController Instance;

    [SerializeField] public TMP_InputField TimeScaleInputField;
    [SerializeField] public TMP_InputField RerunTimesInputField;
    [SerializeField] public TMP_Text FpsText;
    [SerializeField] public TMP_InputField BTSourceInputField;
    [SerializeField] public TMP_Dropdown RndSeedModeDropdown;
    [SerializeField] public TMP_InputField RndSeedInputField;

    [SerializeField] public Toggle RenderToggle;
    [SerializeField] public GameObject[] RenderToggleGameObjectList;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        if(Communicator.Instance != null) {
            if (RndSeedModeDropdown != null)
                RndSeedModeDropdown.value = (int)Communicator.Instance.RandomSeedMode;

            if (RndSeedInputField != null)
                RndSeedInputField.text = Communicator.Instance.InitialSeed.ToString();

            if (RerunTimesInputField != null)
                RerunTimesInputField.text = Communicator.Instance.RerunTimes.ToString();
        }

        if (TimeScaleInputField != null)
            TimeScaleInputField.text = Time.timeScale.ToString();
    }

    private void Update() {
        DisplayFPS();
    }

    public void StopListener() {
        if (Communicator.Instance != null) {
            Communicator.Instance.StopListener();
            Communicator.Instance = null;
        }
    }

    public void BackToMenuButtonClick() {
        StopListener();
        if(MenuManager.Instance != null)
            Destroy(MenuManager.Instance.gameObject);

        UnityEngine.SceneManagement.SceneManager.LoadScene("BaseScene");
        Destroy(Instance.gameObject);
    }

    public void DisplayFPS() {
        if (FpsText != null)
            FpsText.text = "FPS: " + (1.0f / Time.unscaledDeltaTime).ToString("F0");
    }

    public void UpdateTimeScale() {
        if (TimeScaleInputField != null && TimeScaleInputField.text.Length > 0)
            Time.timeScale = int.Parse(TimeScaleInputField.text);
    }

    public void UpdateRender() {
        if (RenderToggle != null) {
            // FInd all objects with renderer and update their visibility
            Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.enabled = RenderToggle.isOn;
            }

            // Update all prefab gameObjects
            foreach (GameObject go in RenderToggleGameObjectList) {
                renderers = go.GetComponentsInChildren<Renderer>();
                renderers.ToList().ForEach(r => r.enabled = RenderToggle.isOn);
                go.GetComponent<Renderer>().enabled = RenderToggle.isOn;
            }
        }
    }

    public void OnRenderToggleValueChanged() {
        UpdateRender();
    }

    public void OnTimeScaleInputEditValueChange() {
        UpdateTimeScale();
    }

    public void RndSeedModeDropdownChange() {
        Communicator.Instance.RandomSeedMode = (RandomSeedMode) RndSeedModeDropdown.value;
        if(Communicator.Instance.RandomSeedMode == RandomSeedMode.RandomAll)
            Communicator.Instance.InitialSeed = new System.Random().Next();
    }

    public void OnRndSeedInputFieldValueChange() {
        if (RndSeedInputField != null && RndSeedInputField.text.Length > 0)
            Communicator.Instance.InitialSeed = int.Parse(RndSeedInputField.text);
    }

    public void OnRerunTimesInputFieldValueChange() {
        if (RerunTimesInputField != null && RerunTimesInputField.text.Length > 0)
            Communicator.Instance.RerunTimes = int.Parse(RerunTimesInputField.text);
    }
}