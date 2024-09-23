using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    public class Inverter : DecoratorNode {
        protected override void OnStart() {
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            switch (child.Update()) {
                case State.Running:
                    return State.Running;
                case State.Failure:
                    return State.Success;
                case State.Success:
                    return State.Failure;
            }
            return State.Failure;
        }

        public static Node CreateNodeFromBehaviourTreeNodeDef(BehaviourTreeNodeDef behaviourTreeNodeDef, List<BehaviourTreeNodeDef> behaviourTreeNodeDefs, BehaviourTree tree) {
            // Create node
            Inverter inverterNode = new Inverter();

            // Find the child node
            BehaviourTreeNodeDef childNodeDef = behaviourTreeNodeDefs.Find(def => def.m_fileID == behaviourTreeNodeDef.child.fileID);

            // Set node properties
            inverterNode.child = Node.CreateNodeTreeFromBehaviourTreeNodeDef(childNodeDef, behaviourTreeNodeDefs, tree);

            tree.nodes.Add(inverterNode);
            return inverterNode;
        }
    }
}