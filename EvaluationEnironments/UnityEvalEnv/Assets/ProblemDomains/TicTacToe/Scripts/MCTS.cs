using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Problems.TicTacToe
{
    public class MCTSNode
    {
        public int[,,] State;
        public MCTSNode Parent;
        public List<MCTSNode> Children = new List<MCTSNode>();
        public List<Vector3Int> UntriedMoves;
        public TicTacToeGrid Grid;

        public Vector3Int Move;
        public int PlayerToMove;

        public int Visits = 0;
        public double Wins = 0;

        public MCTSNode(int[,,] state, MCTSNode parent, Vector3Int move, int playerToMove, TicTacToeGrid grid)
        {
            State = (int[,,])state.Clone();
            Parent = parent;
            Move = move;
            PlayerToMove = playerToMove;
            Grid = grid;

            UntriedMoves = GetMoves(state);
        }

        List<Vector3Int> GetMoves(int[,,] s)
        {
            var moves = new List<Vector3Int>();
            for (int x = 0; x < s.GetLength(0); x++)
                for (int y = 0; y < s.GetLength(1); y++)
                    for (int z = 0; z < s.GetLength(2); z++)
                        if (s[x, y, z] == -1)
                            moves.Add(new Vector3Int(x, y, z));
            return moves;
        }

        public bool IsFullyExpanded() => UntriedMoves.Count == 0;

        public bool IsTerminal()
        {
            var (term, _) = Grid.CheckWinner(State);
            return term;
        }

        public MCTSNode Expand()
        {
            var move = UntriedMoves[UntriedMoves.Count - 1];
            UntriedMoves.RemoveAt(UntriedMoves.Count - 1);

            var newState = (int[,,])State.Clone();
            Grid.Apply(newState, move, PlayerToMove);

            int nextPlayer = Grid.GetOpponentId(newState, PlayerToMove);

            var child = new MCTSNode(newState, this, move, nextPlayer, Grid);
            Children.Add(child);

            return child;
        }

        public MCTSNode SelectChild()
        {
            double c = 1.4;

            return Children.OrderByDescending(child =>
            {
                double uct =
                    (child.Wins / (child.Visits + 1e-6)) +
                    c * Math.Sqrt(Math.Log(Visits + 1) / (child.Visits + 1e-6));
                return uct;
            }).First();
        }

        public void Backpropagate(double result)
        {
            Visits++;
            Wins += result;

            Parent?.Backpropagate(1 - result); // flip perspective
        }
    }
}