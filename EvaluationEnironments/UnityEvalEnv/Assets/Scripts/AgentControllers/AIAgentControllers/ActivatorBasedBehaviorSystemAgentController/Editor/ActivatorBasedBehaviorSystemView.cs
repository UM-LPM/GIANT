using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public class ActivatorBasedBehaviorSystemView : GraphView
    {
        public Action<ABiSNodeView> OnNodeSelected;
        public new class UxmlFactory : UxmlFactory<ActivatorBasedBehaviorSystemView, GraphView.UxmlTraits> { }
        ActivatorBasedBehaviorSystemAgentController abis;
        ActivatorBasedBehaviorSystemSettings settings;

        public struct ScriptTemplate
        {
            public TextAsset templateFile;
            public string defaultFileName;
            public string subFolder;
        }


        public ActivatorBasedBehaviorSystemView()
        {
            settings = ActivatorBasedBehaviorSystemSettings.GetOrCreateSettings();

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new DoubleClickSelection());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet = settings.activatorBasedBehaviorSystemStyle;
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            PopulateView(abis);
            AssetDatabase.SaveAssets();
        }

        public ABiSNodeView FindNodeView(ABiSNode node)
        {
            return GetNodeByGuid(node.guid) as ABiSNodeView;
        }

        internal void PopulateView(ActivatorBasedBehaviorSystemAgentController abis)
        {
            this.abis = abis;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            // Automatically create RootNode if behaviour Tree is empty
            if (abis.RootNode == null)
            {
                abis.RootNode = abis.CreateNode(typeof(RootNode)) as RootNode;
                EditorUtility.SetDirty(abis);
                AssetDatabase.SaveAssets();
            }

            abis.Nodes.ForEach(n => CreateNodeView(n));

            // Create edges
            abis.Nodes.ForEach(n => {
                var children = ActivatorBasedBehaviorSystemAgentController.GetChildren(n);
                children.ForEach(c => {
                    ABiSNodeView parentView = FindNodeView(n);
                    ABiSNodeView childView = FindNodeView(c);

                    Edge edge = parentView.output.ConnectTo(childView.input); // Create connection
                    AddElement(edge); // Add edge to graph
                });
            });
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
                    ABiSNodeView nodeView = elem as ABiSNodeView;
                    if (nodeView != null)
                    {
                        abis.DeleteNode(nodeView.node);
                    }

                    Edge edge = elem as Edge;
                    if (edge != null)
                    {
                        ABiSNodeView parentView = edge.output.node as ABiSNodeView;
                        ABiSNodeView childView = edge.input.node as ABiSNodeView;
                        abis.RemoveChild(parentView.node, childView.node);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge => {
                    ABiSNodeView parentView = edge.output.node as ABiSNodeView;
                    ABiSNodeView childView = edge.input.node as ABiSNodeView;
                    abis.AddChild(parentView.node, childView.node);
                });
            }

            nodes.ForEach((n) => {
                ABiSNodeView view = n as ABiSNodeView;
                view.SortChildren();
            });

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {

            //base.BuildContextualMenu(evt);

            // TODO: Implement
            // New script functions
            /*evt.menu.AppendAction($"Create Script.../New Action BTNode", (a) => CreateNewScript(scriptFileAssets[0]));
            evt.menu.AppendAction($"Create Script.../New Composite BTNode", (a) => CreateNewScript(scriptFileAssets[1]));
            evt.menu.AppendAction($"Create Script.../New Decorator BTNode", (a) => CreateNewScript(scriptFileAssets[2]));
            evt.menu.AppendAction($"Create Script.../New Condition BTNode", (a) => CreateNewScript(scriptFileAssets[3]));
            evt.menu.AppendSeparator();
            */


            Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);

            // Add Duplicate to the menu (Duplicates currently selected nodes)
            {
                evt.menu.AppendAction("Duplicate", (a) => {
                    // Check if RootNode is selected, if so, do not duplicate
                    if (selection.OfType<ABiSNodeView>().Any(n => n.node is RootNode))
                    {
                        DebugSystem.LogWarning("Cannot duplicate RootNode. Please select other nodes to duplicate.");
                        return;
                    }

                    var selectedNodes = selection.OfType<ABiSNodeView>().ToList();
                    var newNodes = new List<ABiSNodeView>();
                    selectedNodes.ForEach(nodeView => {
                        ABiSNode node = abis.CreateNode(nodeView.node.GetType());
                        node.position = nodeView.node.position + new Vector2(20, 20);
                        CreateNodeView(node);
                        newNodes.Add(FindNodeView(node));
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

            {

                var types = TypeCache.GetTypesDerivedFrom<ActivatorNode>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[Activators]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }

            // Add ConnectionNode to the menu
            evt.menu.AppendAction("[Connection] ConnectionNode", (a) => CreateNode(typeof(ConnectionNode), nodePosition));

            {
                var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[Decorator]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[Composite]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }

            {

                var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[Action]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }
        }

        void CreateNode(System.Type type, Vector2 position)
        {
            ABiSNode node = abis.CreateNode(type);
            node.position = position;
            CreateNodeView(node);
        }

        void CreateNodeView(ABiSNode node)
        {
            ABiSNodeView nodeView = new ABiSNodeView(node);
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }

        public void UpdateNodeStates()
        {
            nodes.ForEach(n => {
                ABiSNodeView view = n as ABiSNodeView;
                view.UpdateState();
            });
        }
    }
}