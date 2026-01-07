using UnityEditor;
using UnityEngine.UIElements;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
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
            var graphView = target as ADiSView;
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

            var graphView = target as ADiSView;
            if (graphView == null)
                return;

            if (!CanStopManipulation(evt))
                return;

            ADiSComponentView clickedElement = evt.target as ADiSComponentView;
            if (clickedElement == null)
            {
                var ve = evt.target as VisualElement;
                clickedElement = ve.GetFirstAncestorOfType<ADiSComponentView>();
                if (clickedElement == null)
                    return;
            }

            // TODO: Add find all connected nodes with this node
        }
    }
}