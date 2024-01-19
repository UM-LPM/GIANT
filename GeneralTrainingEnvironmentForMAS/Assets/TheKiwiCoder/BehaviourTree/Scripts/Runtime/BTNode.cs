using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheKiwiCoder;
using UnityEngine;

public class BTNode : MonoBehaviour {
    public Node Node;
    public int[] ChildrenIndxes;

    public BTNode(Node node, int[] childrenIndxs) {
        Node = node;
        ChildrenIndxes = childrenIndxs;
    }
}