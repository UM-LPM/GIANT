using System.Collections.Generic;
using UnityEngine;

namespace AgentControllers.AIAgentControllers.BehaviorTreeAgentController {
    public abstract class Node : ScriptableObject {
        public enum State {
            Running,
            Failure,
            Success
        }

        [HideInInspector] public State state = State.Running;
        [HideInInspector] public bool started = false;
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;
        [HideInInspector] public Context context;
        [HideInInspector] public Blackboard blackboard;
        public int callFrequencyCount;
        [TextArea] public string description;
        public bool drawGizmos = false;

        public State Update() {

            if (!started) {
                OnStart();
                started = true;
            }

            state = OnUpdate();

            if (state != State.Running) {
                OnStop();
                started = false;
            }

            // Node is being called so we increase the call frequency count
            callFrequencyCount++;

            return state;
        }

        public virtual Node Clone() {
            return Instantiate(this);
        }

        public void Abort() {
            BehaviorTreeAgentController.Traverse(this, (node) => {
                node.started = false;
                node.state = State.Running;
                node.OnStop();
            });
        }

        public virtual void OnDrawGizmos() { }

        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State OnUpdate();

        //public abstract List<Node> GetChildren(); 

        public static string NodeStateToString(State state) {
            switch (state) {
                case State.Running:
                    return "Running";
                case State.Failure:
                    return "Failure";
                case State.Success:
                    return "Success";
                default:
                    return "Unknown";
            }
        }

        public static State NodeStateStringToNodeState(string state) {
            switch (state) {
                case "Running":
                    return State.Running;
                case "Failure":
                    return State.Failure;
                case "Success":
                    return State.Success;
                default:
                    return State.Running;
            }
        }

        public static Node CreateNodeTreeFromBehaviourTreeNodeDef(BehaviourTreeNodeDef currentBehaviourTreeNodeDef, List<BehaviourTreeNodeDef> behaviourTreeNodeDefs, BehaviorTreeAgentController tree) {
            // 1. Create node from rootBehaviourTreeNodeDef
            Node node = null;
            switch (currentBehaviourTreeNodeDef.m_Script.guid) {
                case "163c147d123e4a945b688eddc64e3ea5":
                    node = RootNode.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "afb5496e8cd973748a10b3e3ef436ebd":
                    node = Repeat.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "460be9e34c566ea45b9e282b1adcb028":
                    node = Selector.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "61431bba79d7d7843b82bf1de71703f5":
                    node = Sequencer.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "e658b1bd308bc5c429f5a9b404a04943":
                    node = Inverter.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "88210b6ae4b65bc4f975f7a750c75612":
                    node = Encapsulator.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "1fd1e85f30abba2499f6834e124b1450":
                    node = MoveForward.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "7e4181f6492e3fc45bf357f24d63fd4d":
                    node = MoveSide.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "ef843663b73a3c544b520ab90e69c9f4":
                    node = Rotate.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "2896f3e48c4d62d40be88fb007bb6361":
                    node = RayHitObject.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "1191a1255814faa47b52cc65d43e6285":
                    node = RotateTurret.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "a3dc491c3b458a945b4bd32e24ed7627":
                    node = Shoot.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "dfab410791810f74995e77be14f491cb":
                    node = PlaceBomb.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "5c69b16a0f5bc7e43adf37e9a2a453dc":
                    node = GridCellContainsObject.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "e7005ef9ca3dafc4b86928393e168f40":
                    node = HealthLevelBellow.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "a380d7f9274338e4485e03ec126c5859":
                    node = ShieldLevelBellow.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
                case "3541a6a84d600d443a32c4b10ae322a3":
                    node = AmmoLevelBellow.CreateNodeFromBehaviourTreeNodeDef(currentBehaviourTreeNodeDef, behaviourTreeNodeDefs, tree);
                    break;
            }
            return node;
        }
    }
}