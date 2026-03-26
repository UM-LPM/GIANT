using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.TicTacToe
{
    public class TicTacToeMarker : MonoBehaviour
    {
        public MarkerType MarkerType;
        public int MarkerId { get; set; } // == IndividualID of the agent that placed the marker
    }

    public enum MarkerType
    {
        X,
        O
    }
}
