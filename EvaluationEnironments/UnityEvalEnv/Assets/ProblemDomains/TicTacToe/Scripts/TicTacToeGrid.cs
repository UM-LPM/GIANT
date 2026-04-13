using Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Problems.TicTacToe
{
    public enum TicTacToeBestMoveAlgorithm
    {
        Minimax,
        MCTS,
        Heuristic
    }

    public class TicTacToeGrid : MonoBehaviour
    {
        private static Dictionary<string, Vector3Int> BestMoveCache = new Dictionary<string, Vector3Int>();
        private static object cacheLock = new object();

        TicTacToeGridCell[,,] Cells { get; set; }
        TicTacToeEnvironmentController TicTacToeEnvironmentController { get; set; }

        public static int MINIMAX_MAX_DEPTH = 10; // For Minimax algorithm - can be adjusted based on grid size and performance needs

        public static int MCTS_ITERATIONS = 1000;
        public static TicTacToeBestMoveAlgorithm BEST_MOVE_ALG = TicTacToeBestMoveAlgorithm.Minimax;

        private static readonly (int dx, int dy, int dz)[] Directions =
        {
            (1,0,0),(0,1,0),(0,0,1),
            (1,1,0),(1,-1,0),(1,0,1),(1,0,-1),
            (0,1,1),(0,1,-1),
            (1,1,1),(1,1,-1),(1,-1,1),(1,-1,-1)
        };

        const int WIN_SCORE = 1_000_000;
        const int BLOCK_SCORE = 500_000;
        const int FORK_SCORE = 50_000;
        const int OPPORTUNITY_SCORE = 10_000; // creating a (marksInARow - 1) with an open cell is good, but much less than an immediate win or fork
        const int CENTER_SCORE = 500;
        const int CORNER_SCORE = 200;
        const int OTHER_SCORE = 50;

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

        public bool PlaceRandomMarker(TicTacToeMarker marker, TicTacToeAgentComponent agent)
        {
            List<TicTacToeGridCell> emptyCells = GetEmptyCells();

            if (emptyCells.Count > 0)
            {
                int randomIndex = TicTacToeEnvironmentController.Util.Rnd.Next(0, emptyCells.Count);
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
                return false;

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

            if(OpportunityCreated(cell, markerComponent.MarkerId) > 0)
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

        public int OpportunityCreated(TicTacToeGridCell cell, int markerId)
        {
            int opportunitiesCreated = 0;
            int marksInRow = TicTacToeEnvironmentController.MarksInARow;

            foreach (var (dx, dy, dz) in Directions)
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
                        opportunitiesCreated++;
                }
            }


            return opportunitiesCreated;
        }

        public bool OpponentBlocked(TicTacToeGridCell cell, TicTacToeMarker marker, TicTacToeAgentComponent agent)
        {
            int marksInRow = TicTacToeEnvironmentController.MarksInARow;
            int markerId = marker.MarkerId;

            foreach (var (dx, dy, dz) in Directions)
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

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        var startCell = Cells[x, y, z];
                        if (startCell.Marker == null) continue;

                        int markerId = startCell.Marker.MarkerId;

                        foreach (var (dx, dy, dz) in Directions)
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
            if(BEST_MOVE_ALG == TicTacToeBestMoveAlgorithm.Heuristic)
            {
                int[,,] rootState = CaptureState();
                string key = SerializeState(rootState);

                lock (cacheLock)
                {
                    if (BestMoveCache.TryGetValue(key, out var cachedMove))
                        return cachedMove;
                }

                var moves = GetAvailableMoves(rootState);

                Vector3Int bestMove = default;
                int bestScore = int.MinValue;

                foreach (var move in moves)
                {
                    int score = EvaluateMove(rootState, move, markerId);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }

                    if(score == WIN_SCORE) // can't do better than an immediate win, stop searching
                        break;
                }

                lock (cacheLock)
                {
                    if (!BestMoveCache.ContainsKey(key))
                        BestMoveCache[key] = bestMove;
                }


                return bestMove;

            }
            else if (BEST_MOVE_ALG == TicTacToeBestMoveAlgorithm.MCTS)
            {
                // MCTS 
                int[,,] rootState = CaptureState();
                string key = SerializeState(rootState);

                lock (cacheLock)
                {
                    if (BestMoveCache.TryGetValue(key, out var cachedMove))
                        return cachedMove;
                }

                var root = new MCTSNode(rootState, null, default, markerId, this);

                int iterations = MCTS_ITERATIONS; // tune (500–5000 typical)

                for (int i = 0; i < iterations; i++)
                {
                    var node = root;

                    // 1. Selection
                    while (node.IsFullyExpanded() && node.Children.Count > 0)
                        node = node.SelectChild();

                    // 2. Expansion
                    if (!node.IsTerminal())
                        node = node.Expand();

                    // 3. Simulation
                    double result = Rollout(node.State, markerId);

                    // 4. Backpropagation
                    node.Backpropagate(result);
                }

                // pick most visited move
                Vector3Int bestMove = root.Children
                    .OrderByDescending(c => c.Visits)
                    .First().Move;

                lock (cacheLock)
                {
                    if (!BestMoveCache.ContainsKey(key))
                        BestMoveCache[key] = bestMove;
                }

                return bestMove;
            }
            else if (BEST_MOVE_ALG == TicTacToeBestMoveAlgorithm.Minimax)
            {
                // Minimax with alpha-beta pruning and caching
                int[,,] state = CaptureState();
                string key = SerializeState(state);

                lock (cacheLock)
                {
                    if (BestMoveCache.TryGetValue(key, out var cachedMove))
                        return cachedMove;
                }

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

                lock (cacheLock)
                {
                    if (!BestMoveCache.ContainsKey(key))
                        BestMoveCache[key] = bestMove;
                }


                return bestMove;
            }
            else
            {
                throw new Exception("Invalid best move algorithm selected");
            }
        }

        double Rollout(int[,,] state, int rootPlayer)
        {
            var simState = (int[,,])state.Clone();

            int currentPlayer = rootPlayer;

            while (true)
            {
                var (terminal, winner) = CheckWinner(simState);

                if (terminal)
                {
                    if (winner == rootPlayer) return 1.0;
                    if (winner == -1) return 0.5;
                    return 0.0;
                }

                var moves = GetAvailableMoves(simState);

                // simple random playout
                var move = moves[TicTacToeEnvironmentController.Util.Rnd.Next(0, moves.Count)];

                Apply(simState, move, currentPlayer);
                currentPlayer = GetOpponentId(simState, currentPlayer);
            }
        }

        private int EvaluateMove(int[,,] state, Vector3Int move, int playerId)
        {
            int sx = state.GetLength(0);
            int sy = state.GetLength(1);
            int sz = state.GetLength(2);

            int x = move.x;
            int y = move.y;
            int z = move.z;

            Apply(state, move, playerId);

            var (terminal, winner) = CheckWinner(state);

            // 1. Immediate win
            if (terminal && winner == playerId)
            {
                Undo(state, move);
                return WIN_SCORE;
            }

            int opponentId = GetOpponentId(state, playerId);

            // 2. Block opponent win (ONLY win-level blocking)
            state[x, y, z] = opponentId;
            var (oppTerminal, oppWinner) = CheckWinner(state);
            state[x, y, z] = playerId;

            if (oppTerminal && oppWinner == opponentId)
            {
                Undo(state, move);
                return BLOCK_SCORE;
            }

            // 3. Opportunity creation and fork detection (count immediate winning threats created)
            int opportunities = CountForkOpportunities(state, move, playerId);
            if (opportunities >= 2)
            {
                Undo(state, move);
                return FORK_SCORE;
            }
            else if (opportunities == 1) {
                Undo(state, move);
                return OPPORTUNITY_SCORE;
            }

            // 5. Positional heuristics (center → corners → others)
            int score = EvaluatePosition(move, sx, sy, sz, CENTER_SCORE, CORNER_SCORE, OTHER_SCORE);

            Undo(state, move);

            return score;
        }

        private bool HasPotentialWinLine(int[,,] state, Vector3Int origin, int dx, int dy, int dz, int playerId)
        {
            int marksInRow = TicTacToeEnvironmentController.MarksInARow;

            int sx = state.GetLength(0);
            int sy = state.GetLength(1);
            int sz = state.GetLength(2);

            int count = 1;
            int empty = 0;

            int nx = origin.x;
            int ny = origin.y;
            int nz = origin.z;

            for (int i = 1; i < marksInRow; i++)
            {
                nx += dx;
                ny += dy;
                nz += dz;

                if ((uint)nx >= sx || (uint)ny >= sy || (uint)nz >= sz)
                    break;

                int v = state[nx, ny, nz];

                if (v == playerId)
                    count++;
                else if (v == -1)
                    empty++;
            }

            return count == marksInRow - 1 && empty >= 0;
        }

        private int CountForkOpportunities(int[,,] state, Vector3Int move, int playerId)
        {
            int count = 0;

            foreach (var dir in Directions)
            {
                int threats = 0;

                // scan line through move
                if (HasPotentialWinLine(state, move, dir.dx, dir.dy, dir.dz, playerId))
                    threats++;

                if (threats >= 1)
                    count++;
            }

            return count;
        }

        private int EvaluatePosition(Vector3Int move, int sx, int sy, int sz, int centerScore, int cornerScore, int otherScore)
        {
            int cx = sx / 2;
            int cy = sy / 2;
            int cz = sz / 2;

            // center
            if (move.x == cx && move.y == cy && move.z == cz)
                return centerScore;

            // corners
            bool isCorner =
                (move.x == 0 || move.x == sx - 1) &&
                (move.y == 0 || move.y == sy - 1) &&
                (move.z == 0 || move.z == sz - 1);

            if (isCorner)
                return cornerScore;

            return otherScore;
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

        static string SerializeState(int[,,] state)
        {
            int sx = state.GetLength(0);
            int sy = state.GetLength(1);
            int sz = state.GetLength(2);

            StringBuilder sb = new StringBuilder(sx * sy * sz);

            for (int x = 0; x < sx; x++)
                for (int y = 0; y < sy; y++)
                    for (int z = 0; z < sz; z++)
                        sb.Append(state[x, y, z] + 1); // convert -1 → 0, 0 → 1, 1 → 2, etc.

            return sb.ToString();
        }

        int Minimax(int[,,] state, int depth, bool isMaximizing, int markerId, int alpha, int beta)
        {
            var (terminal, winner) = CheckWinner(state);

            if (terminal || depth >= MINIMAX_MAX_DEPTH)
            {
                if (winner == markerId) return MINIMAX_MAX_DEPTH - depth;
                if (winner == -1) return 0;
                return -MINIMAX_MAX_DEPTH + depth;
            }

            // TODO : Heuristic evaluation for non-terminal states when depth limit is reached (for larger boards) - If needed
            /*if (depth >= MAX_DEPTH)
            {
                return Heuristic(state, markerId);
            }*/

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

        public void Apply(int[,,] state, Vector3Int move, int playerId)
        {
            state[move.x, move.y, move.z] = playerId;
        }

        void Undo(int[,,] state, Vector3Int move)
        {
            state[move.x, move.y, move.z] = -1;
        }

        public int GetOpponentId(int[,,] state, int markerId)
        {
            foreach (var v in state)
            {
                if (v != -1 && v != markerId)
                    return v;
            }
            return TicTacToeEnvironmentController.GetAgentOpponentMarkerID(markerId); // fallback
        }

        public (bool, int) CheckWinner(int[,,] state)
        {
            int marksInRow = TicTacToeEnvironmentController.MarksInARow;

            int sx = state.GetLength(0);
            int sy = state.GetLength(1);
            int sz = state.GetLength(2);

            for (int x = 0; x < sx; x++)
                for (int y = 0; y < sy; y++)
                    for (int z = 0; z < sz; z++)
                    {
                        int player = state[x, y, z];
                        if (player == -1) continue;

                        foreach (var (dx, dy, dz) in Directions)
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
