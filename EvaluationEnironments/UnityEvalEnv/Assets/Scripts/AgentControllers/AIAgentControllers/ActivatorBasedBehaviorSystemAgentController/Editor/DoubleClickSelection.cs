using UnityEditor;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public class DoubleClickSelection : MouseManipulator
    {
        double time;
        double doubleClickDuration = 0.3;

        public DoubleClickSelection()
        {
            time = EditorApplication.timeSinceStartup;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {

            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            var graphView = target as ActivatorBasedBehaviorSystemView;
            if (graphView == null)
                return;

            double duration = EditorApplication.timeSinceStartup - time;
            if (duration < doubleClickDuration)
            {
                SelectChildren(evt);
            }

            time = EditorApplication.timeSinceStartup;
        }

        void SelectChildren(MouseDownEvent evt)
        {

            var graphView = target as ActivatorBasedBehaviorSystemView;
            if (graphView == null)
                return;

            if (!CanStopManipulation(evt))
                return;

            ABiSNodeView clickedElement = evt.target as ABiSNodeView;
            if (clickedElement == null)
            {
                var ve = evt.target as VisualElement;
                clickedElement = ve.GetFirstAncestorOfType<ABiSNodeView>();
                if (clickedElement == null)
                    return;
            }

            // Add children to selection so the root element can be moved
            ActivatorBasedBehaviorSystemAgentController.Traverse(clickedElement.node, node => {
                var view = graphView.FindNodeView(node);
                graphView.AddToSelection(view);
            });
        }
    }
}