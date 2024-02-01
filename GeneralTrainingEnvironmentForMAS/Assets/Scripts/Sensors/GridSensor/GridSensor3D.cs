using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GridSensor3D : Sensor<SensorPerceiveOutput[,,]> {

    [Header("Base configuration")]
    [SerializeField] Vector3Int GridSize;
    [SerializeField] Vector3 CellSize;
    [SerializeField] Vector3 CellSpacing;
    [SerializeField] Vector3 CenterOffset;

    [Header("Sensor position")]
    [SerializeField] bool fixedPosition;
    [SerializeField] Vector3 position;

    public GridSensor3D() : base("Grid Sensor 3D") {

    }

    public override SensorPerceiveOutput[,,] Perceive() {
        // Create a new 3D array to store the detected objects
        SensorPerceiveOutputs = new SensorPerceiveOutput[GridSize.x, GridSize.y, GridSize.z];

        Vector3 cellWorldPosition = fixedPosition ? position : (transform.position + CenterOffset);

        // Loop through each cell in the SensorPerceiveOutputs
        for (int x = 0; x < GridSize.x; x++) {
            for (int y = 0; y < GridSize.y; y++) {
                for (int z = 0; z < GridSize.z; z++) {
                    // Calculate the center of the current cell
                    Vector3 cellCenter = new Vector3(x * CellSpacing.x, y * CellSpacing.y, z * CellSpacing.z) + cellWorldPosition;

                    // Use Physics.OverlapBox to detect objects in the cell
                    Collider[] hitColliders = Physics.OverlapBox(cellCenter, new Vector3(CellSize.x / 2, CellSize.y / 2, CellSize.z / 2), Quaternion.identity, LayerMask);
                    // If any objects were detected, store the first one in the SensorPerceiveOutputs array
                    if (hitColliders.Length > 0 && hitColliders[0].gameObject != gameObject) {
                        SensorPerceiveOutputs[x, y, z] = new SensorPerceiveOutput { HasHit = true, HitGameObjects = hitColliders.Select(a => a.gameObject).ToArray(), EndPositionWorld = cellCenter };
                    }
                    else {
                        SensorPerceiveOutputs[x, y, z] = new SensorPerceiveOutput { HasHit = false, HitGameObjects = null, EndPositionWorld = cellCenter };
                    }
                }
            }
        }

        return SensorPerceiveOutputs;
    }

    void OnDrawGizmosSelected() {
        if (DrawGizmos) {
            Perceive();

            // Loop through each cell in the SensorPerceiveOutputs
            for (int x = 0; x < GridSize.x; x++) {
                for (int y = 0; y < GridSize.y; y++) {
                    for (int z = 0; z < GridSize.z; z++) {
                        if ((DrawOnlyHitSensors && SensorPerceiveOutputs[x, y, z].HasHit) || !DrawOnlyHitSensors) {
                            // Draw a wire cube at the cell's position with the cell's size
                            Gizmos.color = SensorPerceiveOutputs[x, y, z] != null && SensorPerceiveOutputs[x, y, z].HasHit ? Color.red : Color.white;
                            Gizmos.DrawWireCube(SensorPerceiveOutputs[x, y, z].EndPositionWorld, new Vector3(CellSize.x, CellSize.y, CellSize.z));
                        }
                    }
                }
            }
        }
    }
}
