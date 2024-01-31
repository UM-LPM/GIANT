using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Tilemaps;
using UnityEngine;

public class GridSensor2D : Sensor<SensorPerceiveOutput[,]> {

    [Header("Base configuration")]
    [SerializeField] bool discretizeGridPos;


    [SerializeField] Vector2Int gridSize;
    [SerializeField] Vector2 cellSize;
    [SerializeField] Vector2 cellSpacing;
    [SerializeField] public LayerMask layerMask;
    [SerializeField] Vector2 centerOffset;

    [Header("Sensor position")]
    [SerializeField] bool fixedPosition;
    [SerializeField] Vector2 position;

    [Header("Sensor result")]
    [SerializeField] Texture3D sensorCapturePreview;

    public SensorPerceiveOutput[,] SensorPerceiveOutputs { get; private set; }

    public GridSensor2D() : base("GridSensor") {

    }

    public override SensorPerceiveOutput[,] Perceive() {
        Vector2 agentPos = discretizeGridPos? new Vector2(CustomRound(transform.position.x), CustomRound(transform.position.y)) : transform.position;
        SensorPerceiveOutputs = new SensorPerceiveOutput[gridSize.x, gridSize.y];

        Vector2 cellWorldPosition = fixedPosition ? position : (new Vector2(agentPos.x, agentPos.y) + centerOffset);

        // Loop through each cell in the SensorPerceiveOutputs
        for (int x = 0; x < gridSize.x; x++) {
            for (int y = 0; y < gridSize.y; y++) {
                // Calculate the center of the current cell
                Vector2 cellCenter = new Vector2((x * cellSpacing.x), (y * cellSpacing.y)) + cellWorldPosition;

                // Use Physics.OverlapBox to detect objects in the cell
                Collider2D[] hitColliders = Physics2D.OverlapBoxAll(cellCenter, new Vector2(cellSize.x / 1.15f, cellSize.y / 1.15f), 0f, layerMask);
                // If any objects were detected, store the first one in the SensorPerceiveOutputs array
                if (hitColliders.Length > 0 && hitColliders[0].gameObject != gameObject) {
                    SensorPerceiveOutputs[x, y] = new SensorPerceiveOutput { HasHit = true, HitGameObjects = hitColliders.Select(a => a.gameObject).ToArray(), HitCenterPos = cellCenter };
                }
                else {
                    SensorPerceiveOutputs[x, y] = new SensorPerceiveOutput { HasHit = false, HitGameObjects = null, HitCenterPos = cellCenter };
                }
            }
        }

        return SensorPerceiveOutputs;
    }

    void OnDrawGizmosSelected() {
        Perceive();
        //Vector2 agentPos = discretizeGridPos ? new Vector2(CustomRound(transform.position.x), CustomRound(transform.position.y)) : transform.position;

        //Vector2 cellWorldPosition = fixedPosition ? position : (new Vector2(agentPos.x, agentPos.y) + centerOffset);

        // Loop through each cell in the SensorPerceiveOutputs
        for (int x = 0; x < gridSize.x; x++) {
            for (int y = 0; y < gridSize.y; y++) {
                // Calculate the center of the current cell
                //Vector2 cellCenter = new Vector2(x * cellSpacing.x, y * cellSpacing.y) + cellWorldPosition;

                // Draw a wire cube at the cell's position with the cell's size
                if (SensorPerceiveOutputs[x, y] != null && SensorPerceiveOutputs[x, y].HasHit) {
                    Gizmos.color = SensorPerceiveOutputs[x, y] != null && SensorPerceiveOutputs[x, y].HasHit ? Color.red : Color.white;
                    Gizmos.DrawWireCube(SensorPerceiveOutputs[x, y].HitCenterPos, new Vector2(cellSize.x, cellSize.y));
                }
            }
        }
    }

    private float CustomRound(float value) {
        return Mathf.RoundToInt(value);
    }
}
