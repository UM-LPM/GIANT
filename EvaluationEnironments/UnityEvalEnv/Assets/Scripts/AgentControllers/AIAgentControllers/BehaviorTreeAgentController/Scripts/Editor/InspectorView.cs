using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public class InspectorView : VisualElement {
        public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }

        Editor editor;

        public InspectorView() {

        }

        internal void UpdateSelection(BTNodeView nodeView) {
            Clear();

            UnityEngine.Object.DestroyImmediate(editor);

            editor = Editor.CreateEditor(nodeView.node);
            IMGUIContainer container = new IMGUIContainer(() => {
                if (editor && editor.target) {
                    editor.OnInspectorGUI();
                }
            });
            Add(container);
        }
    }
}