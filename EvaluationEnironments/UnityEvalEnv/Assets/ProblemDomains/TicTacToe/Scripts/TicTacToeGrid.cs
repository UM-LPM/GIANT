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
    public class TicTacToeGrid
    {
        TicTacToeGridCell[,,] Cells { get; set; }

        public TicTacToeGrid(int sizeX, int sizeY, int sizeZ)
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
            cell.Marker = marker;
            PlaceMarker(cell, marker, agent);
            return true;
        }

        public void PlaceRandomMarker(TicTacToeMarker marker, Util util, TicTacToeAgentComponent agent)
        {
            List<TicTacToeGridCell> emptyCells = GetEmptyCells();

            if (emptyCells.Count > 0)
            {
                int randomIndex = util.Rnd.Next(0, emptyCells.Count);
                PlaceMarker(emptyCells[randomIndex], marker, agent);
            }
        }

        private void PlaceMarker(TicTacToeGridCell cell, TicTacToeMarker marker, TicTacToeAgentComponent agent)
        {
            GameObject markerObj = GameObject.Instantiate(marker.gameObject, new Vector3(cell.Position.x, cell.Position.y, cell.Position.z), Quaternion.identity, cell.gameObject.transform);
            markerObj.name = $"Marker_{marker.MarkerType}_{marker.MarkerId}_{cell.Position.x}_{cell.Position.y}_{cell.Position.z}";

            TicTacToeMarker markerComponent = markerObj.GetComponent<TicTacToeMarker>();
            markerComponent.MarkerId = marker.MarkerId;
            cell.Marker = markerComponent;

            agent.MarkersPlaced++;
        }

        public bool AllMarkersPlaced()
        {
            return GetEmptyCells().Count == 0;
        }

        public (bool, int) XInRowAchieved(int marksInRow)
        {
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
    }
}
