using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GridSensor3D : Sensor<SensorPerceiveOutput[,,]> {

    [Header("Base configuration")]
    [SerializeField] Vector3Int gridSize;
    [SerializeField] Vector3 cellSize;
    [SerializeField] Vector3 cellSpacing;
    [SerializeField] LayerMask layerMask;
    [SerializeField] Vector3 centerOffset;

    [Header("Sensor position")]
    [SerializeField] bool fixedPosition;
    [SerializeField] Vector3 position;

    [Header("Sensor result")]
    [SerializeField] Texture3D sensorCapturePreview;

    public SensorPerceiveOutput[,,] GridSensorOutputs { get; private set; }

    private void Awake() {
    }

    public GridSensor3D() : base("GridSensor") {

    }

    public override SensorPerceiveOutput[,,] Perceive() {
        // Create a new 3D array to store the detected objects
        GridSensorOutputs = new SensorPerceiveOutput[gridSize.x, gridSize.y, gridSize.z];

        Vector3 cellWorldPosition = fixedPosition ? position : (transform.position + centerOffset);

        // Loop through each cell in the SensorPerceiveOutputs
        for (int x = 0; x < gridSize.x; x++) {
            for (int y = 0; y < gridSize.y; y++) {
                for (int z = 0; z < gridSize.z; z++) {
                    // Calculate the center of the current cell
                    Vector3 cellCenter = new Vector3(x * cellSpacing.x, y * cellSpacing.y, z * cellSpacing.z) + cellWorldPosition;

                    // Use Physics.OverlapBox to detect objects in the cell
                    Collider[] hitColliders = Physics.OverlapBox(cellCenter, new Vector3(cellSize.x / 2, cellSize.y / 2, cellSize.z / 2), Quaternion.identity, layerMask);
                    // If any objects were detected, store the first one in the SensorPerceiveOutputs array
                    if (hitColliders.Length > 0 && hitColliders[0].gameObject != gameObject) {
                        GridSensorOutputs[x, y, z] = new SensorPerceiveOutput { HasHit = true, HitGameObjects = hitColliders.Select(a => a.gameObject).ToArray() };
                    }
                    else {
                        GridSensorOutputs[x, y, z] = new SensorPerceiveOutput { HasHit = false, HitGameObjects = null };
                    }
                }
            }
        }

        return GridSensorOutputs;
    }

    void OnDrawGizmosSelected() {
        Perceive();

        Vector3 cellWorldPosition = fixedPosition ? position : (transform.position + centerOffset);

        // Loop through each cell in the SensorPerceiveOutputs
        for (int x = 0; x < gridSize.x; x++) {
            for (int y = 0; y < gridSize.y; y++) {
                for (int z = 0; z < gridSize.z; z++) {
                    // Calculate the center of the current cell
                    Vector3 cellCenter = new Vector3(x * cellSpacing.x, y * cellSpacing.y, z * cellSpacing.z) + cellWorldPosition;

                    // Draw a wire cube at the cell's position with the cell's size
                    Gizmos.color = GridSensorOutputs[x, y, z] != null && GridSensorOutputs[x, y, z].HasHit ? Color.red : Color.white;
                    Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize.x, cellSize.y, cellSize.z));
                }
            }
        }
    }
}
