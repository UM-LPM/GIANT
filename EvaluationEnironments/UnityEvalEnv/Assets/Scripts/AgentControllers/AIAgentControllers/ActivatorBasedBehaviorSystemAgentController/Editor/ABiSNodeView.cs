using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            }

            if (input != null)
            {
                input.portName = "";
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
                output = new NodePort(Direction.Output, Port.Capacity.Single);
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
                output.style.flexDirection = FlexDirection.Column;
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
            if (node is CompositeNode composite)
            {
                composite.Children.Sort(SortByHorizontalPosition);
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
    }
}