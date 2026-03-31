using Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utils;

namespace Problems.TicTacToe
{
    public class TicTacToeGrid : MonoBehaviour
    {
        TicTacToeGridCell[,,] Cells { get; set; }
        TicTacToeEnvironmentController TicTacToeEnvironmentController { get; set; }

        int MAX_DEPTH = 10; // For Minimax algorithm - can be adjusted based on grid size and performance needs

        private void Awake()
        {
            TicTacToeEnvironmentController = GetComponent<TicTacToeEnvironmentController>();
        }

        public void Init(int sizeX, int sizeY, int sizeZ)
        {
            Cells = new TicTacToeGridCell[sizeX, sizeY, sizeZ];
        }

        public void Spawn(GameObject cellPrefab, Transform parent)
        {
            for (int x = 0; x < Cells.GetLength(0); x++)
            {
                for (int y = 0; y < Cells.GetLength(1); y++)
                {
                    for (int z = 0; z < Cells.GetLength(2); z++)
                    {
                        GameObject cellObj = GameObject.Instantiate(cellPrefab, new Vector3(x, y, z), Quaternion.identity, parent);
                        cellObj.name = $"Cell_{x}_{y}_{z}";
                        Cells[x, y, z] = cellObj.GetComponent<TicTacToeGridCell>();
                        Cells[x, y, z].Position = new Vector3Int(x, y, z);
                    }
                }
            }
        }

        public TicTacToeGridCell GetCell(int x, int y, int z)
        {
            if (x < 0 || x >= Cells.GetLength(0) || y < 0 || y >= Cells.GetLength(1) || z < 0 || z >= Cells.GetLength(2))
            {
                throw new IndexOutOfRangeException("Cell position is out of bounds");
            }
            return Cells[x, y, z];
        }

        public bool IsCellOccupied(int x, int y, int z)
        {
            return GetCell(x, y, z).Marker != null;
        }

        public bool IsCellOccupiedByAgent(int x, int y, int z, int agentId)
        {
            var cell = GetCell(x, y, z);
            return cell.Marker != null && cell.Marker.MarkerId == agentId;
        }

        public bool IsCellOccupiedByOtherAgent(int x, int y, int z, int agentId)
        {
            var cell = GetCell(x, y, z);
            return cell.Marker != null && cell.Marker.MarkerId != agentId;
        }

        public List<TicTacToeGridCell> GetEmptyCells()
        {
            List<TicTacToeGridCell> emptyCells = new List<TicTacToeGridCell>();
            for (int x = 0; x < Cells.GetLength(0); x++)
            {
                for (int y = 0; y < Cells.GetLength(1); y++)
                {
                    for (int z = 0; z < Cells.GetLength(2); z++)
                    {
                        if (!IsCellOccupied(x, y, z))
                        {
                            emptyCells.Add(GetCell(x, y, z));
                        }
                    }
                }
            }
            return emptyCells;
        }

        public bool PlaceMarker(int x, int y, int z, TicTacToeMarker marker, TicTacToeAgentComponent agent)
        {
            TicTacToeGridCell cell = GetCell(x, y, z);
            if (IsCellOccupied(x, y, z))
            {
                return false;
            }

            return PlaceMarker(cell, marker, agent);
        }

        public bool PlaceRandomMarker(TicTacToeMarker marker, Util util, TicTacToeAgentComponent agent)
        {
            List<TicTacToeGridCell> emptyCells = GetEmptyCells();

            if (emptyCells.Count > 0)
            {
                int randomIndex = util.Rnd.Next(0, emptyCells.Count);
                return PlaceMarker(emptyCells[randomIndex], marker, agent);
            }

            return false;
        }

        private bool PlaceMarker(TicTacToeGridCell cell, TicTacToeMarker marker, TicTacToeAgentComponent agent)
        {
            if (TicTacToeEnvironmentController.WinIndividualID != -1 ||
                agent.MarkerPlacedCurrentRound
                )
                return false;

            if (cell.Marker != null)
                throw new Exception($"Trying to place a marker on an occupied cell at position {cell.Position}");

            Vector3Int bestMove = GetBestMove(marker.MarkerId);
            if(bestMove == cell.Position)
            {
                agent.OptimalMoves++;
            }

            GameObject markerObj = GameObject.Instantiate(marker.gameObject, new Vector3(cell.Position.x, cell.Position.y, cell.Position.z), Quaternion.identity, cell.gameObject.transform);
            markerObj.name = $"Marker_{marker.MarkerType}_{marker.MarkerId}_{cell.Position.x}_{cell.Position.y}_{cell.Position.z}";

            TicTacToeMarker markerComponent = markerObj.GetComponent<TicTacToeMarker>();
            markerComponent.MarkerId = marker.MarkerId;
            cell.Marker = markerComponent;

            if(OpportunityCreated(cell, markerComponent, agent))
                agent.OpportunitiesCreated++;

            if(OpponentBlocked(cell, markerComponent, agent))
                agent.OpponnentBlocked++;

            agent.MarkersPlaced++;

            agent.MarkerPlacedCurrentRound = true;

            TicTacToeEnvironmentController.CheckIfMarksInRowAchieved();

            return true;
        }

        public bool AllMarkersPlaced()
        {
            return GetEmptyCells().Count == 0;
        }

        public bool OpportunityCreated(TicTacToeGridCell cell, TicTacToeMarker marker, TicTacToeAgentComponent agent)
        {
            int marksInRow = TicTacToeEnvironmentController.MarksInARow;
            int markerId = marker.MarkerId;

            (int dx, int dy, int dz)[] directions = new (int, int, int)[]
            {
                (1,0,0), (0,1,0), (0,0,1),
                (1,1,0), (1,-1,0), (1,0,1), (1,0,-1), (0,1,1), (0,1,-1),
                (1,1,1), (1,1,-1), (1,-1,1), (1,-1,-1)
            };

            foreach (var (dx, dy, dz) in directions)
            {
                var line = GetLine(cell, dx, dy, dz);

                for (int i = 0; i <= line.Count - marksInRow; i++)
                {
                    var window = line.Skip(i).Take(marksInRow);

                    int myCount = 0;
                    int emptyCount = 0;

                    foreach (var c in window)
                    {
                        if (c.Marker == null) emptyCount++;
                        else if (c.Marker.MarkerId == markerId) myCount++;
                    }

                    if (myCount == marksInRow - 1 && emptyCount == 1)
                        return true;
                }
            }


            return false;
        }

        public bool OpponentBlocked(TicTacToeGridCell cell, TicTacToeMarker marker, TicTacToeAgentComponent agent)
        {
            int marksInRow = TicTacToeEnvironmentController.MarksInARow;
            int markerId = marker.MarkerId;

            (int dx, int dy, int dz)[] directions = new (int, int, int)[]
            {
                (1,0,0), (0,1,0), (0,0,1),
                (1,1,0), (1,-1,0), (1,0,1), (1,0,-1), (0,1,1), (0,1,-1),
                (1,1,1), (1,1,-1), (1,-1,1), (1,-1,-1)
            };

            foreach (var (dx, dy, dz) in directions)
            {
                var line = GetLine(cell, dx, dy, dz);

                for (int i = 0; i <= line.Count - marksInRow; i++)
                {
                    var window = line.Skip(i).Take(marksInRow);

                    int opponentId = -1;
                    int opponentCount = 0;
                    int myCount = 0;

                    foreach (var c in window)
                    {
                        if (c.Marker == null) continue;

                        if (c.Marker.MarkerId == markerId)
                            myCount++;
                        else
                        {
                            opponentId = c.Marker.MarkerId;
                            opponentCount++;
                        }
                    }

                    // If before move: opponent had (marksInRow - 1) and THIS cell was empty
                    // After move: we occupy it -> block

                    if (opponentCount == marksInRow - 1 &&
                        myCount == 1 && // this placed marker
                        window.Contains(cell))
                    {
                        return true;
                    }
                }
            }


            return false;
        }

        private List<TicTacToeGridCell> GetLine(TicTacToeGridCell origin, int dx, int dy, int dz)
        {
            List<TicTacToeGridCell> line = new List<TicTacToeGridCell>();

            int sizeX = Cells.GetLength(0);
            int sizeY = Cells.GetLength(1);
            int sizeZ = Cells.GetLength(2);

            int x = origin.Position.x;
            int y = origin.Position.y;
            int z = origin.Position.z;

            // Step 1: go backwards to the start of the line
            while (true)
            {
                int px = x - dx;
                int py = y - dy;
                int pz = z - dz;

                if (px < 0 || px >= sizeX ||
                    py < 0 || py >= sizeY ||
                    pz < 0 || pz >= sizeZ)
                    break;

                x = px;
                y = py;
                z = pz;
            }

            // Step 2: walk forward and collect the full line
            while (x >= 0 && x < sizeX &&
                   y >= 0 && y < sizeY &&
                   z >= 0 && z < sizeZ)
            {
                line.Add(Cells[x, y, z]);

                x += dx;
                y += dy;
                z += dz;
            }

            return line;
        }

        public (bool, int) MarksInRowAchieved()
        {
            int marksInRow = TicTacToeEnvironmentController.MarksInARow;
            // Check rows, columns, and diagonals for marksInRow in a row for the same marker (MarkerId)
            int sizeX = Cells.GetLength(0);
            int sizeY = Cells.GetLength(1);
            int sizeZ = Cells.GetLength(2);

            // All relevant directions
            (int dx, int dy, int dz)[] directions = new (int, int, int)[]
            {
                (1,0,0), (0,1,0), (0,0,1),
                (1,1,0), (1,-1,0), (1,0,1), (1,0,-1), (0,1,1), (0,1,-1),
                (1,1,1), (1,1,-1), (1,-1,1), (1,-1,-1)
            };

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        var startCell = Cells[x, y, z];
                        if (startCell.Marker == null) continue;

                        int markerId = startCell.Marker.MarkerId;

                        foreach (var (dx, dy, dz) in directions)
                        {
                            int count = 1;
                            //List<TicTacToeGridCell> cellsInLine = new List<TicTacToeGridCell> { startCell };

                            for (int step = 1; step < marksInRow; step++)
                            {
                                int nx = x + dx * step;
                                int ny = y + dy * step;
                                int nz = z + dz * step;

                                // Bounds check
                                if (nx < 0 || nx >= sizeX ||
                                    ny < 0 || ny >= sizeY ||
                                    nz < 0 || nz >= sizeZ)
                                    break;

                                var cell = Cells[nx, ny, nz];

                                if (cell.Marker == null || cell.Marker.MarkerId != markerId)
                                    break;

                                count++;
                                //cellsInLine.Add(cell);
                            }

                            if (count == marksInRow)
                                return (true, markerId);
                        }
                    }
                }
            }

            return (false, -1);
        }

        // Minimax algorithm logic for determining the best move for a given marker
        
        private Vector3Int GetBestMove(int markerId) // markerId represents the agent for which we want to find the best move
        {
            int[,,] state = CaptureState();

            int bestScore = int.MinValue;
            Vector3Int bestMove = new Vector3Int(-1, -1, -1);

            foreach (var move in GetAvailableMoves(state))
            {
                Apply(state, move, markerId);

                int score = Minimax(
                    state,
                    depth: 0,
                    isMaximizing: false,
                    markerId: markerId,
                    alpha: int.MinValue,
                    beta: int.MaxValue
                );

                Undo(state, move);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        int[,,] CaptureState()
        {
            int sx = Cells.GetLength(0);
            int sy = Cells.GetLength(1);
            int sz = Cells.GetLength(2);

            int[,,] state = new int[sx, sy, sz];

            for (int x = 0; x < sx; x++)
                for (int y = 0; y < sy; y++)
                    for (int z = 0; z < sz; z++)
                    {
                        var m = Cells[x, y, z].Marker;
                        state[x, y, z] = m == null ? -1 : m.MarkerId;
                    }

            return state;
        }

        int Minimax(int[,,] state, int depth, bool isMaximizing, int markerId, int alpha, int beta)
        {
            var (terminal, winner) = CheckWinner(state);

            if (terminal)
            {
                if (winner == markerId) return MAX_DEPTH - depth;
                if (winner == -1) return 0;
                return -MAX_DEPTH + depth;
            }

            // TODO : Heuristic evaluation for non-terminal states when depth limit is reached (for larger boards) - If needed
            //if (depth >= MAX_DEPTH)
            //    return Heuristic(state, myId);

            var moves = GetAvailableMoves(state);

            if (isMaximizing)
            {
                int best = int.MinValue;

                foreach (var move in moves)
                {
                    Apply(state, move, markerId);
                    int score = Minimax(state, depth + 1, false, markerId, alpha, beta);
                    Undo(state, move);

                    best = Math.Max(best, score);
                    alpha = Math.Max(alpha, best);

                    if (beta <= alpha) break; // prune
                }

                return best;
            }
            else
            {
                int opponentId = GetOpponentId(state, markerId);
                int best = int.MaxValue;

                foreach (var move in moves)
                {
                    Apply(state, move, opponentId);
                    int score = Minimax(state, depth + 1, true, markerId, alpha, beta);
                    Undo(state, move);

                    best = Math.Min(best, score);
                    beta = Math.Min(beta, best);

                    if (beta <= alpha) break; // prune
                }

                return best;
            }
        }

        List<Vector3Int> GetAvailableMoves(int[,,] state)
        {
            var moves = new List<Vector3Int>();

            for (int x = 0; x < state.GetLength(0); x++)
                for (int y = 0; y < state.GetLength(1); y++)
                    for (int z = 0; z < state.GetLength(2); z++)
                    {
                        if (state[x, y, z] == -1)
                            moves.Add(new Vector3Int(x, y, z));
                    }

            return moves;
        }

        void Apply(int[,,] state, Vector3Int move, int playerId)
        {
            state[move.x, move.y, move.z] = playerId;
        }

        void Undo(int[,,] state, Vector3Int move)
        {
            state[move.x, move.y, move.z] = -1;
        }

        int GetOpponentId(int[,,] state, int myId)
        {
            foreach (var v in state)
            {
                if (v != -1 && v != myId)
                    return v;
            }
            return myId == 0 ? 1 : 0; // fallback
        }

        (bool, int) CheckWinner(int[,,] state)
        {
            int marksInRow = TicTacToeEnvironmentController.MarksInARow;

            int sx = state.GetLength(0);
            int sy = state.GetLength(1);
            int sz = state.GetLength(2);

            (int dx, int dy, int dz)[] dirs = new (int, int, int)[]
            {
                (1,0,0),(0,1,0),(0,0,1),
                (1,1,0),(1,-1,0),(1,0,1),(1,0,-1),(0,1,1),(0,1,-1),
                (1,1,1),(1,1,-1),(1,-1,1),(1,-1,-1)
            };

            for (int x = 0; x < sx; x++)
                for (int y = 0; y < sy; y++)
                    for (int z = 0; z < sz; z++)
                    {
                        int player = state[x, y, z];
                        if (player == -1) continue;

                        foreach (var (dx, dy, dz) in dirs)
                        {
                            int count = 1;

                            for (int step = 1; step < marksInRow; step++)
                            {
                                int nx = x + dx * step;
                                int ny = y + dy * step;
                                int nz = z + dz * step;

                                if (nx < 0 || ny < 0 || nz < 0 ||
                                    nx >= sx || ny >= sy || nz >= sz)
                                    break;

                                if (state[nx, ny, nz] != player)
                                    break;

                                count++;
                            }

                            if (count == marksInRow)
                                return (true, player);
                        }
                    }

            // draw?
            bool full = true;
            foreach (var v in state)
                if (v == -1) { full = false; break; }

            if (full) return (true, -1);

            return (false, -1);
        }

    }
}
