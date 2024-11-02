using AgentControllers.AIAgentControllers.BehaviorTreeAgentController;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController
{
    [Serializable]
    [CreateAssetMenu(fileName = "BehaviorTreeAgentController", menuName = "AgentControllers/AIAgentControllers/BehaviorTreeAgentController")]
    public class BehaviorTreeAgentController : AIAgentController
    {
        public Node RootNode;
        public Node.State TreeState = Node.State.Running;
        public List<Node> Nodes = new List<Node>();
        public Blackboard Blackboard = new Blackboard(); // Blackboard for all Nodes

        public override void GetActions(in ActionBuffer actionsOut)
        {
            UpdateTree(actionsOut);
        }

        public override AgentController Clone()
        {
            BehaviorTreeAgentController tree = Instantiate(this);
            tree.RootNode = tree.RootNode.Clone();
            tree.Nodes = new List<Node>();
            Traverse(tree.RootNode, (n) => {
                tree.Nodes.Add(n);
            });

            return tree;
        }

        public Node.State Update()
        {
            if (RootNode.state == Node.State.Running)
            {
                TreeState = RootNode.Update();
            }
            return TreeState;
        }
        public static List<Node> GetChildren(Node parent)
        {
            List<Node> children = new List<Node>();

            if (parent is DecoratorNode decorator && decorator.child != null)
            {
                children.Add(decorator.child);
            }

            if (parent is RootNode rootNode && rootNode.child != null)
            {
                children.Add(rootNode.child);
            }

            if (parent is CompositeNode composite)
            {
                return composite.children;
            }

            return children;
        }

        public static void Traverse(Node node, System.Action<Node> visiter, bool includeEncapsulatedNodes = true)
        {
            if (node == null) return;

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

        /*public BehaviorTreeAgentController Clone()
        {
            BehaviorTreeAgentController tree = Instantiate(this);
            tree.RootNode = tree.RootNode.Clone();
            tree.Nodes = new List<Node>();
            Traverse(tree.RootNode, (n) => {
                tree.Nodes.Add(n);
            });

            return tree;
        }*/

        public void Bind(Context context)
        {
            Traverse(RootNode, node => {
                node.context = context;
                node.blackboard = Blackboard;
                // Here other shared properties of behaviour Tree must be binded
            });
        }

        public Context GetContext()
        {
            return RootNode.context;
        }

        public static Context CreateBehaviourTreeContext(GameObject agentGameObject)
        {
            return Context.CreateFromGameObject(agentGameObject);
        }

        public void UpdateTree(in ActionBuffer actionsOut)
        {
            Blackboard.actionsOut = actionsOut;
            Update();
        }

        public void InitNodeCallFrequencyCounter()
        {
            Traverse(RootNode, (node) =>
            {
                node.callFrequencyCount = 0;
            });
        }

        public int[] GetNodeCallFrequencies(bool includeEncapsulatedNodes)
        {
            List<int> callFrequencies = new List<int>();
            Traverse(RootNode, (node) =>
            {
                callFrequencies.Add(node.callFrequencyCount);
            }, includeEncapsulatedNodes);

            // Remove first two elements (root and first child (Repeat))
            callFrequencies.RemoveRange(0, 2);

            return callFrequencies.ToArray();
        }

        #region Editor Compatibility
#if UNITY_EDITOR

        public Node CreateNode(System.Type type)
        {
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();

            Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
            Nodes.Add(node);

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
            }

            Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");

            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(Node node)
        {
            Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
            Nodes.Remove(node);

            //AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);

            AssetDatabase.SaveAssets();
        }

        public void AddChild(Node parent, Node child)
        {
            if (parent is DecoratorNode decorator)
            {
                Undo.RecordObject(decorator, "Behaviour Tree (AddChild)");
                decorator.child = child;
                EditorUtility.SetDirty(decorator);
            }

            if (parent is RootNode rootNode)
            {
                Undo.RecordObject(rootNode, "Behaviour Tree (AddChild)");
                rootNode.child = child;
                EditorUtility.SetDirty(rootNode);
            }

            if (parent is CompositeNode composite)
            {
                Undo.RecordObject(composite, "Behaviour Tree (AddChild)");
                composite.children.Add(child);
                EditorUtility.SetDirty(composite);
            }
        }

        public void RemoveChild(Node parent, Node child)
        {
            if (parent is DecoratorNode decorator)
            {
                Undo.RecordObject(decorator, "Behaviour Tree (RemoveChild)");
                decorator.child = null;
                EditorUtility.SetDirty(decorator);
            }

            if (parent is RootNode rootNode)
            {
                Undo.RecordObject(rootNode, "Behaviour Tree (RemoveChild)");
                rootNode.child = null;
                EditorUtility.SetDirty(rootNode);
            }

            if (parent is CompositeNode composite)
            {
                Undo.RecordObject(composite, "Behaviour Tree (RemoveChild)");
                composite.children.Remove(child);
                EditorUtility.SetDirty(composite);
            }
        }
#endif
        #endregion Editor Compatibility
    }
}