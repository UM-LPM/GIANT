using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Grid {
    public Vector3Int GridSize { get; set; }
    public Vector3Int GridSpacing { get; set; }

    public GridCell[,,] GridCells { get; set; }

    public Grid(Vector3Int gridSize, Vector3Int gridSpacing) {
        GridSize = gridSize;
        GridSpacing = gridSpacing;

        InitializeGrid(gridSize);
    }

    public void InitializeGrid(Vector3Int gridSize) {
        GridCells = new GridCell[gridSize.x, gridSize.y, gridSize.z];

        for (int x = 0; x < GridSize.x; x++) {
            for (int y = 0; y < GridSize.y; y++) {
                for (int z = 0; z < GridSize.z; z++) {
                    GridCells[x, y, z] = new GridCell(new Vector3(x * GridSpacing.x, y * GridSpacing.y, z * GridSpacing.z));
                }
            }
        }
        // Implement any additional logic
    }


    public int NumberOfUsedGridCells() {
        int usedGridCells = 0;
        for (int x = 0; x < GridSize.x; x++) {
            for (int y = 0; y < GridSize.y; y++) {
                for (int z = 0; z < GridSize.z; z++) {
                    if (GridCells[x, y, z].GridAvailability > 0) {
                        usedGridCells++;
                    }
                }
            }
        }
        return usedGridCells;
    }

    public GridCell GetAndReserveAvailableGridlayer(int matchIndex, string gameSceneName, string agentSceneName) {
        if (CanUseAnotherGridCell()) {
            for (int x = 0; x < GridSize.x; x++) {
                for (int y = 0; y < GridSize.y; y++) {
                    for (int z = 0; z < GridSize.z; z++) {
                        if (GridCells[x, y, z].GridAvailability == 0) {
                            GridCells[x, y, z].GridAvailability = 1;
                            GridCells[x, y, z].MatchIndex = matchIndex;
                            GridCells[x, y, z].GameSceneName = gameSceneName;
                            GridCells[x, y, z].AgentSceneName = agentSceneName;
                            return GridCells[x, y, z];
                        }
                    }
                }
            }
        }
        return null;
    }

    public bool IsBatchExecuted(string gameSceneName) {
        if (NumberOfUsedGridCells() == 0) {
            if (SceneManager.sceneCount > 1)
                SceneManager.UnloadSceneAsync(gameSceneName);
            SceneManager.LoadScene(gameSceneName);
        }
        return true;
    }

    public bool CanUseAnotherGridCell() {
        if (NumberOfUsedGridCells() >= (GridSize.x * GridSize.y * GridSize.z))
            return false;
        return true;
    }

    public void ReleaseGridCell(GridCell gridCell) {
        //GridCells[gridCell[0], gridCell[1], gridCell[2]].GridAvailability = 0;
        gridCell.GridAvailability = 0;
        gridCell.MatchIndex = -1;
    }

    public GridCell GetReservedGridCell() {
        for (int x = 0; x < GridSize.x; x++) {
            for (int y = 0; y < GridSize.y; y++) {
                for (int z = 0; z < GridSize.z; z++) {
                    if (GridCells[x, y, z].GridAvailability == 1) {
                        GridCells[x, y, z].GridAvailability = 2;
                        return GridCells[x, y, z];
                    }
                }
            }
        }

        Debug.LogError("GetReservedLayer() method returned null");
        return null;
    }
}

public class GridCell {
    // 0 - Available; 1 - Reserverd; 2 - In Use
    public int GridAvailability { get; set; }
    public Vector3 GridCellPosition { get; set; }
    public int MatchIndex { get; set; }
    public int Layer { get; set; }

    public string GameSceneName { get; set; }
    public string AgentSceneName { get; set; }

    public GridCell(Vector3 gridCellPosition) {
        GridCellPosition = gridCellPosition;
        GridAvailability = 0;
        MatchIndex = -1;
        Layer = 6; // Field1

    }
}