using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheKiwiCoder;
using Unity.Collections;

namespace Assets.TheKiwiCoder {
    public enum NodeType {
        Selector,
        Sequencer,
        Repeat,
        Inverter,
        RayHitObject,
        MoveForward,
        MoveSide,
        Rotate
    }

    public struct NodeStruct {
        
        NativeArray<NodeStruct> Children { get; set; }
        Node.State State { get; set; }
        bool Started { get; set; }
        int Guid { get; set; }
        int Current { get; set;}

        public Node.State Update() {

            if (!Started) {
                OnStart();
                Started = true;
            }

            State = OnUpdate();

            if (State != Node.State.Running) {
                OnStop();
                Started = false;
            }

            return State;
        }

        public void OnStart() {
            throw new NotImplementedException();
        }

        public void OnStop() {
            throw new NotImplementedException();
        }

        public Node.State OnUpdate() {
            throw new NotImplementedException();
        }
    }
}
