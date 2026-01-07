using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Antlr3.Runtime.Tree;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class ADiSView : GraphView
    {
        public Action<ADiSComponentView> OnNodeSelected;
        public new class UxmlFactory : UxmlFactory<ADiSView, UxmlTraits> { }
        ADiSAgentController adis;
        ADiSSettings settings;

        public struct ScriptTemplate
        {
            public TextAsset templateFile;
            public string defaultFileName;
            public string subFolder;
        }

        public ScriptTemplate[] scriptFileAssets = {

            new ScriptTemplate{ templateFile=ADiSSettings.GetOrCreateSettings().scriptTemplateConnectionNode, defaultFileName="NewConnectionNode.cs", subFolder="Connections" },
            new ScriptTemplate{ templateFile=ADiSSettings.GetOrCreateSettings().scriptTemplateActivatorNode, defaultFileName="NewActivatorNode.cs", subFolder="Activators" },
            new ScriptTemplate{ templateFile=ADiSSettings.GetOrCreateSettings().scriptTemplateActionNode, defaultFileName="NewActionsNode.cs", subFolder="Actions" },
        };

        public ADiSView()
        {
            settings = ADiSSettings.GetOrCreateSettings();

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new DoubleClickSelection());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet = settings.adisStyle;
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            PopulateView(adis);
            AssetDatabase.SaveAssets();
        }

        public ADiSComponentView FindComponentView(ADiSComponent component)
        {
            return GetNodeByGuid(component.guid) as ADiSComponentView;
        }

        internal void PopulateView(ADiSAgentController adis)
        {
            this.adis = adis;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            // Creates node abisView
            adis.Components.ForEach(c => CreateComponentView(c));

            adis.Connections.ForEach(c =>
            {
                c.Actions.ForEach(a =>
                {
                    ADiSComponentView parentView = FindComponentView(c);
                    ADiSComponentView childView = FindComponentView(a);

                    Edge edge = parentView.output.ConnectTo(childView.input); // Create connection
                    AddElement(edge); // Add edge to graph
                });

                c.ActivatorConnections.ForEach(ac =>
                {
                    ADiSComponentView parentView = FindComponentView(ac.Activator);
                    ADiSComponentView childView = FindComponentView(c);

                    Edge edge = parentView.output.ConnectTo(childView.input); // Create connection
                    AddElement(edge); // Add edge to graph
                });
            });

            // Create edges
            /*tree.Nodes.ForEach(n => {
                var children = BehaviorTreeAgentController.GetChildren(n);
                children.ForEach(c => {
                    BTNodeView parentView = FindNodeView(n);
                    BTNodeView childView = FindNodeView(c);

                    Edge edge = parentView.output.ConnectTo(childView.input); // Create connection
                    AddElement(edge); // Add edge to graph
                });
            });*/
        }

        // Filter all ports based on the input (start) port, so we are able to only connect input and output
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(elem => {
                    ADiSComponentView nodeView = elem as ADiSComponentView;
                    if (nodeView != null)
                    {
                        adis.DeleteComponent(nodeView.component);
                    }

                    Edge edge = elem as Edge;
                    if (edge != null)
                    {
                        ADiSComponentView parentView = edge.output.node as ADiSComponentView;
                        ADiSComponentView childView = edge.input.node as ADiSComponentView;
                        adis.RemoveComponent(parentView.component, childView.component);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge => {
                    ADiSComponentView parentView = edge.output.node as ADiSComponentView;
                    ADiSComponentView childView = edge.input.node as ADiSComponentView;
                    
                    adis.AddComponent(parentView.component, childView.component);
                });
            }

            nodes.ForEach((n) => {
                ADiSComponentView view = n as ADiSComponentView;
                view.SortChildren();
            });

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {

            //base.BuildContextualMenu(evt);

            // New script functions
            evt.menu.AppendAction($"Create Script.../New Connection Component", (a) => CreateNewScript(scriptFileAssets[0]));
            evt.menu.AppendAction($"Create Script.../New Activator Component", (a) => CreateNewScript(scriptFileAssets[1]));
            evt.menu.AppendAction($"Create Script.../New Action Component", (a) => CreateNewScript(scriptFileAssets[2]));
            evt.menu.AppendSeparator();

            Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);

            // Add Duplicate to the menu (Duplicates currently selected nodes)
            {
                evt.menu.AppendAction("Duplicate", (a) => {
                    var selectedNodes = selection.OfType<ADiSComponentView>().ToList();
                    var newNodes = new List<ADiSComponentView>();
                    selectedNodes.ForEach(nodeView => {
                        ADiSComponent component = adis.CreateComponent(nodeView.component.GetType());
                        component.position = nodeView.component.position + new Vector2(20, 20);
                        CreateComponentView(component);
                        newNodes.Add(FindComponentView(component));
                    });

                    // Deselect all nodes and select the newly created nodes
                    selectedNodes.ForEach(n => RemoveFromSelection(n));

                    // Select the newly created nodes
                    newNodes.ForEach(n => {
                        AddToSelection(n);
                        OnNodeSelected?.Invoke(n);
                    });
                });
            }

            evt.menu.AppendAction($"Connection", (a) => CreateComponent(typeof(Connection), nodePosition));

            {

                var types = TypeCache.GetTypesDerivedFrom<Activator>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[Activator]/{type.Name}", (a) => CreateComponent(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<Action>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[Action]/{type.Name}", (a) => CreateComponent(type, nodePosition));
                }
            }
        }


        void SelectFolder(string path)
        {
            // https://forum.unity.com/threads/selecting-a-folder-in-the-project-via-button-in-editor-window.355357/
            // Check the path has no '/' at the end, if it does remove it,
            // Obviously in this example it doesn't but it might
            // if your getting the path some other way.

            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);

            // Load object
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

            // Select the object in the project folder
            Selection.activeObject = obj;

            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);
        }

        void CreateNewScript(ScriptTemplate template)
        {
            SelectFolder($"{settings.newNodeBasePath}/{template.subFolder}");
            var templatePath = AssetDatabase.GetAssetPath(template.templateFile);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, template.defaultFileName);
        }

        void CreateComponent(System.Type type, Vector2 position)
        {
            ADiSComponent component = adis.CreateComponent(type);
            component.position = position;
            CreateComponentView(component);
        }

        void CreateComponentView(ADiSComponent component)
        {
            ADiSComponentView nodeView = new ADiSComponentView(component);
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }
    }
}