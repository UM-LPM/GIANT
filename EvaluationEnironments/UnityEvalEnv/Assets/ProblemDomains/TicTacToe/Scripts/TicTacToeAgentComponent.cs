using Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Problems.TicTacToe
{
    public class TicTacToeAgentComponent : AgentComponent
    {
        public bool MarkerPlacedCurrentRound { get; set; }

        public int MarkersPlaced { get; set; }
        public int OpportunitiesCreated { get; set; }
        public int OpponnentBlocked { get; set; }
        public int OptimalMoves { get; set; }
    }
}
