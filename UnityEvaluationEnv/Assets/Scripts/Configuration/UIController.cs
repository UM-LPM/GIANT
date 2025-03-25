using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Problems;
using Base;

namespace Configuration
{
    public class UIController : MonoBehaviour
    {

        public static UIController Instance;

        [SerializeField] public TMP_InputField TimeScaleInputField;
        [SerializeField] public TMP_InputField FixedTimeStepInputField;
        [SerializeField] public TMP_InputField RerunTimesInputField;
        [SerializeField] public TMP_Text FpsText;
        [SerializeField] public TMP_InputField ACSourceInputField; // AC = Agent Controllers
        [SerializeField] public TMP_Dropdown RndSeedModeDropdown;
        [SerializeField] public TMP_InputField RndSeedInputField;

        [SerializeField] public Toggle RenderToggle;
        [SerializeField] public Toggle RenderToggleInGameCanvas;
        [SerializeField] public GameObject[] RenderToggleGameObjectList;
        [SerializeField] public TMP_Text UriText;

        [SerializeField] public Toggle RenderUI;
        [SerializeField] public GameObject ToggleElements;

        private bool isProgrammaticChange = true;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeUI();
        }

        private void Update()
        {
            DisplayFPS();
            DisplayUri();
        }

        void InitializeUI()
        {
            if (Communicator.Instance != null)
            {
                if (RndSeedModeDropdown != null)
                {
                    RndSeedModeDropdown.value = (int)Communicator.Instance.RandomSeedMode;
                    isProgrammaticChange = false;
                }

                //if (ACSourceInputField != null)
                //    ACSourceInputField.text = Communicator.Instance.IndividualsSource;
                if (RndSeedInputField != null)
                    RndSeedInputField.text = Communicator.Instance.InitialSeed.ToString();
                if (RerunTimesInputField != null)
                    RerunTimesInputField.text = Communicator.Instance.RerunTimes.ToString();
                if (TimeScaleInputField != null)
                    TimeScaleInputField.text = Communicator.Instance.TimeScale.ToString();
                if (FixedTimeStepInputField != null)
                    FixedTimeStepInputField.text = Communicator.Instance.FixedTimeStep.ToString();
            }
            if (MenuManager.Instance != null && MenuManager.Instance.MainConfiguration != null)
            {
                if (MenuManager.Instance.MainConfiguration.Render)
                    RenderToggle.isOn = true;
                else
                    RenderToggle.isOn = false;
            }
        }

        public void StopListener()
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

        public void BackToMenuButtonClick()
        {
            StopListener();
            if (MenuManager.Instance != null)
                Destroy(MenuManager.Instance.gameObject);

            UnityEngine.SceneManagement.SceneManager.LoadScene("BaseScene");
            Destroy(Instance.gameObject);
        }

        public void DisplayFPS()
        {
            if (FpsText != null)
                FpsText.text = "FPS: " + (1.0f / Time.unscaledDeltaTime).ToString("F0");
        }

        public void DisplayUri()
        {
            if (UriText != null && Communicator.Instance != null)
                UriText.text = Communicator.Instance.CommunicatorURI;
        }

        public void UpdateTimeScale()
        {
            if (TimeScaleInputField != null && TimeScaleInputField.text.Length > 0)
            {
                Time.timeScale = int.Parse(TimeScaleInputField.text);
                if (Communicator.Instance != null)
                    Communicator.Instance.TimeScale = int.Parse(TimeScaleInputField.text);
            }
        }

        public void UpdateFixedTimeStep()
        {
            if (FixedTimeStepInputField != null && FixedTimeStepInputField.text.Length > 0)
            {
                Time.fixedDeltaTime = float.Parse(FixedTimeStepInputField.text);
                if (Communicator.Instance != null)
                    Communicator.Instance.FixedTimeStep = float.Parse(FixedTimeStepInputField.text);
            }
        }

        public void UpdateRender(Toggle renderToggle, bool updateInGameObjs, bool updateInGameCanvases, bool updatePrefabs)
        {
            if (renderToggle != null)
            {
                Renderer[] renderers;
                if (updateInGameObjs)
                {
                    // Find all objects with renderer and update their visibility
                    renderers = FindObjectsOfType<Renderer>();
                    foreach (var renderer in renderers)
                    {
                        renderer.enabled = renderToggle.isOn;
                    }
                }

                if (updateInGameCanvases)
                {
                    // Find all objects with canvas and update their visibility
                    CanvasComponent[] canvasComponents = FindObjectsOfType<CanvasComponent>();
                    foreach (var canvasComponent in canvasComponents)
                    {
                        canvasComponent.gameObject.GetComponent<Canvas>().enabled = renderToggle.isOn;
                    }
                }

                if (updatePrefabs)
                {
                    // Update all prefab gameObjects
                    foreach (GameObject go in RenderToggleGameObjectList)
                    {
                        renderers = go.GetComponentsInChildren<Renderer>();
                        renderers.ToList().ForEach(r => r.enabled = renderToggle.isOn);

                        // Update all canvas renderers
                        Canvas[] canvases = go.GetComponentsInChildren<Canvas>();
                        canvases.ToList().ForEach(r => r.enabled = renderToggle.isOn);

                        Renderer goRenderer = go.GetComponent<Renderer>();
                        if (goRenderer != null)
                            go.GetComponent<Renderer>().enabled = renderToggle.isOn;
                    }
                }
            }
        }

        public void OnUpdateRenderUIToggle()
        {
            ToggleElements.SetActive(RenderUI.isOn);
        }

        public void OnRenderToggleValueChanged()
        {
            UpdateRender(RenderToggle, true, true, true);
        }

        public void OnRenderToggleInGameCanvasValueChanged()
        {
            UpdateRender(RenderToggleInGameCanvas, false, true, false);
        }

        public void OnTimeScaleInputEditValueChange()
        {
            UpdateTimeScale();
        }

        public void OnFixedTimeStepInputEditValueChange()
        {
            UpdateFixedTimeStep();
        }

        public void RndSeedModeDropdownChange()
        {
            if (!isProgrammaticChange)
            {
                Communicator.Instance.RandomSeedMode = (RandomSeedMode)RndSeedModeDropdown.value;
                if (Communicator.Instance.RandomSeedMode == RandomSeedMode.RandomAll)
                {
                    Communicator.Instance.InitialSeed = new System.Random().Next();
                    if (RndSeedInputField != null)
                        RndSeedInputField.text = Communicator.Instance.InitialSeed.ToString();
                }
            }
        }

        public void OnRndSeedInputFieldValueChange()
        {
            if (RndSeedInputField != null && RndSeedInputField.text.Length > 0)
                Communicator.Instance.InitialSeed = int.Parse(RndSeedInputField.text);
        }

        public void OnRerunTimesInputFieldValueChange()
        {
            if (RerunTimesInputField != null && RerunTimesInputField.text.Length > 0)
                Communicator.Instance.RerunTimes = int.Parse(RerunTimesInputField.text);
        }
    }
}