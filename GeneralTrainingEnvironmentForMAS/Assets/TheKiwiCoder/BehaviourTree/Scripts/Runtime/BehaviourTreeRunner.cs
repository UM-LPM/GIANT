//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
//using UnityEngine;

//namespace TheKiwiCoder {
//    public class BehaviourTreeRunner : MonoBehaviour {

//        // The main behaviour Tree asset
//        public BehaviourTree tree;

//        // Storage container object to hold game object subsystems
//        Context context;

//        [SerializeField] private bool fixedUpdate;


//        float m_KickPower;
//        float m_BallTouch;

//        const float k_Power = 2000f;
//        float m_LateralSpeed;
//        float m_ForwardSpeed;
//        private Rigidbody agentRb;
//        private SoccerSettings m_SoccerSettings;

//        Util util;

//        // Start is called before the first frame update
//        void Start() {
//            context = CreateBehaviourTreeContext();
//            tree = tree.Clone();
//            tree.Bind(context);

//            m_LateralSpeed = 1.0f;
//            m_ForwardSpeed = 1.0f;
//            m_SoccerSettings = FindObjectOfType<SoccerSettings>();
//            agentRb = GetComponent<Rigidbody>();
//            agentRb.maxAngularVelocity = 500;
//            util = context.gameObject.GetComponentInParent<Util>();
//        }

//        // Update is called once per frame
//        void FixedUpdate() {
//            if (fixedUpdate) {
//                UpdateTreeAndMoveAgent();
//            }
//        }

//        public void UpdateTreeAndMoveAgent() {
//            ActionBuffers actionBuffers = new ActionBuffers(null, new int[] { 3, 3, 3 });
//            UpdateTree(actionBuffers);
//            /*var discreteActionsOut = actionBuffers.DiscreteActions;
//            discreteActionsOut[0] = util.rnd.Next(3);
//            discreteActionsOut[1] = util.rnd.Next(3);
//            discreteActionsOut[2] = util.rnd.Next(3);*/
//            MoveAgent(actionBuffers.DiscreteActions);

//        }

//        public void UpdateTree(in ActionBuffers actionsOut) {
//            if (tree) {
//                tree.blackboard.actionsOut = actionsOut;
//                tree.Update();
//            }
//        }

//        Context CreateBehaviourTreeContext() {
//            return Context.CreateFromGameObject(gameObject);
//        }

//        private void OnDrawGizmosSelected() {
//            if (!tree) {
//                return;
//            }

//            BehaviourTree.Traverse(tree.rootNode, (n) => {
//                if (n.drawGizmos) {
//                    n.OnDrawGizmos();
//                }
//            });
//        }

//        public void MoveAgent(ActionSegment<int> act) {
//            var dirToGo = Vector3.zero;
//            var rotateDir = Vector3.zero;

//            m_KickPower = 0f;

//            var forwardAxis = act[0];
//            var rightAxis = act[1];
//            var rotateAxis = act[2];

//            switch (forwardAxis) {
//                case 1:
//                    dirToGo = transform.forward * m_ForwardSpeed;
//                    m_KickPower = 1f;
//                    break;
//                case 2:
//                    dirToGo = transform.forward * -m_ForwardSpeed;
//                    break;
//            }

//            switch (rightAxis) {
//                case 1:
//                    dirToGo = transform.right * m_LateralSpeed;
//                    break;
//                case 2:
//                    dirToGo = transform.right * -m_LateralSpeed;
//                    break;
//            }

//            switch (rotateAxis) {
//                case 1:
//                    rotateDir = transform.up * -1f;
//                    break;
//                case 2:
//                    rotateDir = transform.up * 1f;
//                    break;
//            }

//            transform.Rotate(rotateDir, Time.deltaTime * 100f);
//            agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed,
//                ForceMode.VelocityChange);
//        }

//        void OnCollisionEnter(Collision c) {
//            var force = k_Power * m_KickPower;

//            if (c.gameObject.CompareTag("ball")) {
//                var dir = c.contacts[0].point - transform.position;
//                dir = dir.normalized;
//                c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
//            }
//        }
//    }
//}