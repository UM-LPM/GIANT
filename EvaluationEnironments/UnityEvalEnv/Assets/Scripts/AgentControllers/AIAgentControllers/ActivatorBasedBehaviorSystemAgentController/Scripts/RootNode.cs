using UnityEngine;
using System.Collections.Generic;
using System;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public class RootNode : ABiSNode
    {
        public bool restartOnSuccess = true;
        public bool restartOnFailure = true;

        private State updateState;

        [HideInInspector] public List<ActivatorNode> Children = new List<ActivatorNode>();

        List<ActivatorNode> activatedChildren = new List<ActivatorNode>();
        List<Tuple<ConnectionNode, List<ActivatorNode>>> candidateConnectionNodeTuple = new List<Tuple<ConnectionNode, List<ActivatorNode>>>();
        Tuple<ConnectionNode, List<ActivatorNode>> activatedConnectionTuple;

        protected override void OnStart()
        {
        }
        protected override void OnStop()
        {
            // Cleanup logic if needed
        }
        protected override State OnUpdate()
        {
            // 1. For every activator that has a a activator, we check if the activator is activated.
            activatedChildren.Clear();
            foreach (var activator in Children)
            {
                updateState = activator.Update();
                if (activator.Child != null && (updateState == State.Success || (updateState == State.Failure && activator.Child is Inverter)))
                {
                    activatedChildren.Add(activator);
                }
            }

            // 2. Find all connection nodes that are activated and store them in a list.
            candidateConnectionNodeTuple.Clear();

            foreach (var activator in activatedChildren)
            {
                // Update candidateConnectionNodeTuple
                if (activator.Child is ConnectionNode connectionNodeWithActivators)
                {
                    if (!candidateConnectionNodeTuple.Exists(t => t.Item1 == connectionNodeWithActivators))
                    {
                        candidateConnectionNodeTuple.Add(new Tuple<ConnectionNode, List<ActivatorNode>>(connectionNodeWithActivators, new List<ActivatorNode> { activator }));
                    }
                    else
                    {
                        var tuple = candidateConnectionNodeTuple.Find(t => t.Item1 == connectionNodeWithActivators);
                        tuple.Item2.Add(activator);
                    }
                }
                else if(activator.Child is DecoratorNode decoratorNodeWithActivators)
                {
                    if (decoratorNodeWithActivators.Child is ConnectionNode connectionNode)
                    {
                        if (!candidateConnectionNodeTuple.Exists(t => t.Item1 == connectionNode))
                        {
                            candidateConnectionNodeTuple.Add(new Tuple<ConnectionNode, List<ActivatorNode>>(connectionNode, new List<ActivatorNode> { activator }));
                        }
                        else
                        {
                            var tuple = candidateConnectionNodeTuple.Find(t => t.Item1 == connectionNode);
                            tuple.Item2.Add(activator);
                        }
                    }
                }
            }

            // 3. For every tuple we find the one with the highest priority (weight)
            // If the weights are equal, we choose the one with the most activators (we assume that more activators mean more complex behavior).
            // If even the number of activators is equal, we choose the first one.
            activatedConnectionTuple = candidateConnectionNodeTuple.Count > 0 ? candidateConnectionNodeTuple[0] : null;

            foreach (var tuple in candidateConnectionNodeTuple)
            {
                if (activatedConnectionTuple == null || tuple.Item1.Weight > activatedConnectionTuple.Item1.Weight)
                {
                    activatedConnectionTuple = tuple;
                }
                
                else if (tuple.Item1.Weight == activatedConnectionTuple.Item1.Weight)
                {
                    if (activatedConnectionTuple.Item2.Count < tuple.Item2.Count)
                    {
                        activatedConnectionTuple = tuple;
                    }
                }
            }

            // 4. If we have a connection node with activators, we execute it.
            // return the state of the activated connection node.
            if (activatedConnectionTuple != null)
            {
                switch (activatedConnectionTuple.Item1.Update())
                {
                    case State.Running:
                        break;
                    case State.Failure:
                        if (restartOnFailure)
                        {
                            return State.Running;
                        }
                        else
                        {
                            return State.Failure;
                        }
                    case State.Success:
                        if (restartOnSuccess)
                        {
                            return State.Running;
                        }
                        else
                        {
                            return State.Success;
                        }
                }
            }

            // 5. Return Running if no activators were activated or no connection node was found.
            return State.Running;
        }
        public override ABiSNode Clone()
        {
            RootNode node = Instantiate(this);
            node.Children = new List<ActivatorNode>();
            foreach (var child in Children)
            {
                node.Children.Add(child.Clone() as ActivatorNode);
            }
            return node;
        }

        override public void RemoveChild(ABiSNode child)
        {
            if (Children.Contains(child as ActivatorNode))
            {
                Children.Remove(child as ActivatorNode);
            }
            else
            {
                Debug.LogWarning("Child not found in RootNode's children list.");
            }
        }
    }
}