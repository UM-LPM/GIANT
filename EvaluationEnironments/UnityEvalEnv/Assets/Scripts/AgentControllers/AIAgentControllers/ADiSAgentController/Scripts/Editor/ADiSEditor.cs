using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class ADiSEditor: EditorWindow
    {
        ADiSView adisView;
        ADiSAgentController adisAgentController;
        InspectorView inspectorView;
        ToolbarMenu toolbarMenu;
        TextField adisNameField;
        TextField locationPathField;
        Button createNewADiSButton;
        VisualElement overlay;
        ADiSSettings settings;

        SerializedObject adisObject;

        [MenuItem("AI/ADiSEditor")]
        public static void OpenWindow()
        {
            ADiSEditor wnd = GetWindow<ADiSEditor>();
            wnd.titleContent = new GUIContent("ADiSEditor");
            wnd.minSize = new Vector2(800, 600);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is ADiSAgentController)
            {
                OpenWindow();
                return true;
            }
            return false;
        }

        List<T> LoadAssets<T>() where T : UnityEngine.Object
        {
            string[] assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            List<T> assets = new List<T>();
            foreach (var assetId in assetIds)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetId);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                assets.Add(asset);
            }
            return assets;
        }

        public void CreateGUI()
        {

            settings = ADiSSettings.GetOrCreateSettings();

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualADiS = settings.adisXml;
            visualADiS.CloneTree(root);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = settings.adisStyle;
            root.styleSheets.Add(styleSheet);

            // Main adisView
            adisView = root.Q<ADiSView>();
            adisView.OnNodeSelected = OnNodeSelectionChanged;

            // Inspector View
            inspectorView = root.Q<InspectorView>();

            // Toolbar assets menu
            toolbarMenu = root.Q<ToolbarMenu>();
            var adises = LoadAssets<ADiSAgentController>();
            adises.ForEach(adis => {
                toolbarMenu.menu.AppendAction($"{adis.name}", (a) => {
                    Selection.activeObject = adis;
                });
            });
            toolbarMenu.menu.AppendSeparator();
            toolbarMenu.menu.AppendAction("New ADiS...", (a) => CreateNewADiS("NewADiS"));

            // New ADiS Dialog
            adisNameField = root.Q<TextField>("ADiSName");
            locationPathField = root.Q<TextField>("LocationPath");
            overlay = root.Q<VisualElement>("Overlay");
            createNewADiSButton = root.Q<Button>("CreateButton");
            createNewADiSButton.clicked += () => CreateNewADiS(adisNameField.value);

            if (adisAgentController == null)
            {
                OnSelectionChange();
            }
            else
            {
                SelectADiS(adisAgentController);
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        private void OnSelectionChange()
        {
            EditorApplication.delayCall += () => {
                ADiSAgentController adis = Selection.activeObject as ADiSAgentController;
                SelectADiS(adis);
            };
        }

        void SelectADiS(ADiSAgentController newADiS)
        {

            if (adisView == null)
            {
                return;
            }

            if (!newADiS)
            {
                return;
            }

            this.adisAgentController = newADiS;
            overlay.style.visibility = Visibility.Hidden;

            if (Application.isPlaying)
            {
                adisView.PopulateView(adisAgentController);
            }
            else
            {
                adisView.PopulateView(adisAgentController);
            }


            adisObject = new SerializedObject(adisAgentController);

            EditorApplication.delayCall += () => {
                adisView.FrameAll();
            };
        }

        void OnNodeSelectionChanged(ADiSComponentView node)
        {
            inspectorView.UpdateSelection(node);
        }

        private void OnInspectorUpdate()
        {
            adisView?.UpdateNodeStates();
        }

        void CreateNewADiS(string assetName)
        {
            string path = System.IO.Path.Combine(locationPathField.value, $"{assetName}.asset");
            ADiSAgentController adis = ScriptableObject.CreateInstance<ADiSAgentController>();
            adis.name = adisNameField.ToString();
            AssetDatabase.CreateAsset(adis, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = adis;
            EditorGUIUtility.PingObject(adis);
        }
    }
}