using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheKiwiCoder {
    [CreateAssetMenu(menuName ="BTs/Behaviour Tree Group")]
    public class BehaviourTreeGroup : ScriptableObject {
        public BehaviourTree[] BehaviourTrees;
        public BehaviourTreeGroupBlackboard BehaviourTreeGroupBlackboard;
    }
}
