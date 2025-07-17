using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;
using System.IO;
using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public class BehaviourTreeView : GraphView {

        public Action<BTNodeView> OnNodeSelected;
        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, GraphView.UxmlTraits> { }
        BehaviorTreeAgentController tree;
        BehaviourTreeSettings settings;

        public struct ScriptTemplate {
            public TextAsset templateFile;
            public string defaultFileName;
            public string subFolder;
        }

        public ScriptTemplate[] scriptFileAssets = {
            
            new ScriptTemplate{ templateFile=BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateActionNode, defaultFileName="NewActionNode.cs", subFolder="Actions" },
            new ScriptTemplate{ templateFile=BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateCompositeNode, defaultFileName="NewCompositeNode.cs", subFolder="Composites" },
            new ScriptTemplate{ templateFile=BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateDecoratorNode, defaultFileName="NewDecoratorNode.cs", subFolder="Decorators" },
            new ScriptTemplate{ templateFile=BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateConditionNode, defaultFileName="NewConditionNode.cs", subFolder="Conditions" },
        };

        public BehaviourTreeView() {
            settings = BehaviourTreeSettings.GetOrCreateSettings();

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new DoubleClickSelection());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet = settings.behaviourTreeStyle;
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo() {
            PopulateView(tree);
            AssetDatabase.SaveAssets();
        }

        public BTNodeView FindNodeView(BTNode node) {
            return GetNodeByGuid(node.guid) as BTNodeView;
        }

        internal void PopulateView(BehaviorTreeAgentController tree) {
            this.tree = tree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            // Automatically create RootNode if behaviour Tree is empty
            if (tree.RootNode == null) {
                tree.RootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
                EditorUtility.SetDirty(tree);
                AssetDatabase.SaveAssets();
            }

            // Creates node abisView
            tree.Nodes.ForEach(n => CreateNodeView(n));

            // Create edges
            tree.Nodes.ForEach(n => {
                var children = BehaviorTreeAgentController.GetChildren(n);
                children.ForEach(c => {
                    BTNodeView parentView = FindNodeView(n);
                    BTNodeView childView = FindNodeView(c);

                    Edge edge = parentView.output.ConnectTo(childView.input); // Create connection
                    AddElement(edge); // Add edge to graph
                });
            });
        }

        // Filter all ports based on the input (start) port, so we are able to only connect input and output
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            if (graphViewChange.elementsToRemove != null) {
                graphViewChange.elementsToRemove.ForEach(elem => {
                    BTNodeView nodeView = elem as BTNodeView;
                    if (nodeView != null) {
                        tree.DeleteNode(nodeView.node);
                    }

                    Edge edge = elem as Edge;
                    if (edge != null) {
                        BTNodeView parentView = edge.output.node as BTNodeView;
                        BTNodeView childView = edge.input.node as BTNodeView;
                        tree.RemoveChild(parentView.node, childView.node);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null) {
                graphViewChange.edgesToCreate.ForEach(edge => {
                    BTNodeView parentView = edge.output.node as BTNodeView;
                    BTNodeView childView = edge.input.node as BTNodeView;
                    tree.AddChild(parentView.node, childView.node);
                });
            }

            nodes.ForEach((n) => {
                BTNodeView view = n as BTNodeView;
                view.SortChildren();
            });

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {

            //base.BuildContextualMenu(evt);

            // New script functions
            evt.menu.AppendAction($"Create Script.../New Action BTNode", (a) => CreateNewScript(scriptFileAssets[0]));
            evt.menu.AppendAction($"Create Script.../New Composite BTNode", (a) => CreateNewScript(scriptFileAssets[1]));
            evt.menu.AppendAction($"Create Script.../New Decorator BTNode", (a) => CreateNewScript(scriptFileAssets[2]));
            evt.menu.AppendAction($"Create Script.../New Condition BTNode", (a) => CreateNewScript(scriptFileAssets[3]));
            evt.menu.AppendSeparator();

            Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);

            if(selection.Count == 1 && selection[0] is BTNodeView)
            {
                evt.menu.AppendAction($"Duplicate node", (a) => DuplicateNode(nodePosition));
            }

            {

                var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
                foreach (var type in types) {
                    evt.menu.AppendAction($"[Action]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
                foreach (var type in types) {
                    evt.menu.AppendAction($"[Composite]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
                foreach (var type in types) {
                    evt.menu.AppendAction($"[Decorator]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<ConditionNode>();
                foreach (var type in types) {
                    evt.menu.AppendAction($"[Condition]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }
        }

        void SelectFolder(string path) {
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

        void CreateNewScript(ScriptTemplate template) {
            SelectFolder($"{settings.newNodeBasePath}/{template.subFolder}");
            var templatePath = AssetDatabase.GetAssetPath(template.templateFile);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, template.defaultFileName);
        }

        void CreateNode(System.Type type, Vector2 position) {
            BTNode node = tree.CreateNode(type);
            node.position = position;
            CreateNodeView(node);
        }

        void CreateNodeView(BTNode node) {
            BTNodeView nodeView = new BTNodeView(node);
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }


        // TODO upgrade for multiple Nodes at the same time + edges
        void DuplicateNode(Vector2 position) {
            BTNodeView nodeView = selection[0] as BTNodeView;

            BTNode node = nodeView.node.Clone();
            node.guid = GUID.Generate().ToString();
            node.position = position;

            if(node is CompositeNode) {
                CompositeNode compositeNode = (CompositeNode)node;
                compositeNode.children = new List<BTNode>();
            }
            else if (node is DecoratorNode) {
                DecoratorNode decoratorNode = (DecoratorNode)node;
                decoratorNode.child = null;
            }
            else if (node is RootNode) {
                RootNode rootNode = (RootNode)node;
                rootNode.child = null;
            }

            CreateNodeView(node);
        }

        public void UpdateNodeStates() {
            nodes.ForEach(n => {
                BTNodeView view = n as BTNodeView;
                view.UpdateState();
            });
        }
    }
}