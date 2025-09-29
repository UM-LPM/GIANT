using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;
using static Unity.VisualScripting.Metadata;

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
                abis.Nodes.Add(n);
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
                Nodes.ForEach(n => {
                    if(!(n is RootNode))
                        n.state = ABiSNode.State.Idle;
                });

                SystemState = RootNode.Update();
            }
            return SystemState;
        }

        public static List<ABiSNode> GetChildren(ABiSNode parent)
        {
            List<ABiSNode> children = new List<ABiSNode>();
            if (parent is RootNode rootNode && rootNode.Children != null)
            {
                children.AddRange(rootNode.Children);
            }
            if (parent is ActivatorNode activator && activator.Children != null)
            {
                children.AddRange(activator.Children);
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
                children.AddRange(composite.Children);
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

        /// <summary>
        /// Traverses the ABiSNode tree starting from the given node and applies the visiter action to each node. 
        /// One node can be visited multiple times if it has multiple parents (e.g. different Activators can lead to one ConnectionNode).
        /// </summary>
        /// <param name="node"></param>
        /// <param name="visiter"></param>
        /// <param name="includeEncapsulatedNodes"></param>
        public static void Traverse(ABiSNode node, System.Action<ABiSNode> visiter)
        {
            if (node == null) return;

            Queue<ABiSNode> queue = new Queue<ABiSNode>();
            HashSet<string> visitedNodeGuids = new HashSet<string>();
            
            queue.Enqueue(node);
            visitedNodeGuids.Add(node.guid);

            while (queue.Count > 0)
            {
                ABiSNode currentNode = queue.Dequeue();
                visiter.Invoke(currentNode);

                var children = GetChildren(currentNode);
                foreach (var child in children)
                {
                    if(!visitedNodeGuids.Contains(child.guid))
                    {
                        queue.Enqueue(child);
                        visitedNodeGuids.Add(child.guid);
                    }
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
                rootNode.Children.Add(child as ActivatorNode);
                EditorUtility.SetDirty(rootNode);
            }

            if (parent is ActivatorNode activator)
            {
                Undo.RecordObject(activator, "Abis (AddChild)");
                activator.Children.Add(child);
                EditorUtility.SetDirty(activator);
            }

            if (parent is DecoratorNode decorator)
            {
                Undo.RecordObject(decorator, "Abis (AddChild)");
                decorator.Child = child as ConnectionNode;
                EditorUtility.SetDirty(decorator);
            }

            if (parent is ConnectionNode connection)
            {
                Undo.RecordObject(connection, "Abis (AddChild)");
                connection.Child = child as BehaviorNode;
                EditorUtility.SetDirty(connection);
            }

            if (parent is CompositeNode composite)
            {
                Undo.RecordObject(composite, "Abis (AddChild)");
                composite.Children.Add(child as BehaviorNode);
                EditorUtility.SetDirty(composite);
            }
        }

        public void RemoveChild(ABiSNode parent, ABiSNode child)
        {
            if (parent is RootNode rootNode)
            {
                Undo.RecordObject(rootNode, "Abis (RemoveChild)");
                rootNode.Children.Remove(child as ActivatorNode);
                EditorUtility.SetDirty(rootNode);
            }
            if (parent is ActivatorNode activator)
            {
                Undo.RecordObject(activator, "Abis (RemoveChild)");
                activator.Children.Remove(child as ActivatorNode);
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
                composite.Children.Remove(child as BehaviorNode);
                EditorUtility.SetDirty(composite);
            }
        }
#endif
#endregion Editor Compatibility
    }
}
