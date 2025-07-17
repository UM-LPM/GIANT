using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    [Serializable]
    [CreateAssetMenu(fileName = "ActivatorBasedBehaviorSystemSettings", menuName = "AgentControllers/AIAgentControllers/ActivatorBasedBehaviorSystem/ActivatorBasedBehaviorSystemSettings")]
    public class ActivatorBasedBehaviorSystemSettings : ScriptableObject
    {
        public VisualTreeAsset activatorBasedBehaviorSystemXml;
        public StyleSheet activatorBasedBehaviorSystemStyle;
        public VisualTreeAsset nodeXml;

        // TODO: Add templates for different node types
        /*public TextAsset scriptTemplateActionNode;
        public TextAsset scriptTemplateCompositeNode;
        public TextAsset scriptTemplateDecoratorNode;
        public TextAsset scriptTemplateConditionNode;*/

        public string newNodeBasePath = "Assets/";

        static ActivatorBasedBehaviorSystemSettings FindSettings()
        {
            var guids = AssetDatabase.FindAssets("t:ActivatorBasedBehaviorSystemSettings");
            if (guids.Length > 1)
            {
                Debug.LogWarning($"Found multiple settings files, using the first.");
            }

            switch (guids.Length)
            {
                case 0:
                    return null;
                default:
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    return AssetDatabase.LoadAssetAtPath<ActivatorBasedBehaviorSystemSettings>(path);
            }
        }

        internal static ActivatorBasedBehaviorSystemSettings GetOrCreateSettings()
        {
            var settings = FindSettings();
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<ActivatorBasedBehaviorSystemSettings>();
                AssetDatabase.CreateAsset(settings, "Assets");
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    // Register a SettingsProvider using UIElements for the drawing framework:
    static class MyCustomSettingsUIElementsRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            var provider = new SettingsProvider("Project/MyCustomUIElementsSettings", SettingsScope.Project)
            {
                label = "ActivatorBasedBehaviorSystem",
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = ActivatorBasedBehaviorSystemSettings.GetSerializedSettings();

                    // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                    // isn't called because the SettingsProvider uses the UIElements drawing framework.
                    var title = new Label()
                    {
                        text = "Activator-Based Behavior System Settings"
                    };
                    title.AddToClassList("title");
                    rootElement.Add(title);

                    var properties = new VisualElement()
                    {
                        style =
                    {
                        flexDirection = FlexDirection.Column
                    }
                    };
                    properties.AddToClassList("property-list");
                    rootElement.Add(properties);

                    properties.Add(new InspectorElement(settings));

                    rootElement.Bind(settings);
                },
            };

            return provider;
        }
    }
}