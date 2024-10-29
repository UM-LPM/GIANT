using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AITechniques.BehaviorTrees
{
	public class Encapsulator : DecoratorNode
	{
		protected override void OnStart()
		{
		}

		protected override void OnStop()
		{
		}

		protected override State OnUpdate()
		{
			switch (child.Update())
			{
				case State.Running:
					return State.Running;
				case State.Failure:
					return State.Failure;
				case State.Success:
					return State.Success;
			}
			return State.Failure;
		}

		public static Node CreateNodeFromBehaviourTreeNodeDef(BehaviourTreeNodeDef behaviourTreeNodeDef, List<BehaviourTreeNodeDef> behaviourTreeNodeDefs, BehaviourTree tree)
		{
			// Create node
			Encapsulator encapsulatedNode = new Encapsulator();

			// Find the child node
			BehaviourTreeNodeDef childNodeDef = behaviourTreeNodeDefs.Find(def => def.m_fileID == behaviourTreeNodeDef.child.fileID);

            // Set node properties
            encapsulatedNode.child = Node.CreateNodeTreeFromBehaviourTreeNodeDef(childNodeDef, behaviourTreeNodeDefs, tree);

			tree.nodes.Add(encapsulatedNode);
			return encapsulatedNode;
		}
	}
}