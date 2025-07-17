using UnityEditor;
using UnityEngine.UIElements;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }

        Editor editor;

        public InspectorView()
        {

        }

        internal void UpdateSelection(ABiSNodeView nodeView)
        {
            Clear();

            UnityEngine.Object.DestroyImmediate(editor);

            editor = Editor.CreateEditor(nodeView.node);
            IMGUIContainer container = new IMGUIContainer(() => {
                if (editor && editor.target)
                {
                    editor.OnInspectorGUI();
                }
            });
            Add(container);
        }
    }
}