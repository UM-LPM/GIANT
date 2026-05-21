using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class ADiSEdge: Edge
    {
        public ActivatorConnection activator;

        private Toggle toggle;

        public ADiSEdge(ActivatorConnection model)
        {
            this.activator = model;

            CreateNegationToggle();
        }

        private void CreateNegationToggle()
        {
            toggle = new Toggle
            {
                value = activator.IsNegated,
                tooltip = activator.IsNegated ? "Disable negation" : "Enable negation"
            };

            toggle.RegisterValueChangedCallback(evt =>
            {
                //Undo.RecordObject(activator, "Toggle Edge Negation");
                activator.IsNegated = evt.newValue;
                //EditorUtility.SetDirty(activator);
            });

            // IMPORTANT: edgeControl is where overlays belong
            edgeControl.Add(toggle);

            // Optional styling
            toggle.style.position = Position.Absolute;
            toggle.style.left = 0;
            toggle.style.top = 20;
            toggle.style.width = 18;

            var checkmark = toggle.Q<VisualElement>("unity-checkmark");
            if (checkmark != null)
            {
                // Set white border
                checkmark.style.borderTopColor = Color.white;
                checkmark.style.borderBottomColor = Color.white;
                checkmark.style.borderLeftColor = Color.white;
                checkmark.style.borderRightColor = Color.white;
                checkmark.style.borderTopWidth = 1;
                checkmark.style.borderBottomWidth = 1;
                checkmark.style.borderLeftWidth = 1;
                checkmark.style.borderRightWidth = 1;

                // Optional: rounded corners
                checkmark.style.borderTopLeftRadius = 4;
                checkmark.style.borderTopRightRadius = 4;
                checkmark.style.borderBottomLeftRadius = 4;
                checkmark.style.borderBottomRightRadius = 4;

                // Optional: background color
                checkmark.style.backgroundColor = Color.clear;
            }
        }

        public override bool UpdateEdgeControl()
        {
            bool b = base.UpdateEdgeControl();

            if (edgeControl != null)
            {
                // Color logic
                if (activator.IsNegated)
                {
                    edgeControl.outputColor = new Color(1f, 0.3f, 0.3f);
                    edgeControl.inputColor = new Color(1f, 0.3f, 0.3f);
                }
                else
                {
                    edgeControl.outputColor = new Color(0.3f, 1f, 0.3f);
                    edgeControl.inputColor = new Color(0.3f, 1f, 0.3f);
                }

                tooltip = activator.IsNegated ? "Disable negation" : "Enable negation";

                // Move toggle to midpoint
                UpdateTogglePosition();
            }

            return b;
        }

        private void UpdateTogglePosition()
        {
            if (output == null || input == null || toggle == null || parent == null)
                return;

            // Get center positions in GraphView coordinates
            Vector2 start = output.worldBound.center;
            Vector2 end = input.worldBound.center;

            toggle.style.position = Position.Relative;
            toggle.style.top = Math.Abs(start.y - end.y) / 2;
            toggle.style.alignSelf = Align.Center;
        }
    }
}