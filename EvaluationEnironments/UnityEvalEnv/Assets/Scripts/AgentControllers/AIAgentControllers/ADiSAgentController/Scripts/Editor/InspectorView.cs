using UnityEngine.UIElements;
using UnityEditor;

namespace AgentControllers.AIAgentControllers.ADiSAgentController
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }

        Editor editor;

        public InspectorView()
        {

        }

        internal void UpdateSelection(ADiSComponentView nodeView)
        {
            Clear();

            UnityEngine.Object.DestroyImmediate(editor);

            //editor = Editor.CreateEditor(nodeView.component);
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