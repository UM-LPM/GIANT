using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace TheKiwiCoder {
    [CreateAssetMenu(menuName = "BTs/Behaviour Tree")]
    public class BehaviourTree : ScriptableObject {
        public int id;
        public Node rootNode;
        public Node.State treeState = Node.State.Running;
        public List<Node> nodes = new List<Node>();
        public Blackboard blackboard = new Blackboard(); // Blackboard for all nodes
        public BTNode[] bTNodes;

        public Node.State Update() {
            if (rootNode.state == Node.State.Running) {
                treeState = rootNode.Update();
            }
            return treeState;
        }

        /*public Node.State UpdateEnchanced() {
            Queue<BTNode> bTNodeQueue = new Queue<BTNode>();

            bTNodeQueue.Enqueue(bTNodes[0]);

            BTNode currentBTNode;
            while(bTNodeQueue.TryDequeue(out currentBTNode)) {
                switch (currentBTNode.Node) {
                    case RootNode root:
                        break;
                    case Repeat repeat:
                        break;
                    case Sequencer sequencer:
                        break;
                    case Selector selector:
                        break;
                    case ActionNode action:
                        break;
                }
            }
            

            return treeState;
        }*/

        public static List<Node> GetChildren(Node parent) {
            List<Node> children = new List<Node>();

            if (parent is DecoratorNode decorator && decorator.child != null) {
                children.Add(decorator.child);
            }

            if (parent is RootNode rootNode && rootNode.child != null) {
                children.Add(rootNode.child);
            }

            if (parent is CompositeNode composite) {
                return composite.children;
            }

            return children;
        }

        public static void Traverse(Node node, System.Action<Node> visiter) {
            if (node) {
                visiter.Invoke(node);
                var children = GetChildren(node);
                children.ForEach((n) => Traverse(n, visiter));
            }
        }

        public BehaviourTree Clone() {
            BehaviourTree tree = Instantiate(this);
            tree.rootNode = tree.rootNode.Clone();
            tree.nodes = new List<Node>();
            Traverse(tree.rootNode, (n) => {
                tree.nodes.Add(n);
            });

            return tree;
        }

        public void Bind(Context context) {
            Traverse(rootNode, node => {
                node.context = context;
                node.blackboard = blackboard;
                // Here other shared properties of behaviour Tree must be binded
            });
        }

        public Context GetContext() {
            return rootNode.context;
        }

        public static Context CreateBehaviourTreeContext(GameObject agentGameObject) {
            return Context.CreateFromGameObject(agentGameObject);
        }

        public void UpdateTree(in ActionBuffers actionsOut) {
            blackboard.actionsOut = actionsOut;
            Update();
            //UpdateEnchanced();
        }

        /*public void FillBTNodes() {
            bTNodes = new BTNode[nodes.Count];

            Queue<Node> nodeQueue = new Queue<Node>();
            nodeQueue.Enqueue(rootNode);

            int counter = 0;
            Node currentNode;
            while(nodeQueue.TryDequeue(out currentNode)) {
                List<Node> children = currentNode.GetChildren();
                int[] childrenIndexes = new int[children.Count];

                for (int i = 0; i < children.Count; i++) {
                    childrenIndexes[i] = counter + i;
                    nodeQueue.Enqueue(children[i]);
                }
                bTNodes[counter] = new BTNode(currentNode, childrenIndexes);
            }
        }*/



        #region Editor Compatibility
#if UNITY_EDITOR

        public Node CreateNode(System.Type type) {
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();

            Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
            nodes.Add(node);

            if (!Application.isPlaying) {
                AssetDatabase.AddObjectToAsset(node, this);
            }

            Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");

            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(Node node) {
            Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
            nodes.Remove(node);

            //AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);

            AssetDatabase.SaveAssets();
        }

        public void AddChild(Node parent, Node child) {
            if (parent is DecoratorNode decorator) {
                Undo.RecordObject(decorator, "Behaviour Tree (AddChild)");
                decorator.child = child;
                EditorUtility.SetDirty(decorator);
            }

            if (parent is RootNode rootNode) {
                Undo.RecordObject(rootNode, "Behaviour Tree (AddChild)");
                rootNode.child = child;
                EditorUtility.SetDirty(rootNode);
            }

            if (parent is CompositeNode composite) {
                Undo.RecordObject(composite, "Behaviour Tree (AddChild)");
                composite.children.Add(child);
                EditorUtility.SetDirty(composite);
            }
        }

        public void RemoveChild(Node parent, Node child) {
            if (parent is DecoratorNode decorator) {
                Undo.RecordObject(decorator, "Behaviour Tree (RemoveChild)");
                decorator.child = null;
                EditorUtility.SetDirty(decorator);
            }

            if (parent is RootNode rootNode) {
                Undo.RecordObject(rootNode, "Behaviour Tree (RemoveChild)");
                rootNode.child = null;
                EditorUtility.SetDirty(rootNode);
            }

            if (parent is CompositeNode composite) {
                Undo.RecordObject(composite, "Behaviour Tree (RemoveChild)");
                composite.children.Remove(child);
                EditorUtility.SetDirty(composite);
            }
        }
#endif
        #endregion Editor Compatibility
    }
}