using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public class SplitView : TwoPaneSplitView {
        public new class UxmlFactory : UxmlFactory<SplitView, TwoPaneSplitView.UxmlTraits> { }
    }
}