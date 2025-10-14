using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public class ABiSNodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Action<ABiSNodeView> OnNodeSelected;
        public ABiSNode node;
        public Port input;
        public Port output;

        public ABiSNodeView(ABiSNode node) : base(AssetDatabase.GetAssetPath(ActivatorBasedBehaviorSystemSettings.GetOrCreateSettings().nodeXml))
        {
            this.node = node;
            this.node.name = node.GetType().Name;
            this.title = node.name.Replace("(Clone)", "").Replace("BTNode", "");
            this.viewDataKey = node.guid;

            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
            SetupDataBinding();

            // Subscribe to connection events
            if (input != null)
            {
                (input as NodePort).OnConnected += HandleConnectionMade;
                (input as NodePort).OnDisconnected += HandleConnectionRemoved;
            }
            if (output != null)
            {
                (output as NodePort).OnConnected += HandleConnectionMade;
                (output as NodePort).OnDisconnected += HandleConnectionRemoved;
            }
        }

        private void CreateInputPorts()
        {
            if(node is RootNode)
            {
                input = null;
            }
            else if (node is ActivatorNode)
            {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            }
            else if (node is DecoratorNode)
            {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            }
            else if (node is ConnectionNode)
            {
                input = new NodePort(Direction.Input, Port.Capacity.Multi);
            }
            else if(node is CompositeNode)
            {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            }
            else if (node is ActionNode)
            {
                input = new NodePort(Direction.Input, Port.Capacity.Multi);
            }

            if (input != null)
            {
                input.portName = "";
                // Position input port at the center of the node
                input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(input);
            }
        }

        private void CreateOutputPorts()
        {
            if(node is RootNode)
            {
                output = new NodePort(Direction.Output, Port.Capacity.Multi);
            }
            if (node is ActivatorNode)
            {
                output = new NodePort(Direction.Output, Port.Capacity.Multi);
            }
            else if (node is DecoratorNode)
            {
                output = new NodePort(Direction.Output, Port.Capacity.Single);
            }
            else if (node is ConnectionNode)
            {
                output = new NodePort(Direction.Output, Port.Capacity.Single);
            }
            else if (node is CompositeNode)
            {
                output = new NodePort(Direction.Output, Port.Capacity.Multi);
            }
            else if (node is ActionNode)
            {
                output = null;
            }

            if (output != null)
            {
                output.portName = "";
                output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(output);
            }
        }

        private void SetupClasses()
        {
            if (node is RootNode)
            {
                AddToClassList("root");
            }
            else if (node is ActivatorNode)
            {
                AddToClassList("activator");
            }
            else if (node is DecoratorNode)
            {
                AddToClassList("decorator");
            }
            else if (node is ConnectionNode)
            {
                AddToClassList("connection");
            }
            else if (node is CompositeNode)
            {
                AddToClassList("composite");
            }
            else if (node is ActionNode)
            {
                AddToClassList("action");
            }

        }

        private void SetupDataBinding()
        {
            Label descriptionLabel = this.Q<Label>("description");
            descriptionLabel.bindingPath = "description";
            descriptionLabel.Bind(new SerializedObject(node));
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(node, "ABiS (Set Position");
            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;
            EditorUtility.SetDirty(node);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if (OnNodeSelected != null)
            {
                OnNodeSelected.Invoke(this);
            }
        }

        public void SortChildren()
        {
            if (node is RootNode root)
            {
                root.Children.Sort(SortByHorizontalPosition);
            }
            else if (node is CompositeNode composite)
            {
                composite.Children.Sort(SortByHorizontalPosition);
            }
            else if (node is ActivatorNode activator)
            {
                activator.Children.Sort(SortByHorizontalPosition);
            }
        }

        private int SortByHorizontalPosition(ABiSNode left, ABiSNode right) {
            return left.position.x < right.position.x ? -1 : 1;
        }

        public void UpdateState()
        {
            RemoveFromClassList("running");
            RemoveFromClassList("failure");
            RemoveFromClassList("success");

            if (Application.isPlaying)
            {
                switch (node.state)
                {
                    case ABiSNode.State.Running:
                        if (node.started)
                        {
                            AddToClassList("running");
                        }
                        break;
                    case ABiSNode.State.Failure:
                        AddToClassList("failure");
                        break;
                    case ABiSNode.State.Success:
                        AddToClassList("success");
                        break;
                }
            }
        }
        private void HandleConnectionMade(NodePort outputPort, NodePort inputPort, Edge edge)
        {
            // Get the source and target nodes
            var outputNodeView = outputPort.node as ABiSNodeView;
            var inputNodeView = inputPort.node as ABiSNodeView;

            if (outputNodeView != null && inputNodeView != null)
            {
                bool isValidConnection = CheckConnectionValidity(outputNodeView.node, inputNodeView.node);
                if (!isValidConnection)
                {
                    var abisGraphView = outputPort.GetFirstAncestorOfType<ActivatorBasedBehaviorSystemView>();
                    if (abisGraphView != null)
                    {
                        // 1. Disconnect the edge from both ports
                        outputPort.Disconnect(edge);
                        inputPort.Disconnect(edge);

                        // 2. Remove the child node from the parent node
                        outputNodeView.node.RemoveChild(inputNodeView.node);

                        // 3. Remove the edge from the GraphView
                        abisGraphView.RemoveElement(edge);
                        DebugSystem.LogWarning("Invalid connection detected and removed.");
                    }
                    else
                    {
                        DebugSystem.LogError("Could not find GraphView to remove invalid edge.");
                    }
                }
            }
        }

        private void HandleConnectionRemoved(NodePort outputPort, NodePort inputPort, Edge edge)
        {
            var outputNodeView = outputPort.node as ABiSNodeView;
            var inputNodeView = inputPort.node as ABiSNodeView;
            if (outputNodeView != null && inputNodeView != null)
            {
                DebugSystem.LogVerbose($"Connection removed between {outputNodeView.title} and {inputNodeView.title}");
            }
        }

        private bool CheckConnectionValidity(ABiSNode sourceNode, ABiSNode targetNode)
        {
            if(sourceNode is RootNode && targetNode is not ActivatorNode)
            {
                return false; // ActivatorNode can only connect to RootNode 
            }
            if (sourceNode is ActivatorNode && targetNode is not ConnectionNode && targetNode is not DecoratorNode)
            {
                return false; // ConnectionNode or DecoratorNode can connect to ActivatorNode
            }
            if (sourceNode is DecoratorNode && targetNode is not ConnectionNode)
            {
                return false; // ConnectionNode can connect to DecoratorNode
            }
            if (sourceNode is ConnectionNode && targetNode is not BehaviorNode)
            {
                return false; // BehaviorNode can only connect to ConnectionNode
            }
            if (sourceNode is BehaviorNode && targetNode is not BehaviorNode)
            {
                return false; // BehaviorNode can only connect to other BehaviorNode
            }

            return true;
        }
    }
}