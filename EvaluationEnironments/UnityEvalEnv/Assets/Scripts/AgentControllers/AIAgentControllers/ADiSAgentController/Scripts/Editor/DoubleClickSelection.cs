using System.Collections.Generic;
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
                SelectConnectedNodes(evt);
            }

            time = EditorApplication.timeSinceStartup;
        }

        void SelectConnectedNodes(MouseDownEvent evt)
        {
            if (!CanStopManipulation(evt))
                return;

            if (target is not ADiSView graphView)
                return;

            var clickedView = evt.target as ADiSComponentView
                ?? (evt.target as VisualElement)?.GetFirstAncestorOfType<ADiSComponentView>();

            if (clickedView == null)
                return;

            // ---------------------------------------
            // Build indices (single pass)
            // ---------------------------------------
            var activatorToConnections = new Dictionary<Activator, List<Connection>>();
            var actionToConnections = new Dictionary<Action, List<Connection>>();
            var componentToView = new Dictionary<object, ADiSComponentView>();

            foreach (var node in graphView.nodes)
            {
                if (node is not ADiSComponentView view)
                    continue;

                componentToView[view.component] = view;

                if (view.component is Connection conn)
                {
                    foreach (var ac in conn.ActivatorConnections)
                    {
                        activatorToConnections
                            .GetOrCreate(ac.Activator)
                            .Add(conn);
                    }

                    foreach (var action in conn.Actions)
                    {
                        actionToConnections
                            .GetOrCreate(action)
                            .Add(conn);
                    }
                }
            }

            // ---------------------------------------
            // Selection helpers
            // ---------------------------------------
            void Select(object component)
            {
                if (componentToView.TryGetValue(component, out var view))
                    graphView.AddToSelection(view);
            }

            // ---------------------------------------
            // Dispatch by clicked type
            // ---------------------------------------
            switch (clickedView.component)
            {
                case Activator activator:
                    {
                        if (!activatorToConnections.TryGetValue(activator, out var conns))
                            break;

                        foreach (var conn in conns)
                        {
                            Select(conn);

                            foreach (var action in conn.Actions)
                                Select(action);
                        }
                        break;
                    }

                case Connection conn:
                    {
                        foreach (var ac in conn.ActivatorConnections)
                            Select(ac.Activator);

                        foreach (var action in conn.Actions)
                            Select(action);

                        break;
                    }

                case Action action:
                    {
                        if (!actionToConnections.TryGetValue(action, out var conns))
                            break;

                        foreach (var conn2 in conns)
                        {
                            Select(conn2);

                            foreach (var ac in conn2.ActivatorConnections)
                                Select(ac.Activator);
                        }
                        break;
                    }
            }
        }

    }
}