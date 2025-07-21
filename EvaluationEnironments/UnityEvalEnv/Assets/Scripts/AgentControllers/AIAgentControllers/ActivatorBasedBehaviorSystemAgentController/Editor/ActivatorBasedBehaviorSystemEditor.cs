using Codice.Client.BaseCommands.BranchExplorer;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public class ActivatorBasedBehaviorSystemEditor : EditorWindow
    {
        ActivatorBasedBehaviorSystemView abisView;
        ActivatorBasedBehaviorSystemAgentController abisController;
        InspectorView inspectorView;
        IMGUIContainer blackboardView;
        ToolbarMenu toolbarMenu;
        TextField abisNameField;
        TextField locationPathField;
        Button createNewTreeButton;
        VisualElement overlay;
        ActivatorBasedBehaviorSystemSettings settings;

        SerializedObject abisObject;
        SerializedProperty blackboardProperty;

        [MenuItem("AI/ActivatorBasedBehaviorSystemEditor")]
        public static void OpenWindow()
        {
            ActivatorBasedBehaviorSystemEditor wnd = GetWindow<ActivatorBasedBehaviorSystemEditor>();
            wnd.titleContent = new GUIContent("ActivatorBasedBehaviorSystemEditor");
            wnd.minSize = new Vector2(800, 600);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is ActivatorBasedBehaviorSystemAgentController)
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
            settings = ActivatorBasedBehaviorSystemSettings.GetOrCreateSettings();

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = settings.activatorBasedBehaviorSystemXml;
            visualTree.CloneTree(root);

            var styleSheet = settings.activatorBasedBehaviorSystemStyle;
            root.styleSheets.Add(styleSheet);

            // Main View
            abisView = root.Q<ActivatorBasedBehaviorSystemView>();
            abisView.OnNodeSelected = OnNodeSelectionChanged;

            // Inspector View
            inspectorView = root.Q<InspectorView>();

            // Blackboard View
            blackboardView = root.Q<IMGUIContainer>();
            blackboardView.onGUIHandler = () => {
                if (abisObject != null && abisObject.targetObject != null)
                {
                    abisObject.Update();
                    EditorGUILayout.PropertyField(blackboardProperty);
                    abisObject.ApplyModifiedProperties();
                }
            };

            // Toolbar assets menu
            toolbarMenu = root.Q<ToolbarMenu>();
            toolbarMenu = root.Q<ToolbarMenu>();
            var abis = LoadAssets<ActivatorBasedBehaviorSystemAgentController>();
            abis.ForEach(tree => {
                toolbarMenu.menu.AppendAction($"{tree.name}", (a) => {
                    Selection.activeObject = tree;
                });
            });
            toolbarMenu.menu.AppendSeparator();
            toolbarMenu.menu.AppendAction("New ABiS...", (a) => CreateNewABiS("NewActivatorBasedBehaviorSystem"));

            // New Abis Dialog
            abisNameField = root.Q<TextField>("ABiSName");
            locationPathField = root.Q<TextField>("LocationPath");
            overlay = root.Q<VisualElement>("Overlay");
            createNewTreeButton = root.Q<Button>("CreateButton");
            createNewTreeButton.clicked += () => CreateNewABiS(abisNameField.value);

            if (abisController == null)
            {
                OnSelectionChange();
            }
            else
            {
                SelectABiS(abisController);
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
                ActivatorBasedBehaviorSystemAgentController abis = Selection.activeObject as ActivatorBasedBehaviorSystemAgentController;
                SelectABiS(abis);
            };
        }

        void SelectABiS(ActivatorBasedBehaviorSystemAgentController newAbis)
        {
            if (abisView == null)
            {
                return;
            }

            if (!newAbis)
            {
                return;
            }

            abisController = newAbis;

            overlay.style.visibility = Visibility.Hidden;

            if (Application.isPlaying)
            {
                abisView.PopulateView(abisController);
            }
            else
            {
                abisView.PopulateView(abisController);
            }


            abisObject = new SerializedObject(abisController);
            blackboardProperty = abisObject.FindProperty("Blackboard");

            EditorApplication.delayCall += () => {
                abisView.FrameAll();
            };
        }

        void OnNodeSelectionChanged(ABiSNodeView node) {
            inspectorView.UpdateSelection(node);
        }

        private void OnInspectorUpdate() {
            abisView?.UpdateNodeStates();
        }

        void CreateNewABiS(string assetName) {
            string path = System.IO.Path.Combine(locationPathField.value, $"{assetName}.asset");
            ActivatorBasedBehaviorSystemAgentController abis = ScriptableObject.CreateInstance<ActivatorBasedBehaviorSystemAgentController>();
            abis.name = abisNameField.ToString();
            AssetDatabase.CreateAsset(abis, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = abis;
            EditorGUIUtility.PingObject(abis);
        }
    }
}