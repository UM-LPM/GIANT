using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class ADiSComponentView : UnityEditor.Experimental.GraphView.Node
    {
        public Action<ADiSComponentView> OnNodeSelected;
        public ADiSComponent component;
        public Port input;
        public Port output;

        public ADiSComponentView(ADiSComponent component) : base(AssetDatabase.GetAssetPath(ADiSSettings.GetOrCreateSettings().nodeXml))
        {
            this.component = component;
            this.component.name = component.GetType().Name;
            this.title = component.name.Replace("(Clone)", "").Replace("ADiSComponent", "");
            this.viewDataKey = component.guid;

            style.left = component.position.x;
            style.top = component.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
            SetupDataBinding();
        }

        private void CreateInputPorts()
        {
            if (component is Connection)
            {
                input = new NodePort(Direction.Input, Port.Capacity.Multi);
            }
            else if (component is Action)
            {
                input = new NodePort(Direction.Input, Port.Capacity.Multi);
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
            if (component is Connection)
            {
                output = new NodePort(Direction.Output, Port.Capacity.Multi);
            }
            else if (component is Activator)
            {
                output = new NodePort(Direction.Output, Port.Capacity.Multi);
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
            if (component is Connection)
            {
                AddToClassList("connection");
            }
            else if (component is Activator)
            {
                AddToClassList("activator");
            }
            else if (component is Action)
            {
                AddToClassList("action");
            }
        }

        private void SetupDataBinding()
        {
            Label descriptionLabel = this.Q<Label>("description");
            descriptionLabel.bindingPath = "description";
            descriptionLabel.Bind(new SerializedObject(component));
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(component, "ADiS (Set Position");
            component.position.x = newPos.xMin;
            component.position.y = newPos.yMin;
            EditorUtility.SetDirty(component);
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
            if (component is Connection connection)
            {
                connection.Actions.Sort(SortByHorizontalPosition);
            }
        }

        private int SortByHorizontalPosition(Action left, Action right)
        {
            return left.position.x < right.position.x ? -1 : 1;
        }

        public void UpdateState()
        {
            RemoveFromClassList("active");
            RemoveFromClassList("inactive");
            RemoveFromClassList("executing");

            if (Application.isPlaying)
            {
                if(component is Activator activator)
                {
                    if (activator.IsActive)
                    {
                        AddToClassList("active");
                    }
                    else
                    {
                        AddToClassList("inactive");
                    }
                    return;
                }
                if(component.IsExecuting)
                {
                    AddToClassList("executing");
                }
            }
        }
    }
}