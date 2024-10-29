using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public class Repeat : DecoratorNode {

        public bool restartOnSuccess = true;
        public bool restartOnFailure = false;

        protected override void OnStart() {

        }

        protected override void OnStop() {

        }

        protected override State OnUpdate() {
            switch (child.Update()) {
                case State.Running:
                    break;
                case State.Failure:
                    if (restartOnFailure) {
                        return State.Running;
                    } else {
                        return State.Failure;
                    }
                case State.Success:
                    if (restartOnSuccess) {
                        return State.Running;
                    } else {
                        return State.Success;
                    }
            }
            return State.Running;
        }

        public static Node CreateNodeFromBehaviourTreeNodeDef(BehaviourTreeNodeDef behaviourTreeNodeDef, List<BehaviourTreeNodeDef> behaviourTreeNodeDefs, BehaviorTreeAgentController tree) {
            // Create node
            Repeat repeatNode = new Repeat();

            // Find the child node
            BehaviourTreeNodeDef childNodeDef = behaviourTreeNodeDefs.Find(def => def.m_fileID == behaviourTreeNodeDef.child.fileID);

            // Set node properties
            repeatNode.child = Node.CreateNodeTreeFromBehaviourTreeNodeDef(childNodeDef, behaviourTreeNodeDefs, tree);
            repeatNode.restartOnSuccess = behaviourTreeNodeDef.node_properties["restartOnSuccess"] == "1";
            repeatNode.restartOnFailure = behaviourTreeNodeDef.node_properties["restartOnFailure"] == "1";

            tree.Nodes.Add(repeatNode);
            return repeatNode;
        }
    }

    
}
