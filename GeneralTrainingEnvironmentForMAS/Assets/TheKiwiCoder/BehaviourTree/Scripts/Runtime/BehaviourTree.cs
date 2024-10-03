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

        public static void Traverse(Node node, System.Action<Node> visiter, bool includeEncapsulatedNodes = true) {
            if(node == null) return;

            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(node);

            while (queue.Count > 0)
            {
                Node currentNode = queue.Dequeue();
                visiter.Invoke(currentNode);

                // If the node is an encapsulator, we don't want to traverse its children // TODO Control this with a parameter
                if (currentNode is Encapsulator && !includeEncapsulatedNodes)
                {
                    continue;
                }

                var children = GetChildren(currentNode);
                foreach (var child in children)
                {
                    queue.Enqueue(child);
                }
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

        public void UpdateTree(in ActionBuffer actionsOut) {
            blackboard.actionsOut = actionsOut;
            Update();
        }

        public void InitNodeCallFrequencyCounter()
        {
            Traverse(rootNode, (node) =>
            {
                node.callFrequencyCount = 0;
            });
        }

        public int[] GetNodeCallFrequencies(bool includeEncapsulatedNodes)
        {
            List<int> callFrequencies = new List<int>();
            Traverse(rootNode, (node) =>
            {
                callFrequencies.Add(node.callFrequencyCount);
            }, includeEncapsulatedNodes);

            // Remove first two elements (root and first child (Repeat))
            callFrequencies.RemoveRange(0, 2);

            return callFrequencies.ToArray();
        }

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