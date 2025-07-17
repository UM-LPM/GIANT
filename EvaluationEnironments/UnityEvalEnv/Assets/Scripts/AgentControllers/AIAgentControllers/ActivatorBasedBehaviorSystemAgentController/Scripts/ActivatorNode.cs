using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.ActivatorBasedBehaviorSystemAgentController
{
    public abstract class ActivatorNode : ABiSNode
    {
        public static string[] TargetGameObjects = new string[]
        {
            "Agent",
            "Wall",
            "Obstacle",
            "Object1",
            "Object2",
            "Object3",
            "Object4",
            "Object5",
            "Object6",
            "Object7",
            "Object8",
            "Object9",
            "Object10",
            "Object11",
            "Object12",
            "Object13",
            "Object14",
            "Object15",
            "Object16",
            "Object17",
            "Object18",
            "Object19",
            "Object20",
            "Object21",
            "Object22",
            "Object23",
            "Object24",
            "Object25",
            "Object26",
            "Object27",
            "Object28",
            "Object29",
            "Object30",
            "Object31",
            "Object32",
            "Object33",
            "Object34",
            "Object35",
            "Object36",
            "Object37",
            "Object38",
            "Object39",
            "Object40"
        };

        [HideInInspector] public ABiSNode Child;

        protected abstract bool IsActivated();

        protected override State OnUpdate()
        {
            if (IsActivated())
            {
                return State.Success;
            }
            else
            {
                return State.Failure;
            }
        }

        public override ABiSNode Clone()
        {
            ActivatorNode node = Instantiate(this);
            node.Child = Child.Clone();
            return node;
        }
    }
}
