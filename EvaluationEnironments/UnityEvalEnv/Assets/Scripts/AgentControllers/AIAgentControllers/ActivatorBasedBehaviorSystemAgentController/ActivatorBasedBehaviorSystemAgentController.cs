using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    [Serializable]
    [CreateAssetMenu(fileName = "ActivatorBasedBehaviorSystemAgentController", menuName = "AgentControllers/AIAgentControllers/ActivatorBasedBehaviorSystem/ActivatorBasedBehaviorSystemAgentController")]
    public class ActivatorBasedBehaviorSystemAgentController : AgentController
    {
        
        public ABiSNode RootNode;
        public ABiSNode.State SystemState = ABiSNode.State.Running;
        public List<ABiSNode> Nodes = new List<ABiSNode>();
        public Blackboard Blackboard = new Blackboard();

        public override void GetActions(in ActionBuffer actionsOut)
        {
            UpdateSystem(actionsOut);
        }

        public override AgentController Clone()
        {
            ActivatorBasedBehaviorSystemAgentController abis = Instantiate(this);
            abis.RootNode = abis.RootNode.Clone();
            abis.Nodes = new List<ABiSNode>();

            Traverse(abis.RootNode, (n) => {
                // Since Roots children can be connected to the same node, we need to ensure we don't add duplicates
                // TODO: Implement & Test!
                if (!abis.Nodes.Contains(n))
                {
                    abis.Nodes.Add(n);
                }
            });

            return abis;
        }
        public void UpdateSystem(in ActionBuffer actionsOut)
        {
            Blackboard.actionsOut = actionsOut;
            Update();
        }

        public ABiSNode.State Update()
        {
            if (RootNode.state == ABiSNode.State.Running)
            {
                SystemState = RootNode.Update();
            }
            return SystemState;
        }

        public static List<ABiSNode> GetChildren(ABiSNode parent)
        {
            List<ABiSNode> children = new List<ABiSNode>();
            if (parent is RootNode rootNode && rootNode.Children != null)
            {
                return rootNode.Children;
            }
            if (parent is ActivatorNode activator && activator.Child!= null)
            {
                children.Add(activator.Child);
            }
            if (parent is ConnectionNode connetion && connetion.Child != null)
            {
                children.Add(connetion.Child);
            }
            if (parent is DecoratorNode decorator && decorator.Child != null)
            {
                children.Add(decorator.Child);
            }
            if (parent is CompositeNode composite)
            {
                return composite.Children;
            }
            return children;
        }

        public override void AddAgentControllerToSO(ScriptableObject parent)
        {
#if UNITY_EDITOR
            foreach (var node in Nodes)
            {
                AssetDatabase.AddObjectToAsset(node, parent);
            }
#endif
        }

        public static void Traverse(ABiSNode node, System.Action<ABiSNode> visiter, bool includeEncapsulatedNodes = true)
        {
            if (node == null) return;

            Queue<ABiSNode> queue = new Queue<ABiSNode>();
            queue.Enqueue(node);

            while (queue.Count > 0)
            {
                ABiSNode currentNode = queue.Dequeue();
                visiter.Invoke(currentNode);

                var children = GetChildren(currentNode);
                foreach (var child in children)
                {
                    queue.Enqueue(child);
                }
            }
        }

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

        public static Context CreateActivatorBasedBehaviorSystemContext(GameObject agentGameObject)
        {
            return Context.CreateFromGameObject(agentGameObject);
        }

        #region Editor Compatibility
#if UNITY_EDITOR

        public ABiSNode CreateNode(System.Type type)
        {
            ABiSNode node = ScriptableObject.CreateInstance(type) as ABiSNode;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();

            Undo.RecordObject(this, "ABiS (CreateNode)");
            Nodes.Add(node);

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
            }

            Undo.RegisterCreatedObjectUndo(node, "ABiS (CreateNode)");

            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(ABiSNode node)
        {
            Undo.RecordObject(this, "ABiS (DeleteNode)");
            Nodes.Remove(node);

            //AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);

            AssetDatabase.SaveAssets();
        }

        public void AddChild(ABiSNode parent, ABiSNode child)
        {
            if(parent is RootNode rootNode)
            {
                Undo.RecordObject(rootNode, "Abis (AddChild)");
                rootNode.Children.Add(child);
                EditorUtility.SetDirty(rootNode);
            }

            if (parent is ActivatorNode activator)
            {
                Undo.RecordObject(activator, "Abis (AddChild)");
                activator.Child = child;
                EditorUtility.SetDirty(activator);
            }

            if (parent is DecoratorNode decorator)
            {
                Undo.RecordObject(decorator, "Abis (AddChild)");
                decorator.Child = child;
                EditorUtility.SetDirty(decorator);
            }

            if (parent is ConnectionNode connection)
            {
                Undo.RecordObject(connection, "Abis (AddChild)");
                connection.Child = child;
                EditorUtility.SetDirty(connection);
            }

            if (parent is CompositeNode composite)
            {
                Undo.RecordObject(composite, "Abis (AddChild)");
                composite.Children.Add(child);
                EditorUtility.SetDirty(composite);
            }
        }

        public void RemoveChild(ABiSNode parent, ABiSNode child)
        {
            if (parent is RootNode rootNode)
            {
                Undo.RecordObject(rootNode, "Abis (RemoveChild)");
                rootNode.Children.Remove(child);
                EditorUtility.SetDirty(rootNode);
            }
            if (parent is ActivatorNode activator)
            {
                Undo.RecordObject(activator, "Abis (RemoveChild)");
                activator.Child = null;
                EditorUtility.SetDirty(activator);
            }
            if (parent is DecoratorNode decorator)
            {
                Undo.RecordObject(decorator, "Abis (RemoveChild)");
                decorator.Child = null;
                EditorUtility.SetDirty(decorator);
            }
            if (parent is ConnectionNode connection)
            {
                Undo.RecordObject(connection, "Abis (RemoveChild)");
                connection.Child = null;
                EditorUtility.SetDirty(connection);
            }
            if (parent is CompositeNode composite)
            {
                Undo.RecordObject(composite, "Abis (RemoveChild)");
                composite.Children.Remove(child);
                EditorUtility.SetDirty(composite);
            }
        }
#endif
#endregion Editor Compatibility
    }
}
