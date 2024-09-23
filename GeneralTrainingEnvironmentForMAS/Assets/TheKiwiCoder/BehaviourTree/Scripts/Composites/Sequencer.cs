using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    public class Sequencer : CompositeNode {
        protected int current;

        protected override void OnStart() {
            current = 0;
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            for (int i = current; i < children.Count; ++i) {
                current = i;
                var child = children[current];

                switch (child.Update()) {
                    case State.Running:
                        return State.Running;
                    case State.Failure:
                        return State.Failure;
                    case State.Success:
                        continue;
                }
            }

            return State.Success;
        }

        public static Node CreateNodeFromBehaviourTreeNodeDef(BehaviourTreeNodeDef behaviourTreeNodeDef, List<BehaviourTreeNodeDef> behaviourTreeNodeDefs, BehaviourTree tree) {
            // Create node
            Sequencer sequencerNode = new Sequencer();

            // Set node properties
            foreach (var child in behaviourTreeNodeDef.children) {
                BehaviourTreeNodeDef childNodeDef = behaviourTreeNodeDefs.Find(def => def.m_fileID == child.fileID);
                sequencerNode.children.Add(Node.CreateNodeTreeFromBehaviourTreeNodeDef(childNodeDef, behaviourTreeNodeDefs, tree));
            }

            tree.nodes.Add(sequencerNode);
            return sequencerNode;
        }
    }
}