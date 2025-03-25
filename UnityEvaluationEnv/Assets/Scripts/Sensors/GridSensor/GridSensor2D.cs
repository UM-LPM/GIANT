using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Tilemaps;
using UnityEngine;

public class GridSensor2D : Sensor<SensorPerceiveOutput[,]> {

    [Header("Grid sensor configuration")]
    [SerializeField] bool DiscretizeGridPos;
    [SerializeField] Vector2Int GridSize;
    [SerializeField] Vector2 CellSize;
    [SerializeField] Vector2 CellSpacing;
    [SerializeField] Vector2 CenterOffset;

    [Header("Sensor position")]
    [SerializeField] bool fixedPosition;
    [SerializeField] Vector2 position;

    public GridSensor2D() : base("Grid Sensor 2D") {

    }

    public override SensorPerceiveOutput[,] PerceiveAll() {
        Vector2 agentPos = DiscretizeGridPos? new Vector2(CustomRound(transform.position.x), CustomRound(transform.position.y)) : transform.position;
        SensorPerceiveOutputs = new SensorPerceiveOutput[GridSize.x, GridSize.y];

        Vector2 cellWorldPosition = fixedPosition ? position : (new Vector2(agentPos.x, agentPos.y) + CenterOffset);

        // Loop through each cell in the SensorPerceiveOutputs
        for (int x = 0; x < GridSize.x; x++) {
            for (int y = 0; y < GridSize.y; y++) {
                // Calculate the center of the current cell
                Vector2 cellCenter = new Vector2((x * CellSpacing.x), (y * CellSpacing.y)) + cellWorldPosition;

                // Use Physics.OverlapBox to detect objects in the cell
                Collider2D[] hitColliders = Physics2D.OverlapBoxAll(cellCenter, new Vector2(CellSize.x / 1.15f, CellSize.y / 1.15f), 0f, LayerMask);
                // If any objects were detected, store the first one in the SensorPerceiveOutputs array
                if (hitColliders.Length > 0 && hitColliders[0].gameObject != gameObject) {
                    SensorPerceiveOutputs[x, y] = new SensorPerceiveOutput { HasHit = true, HitGameObjects = hitColliders.Select(a => a.gameObject).ToArray(), EndPositionWorld = cellCenter };
                }
                else {
                    SensorPerceiveOutputs[x, y] = new SensorPerceiveOutput { HasHit = false, HitGameObjects = null, EndPositionWorld = cellCenter };
                }
            }
        }

        return SensorPerceiveOutputs;
    }

    public override SensorPerceiveOutput[,] PerceiveSingle(int xPos = -1, int yPos = -1, int zPos = -1)
    {
        throw new NotImplementedException();
    }

    public override SensorPerceiveOutput[,] PerceiveRange(int startIndex = -1, int endIndex = -1, int step = 1)
    {
        throw new NotImplementedException();
    }

    public override void Init()
    {

    }

    void OnDrawGizmosSelected() {
        if (DrawGizmos) {
            PerceiveAll();

            // Loop through each cell in the SensorPerceiveOutputs
            for (int x = 0; x < GridSize.x; x++) {
                for (int y = 0; y < GridSize.y; y++) {
                    // Draw a wire cube at the cell's position with the cell's size
                    if ((DrawOnlyHitSensors && SensorPerceiveOutputs[x, y].HasHit) || !DrawOnlyHitSensors) {
                        Gizmos.color = SensorPerceiveOutputs[x, y] != null && SensorPerceiveOutputs[x, y].HasHit ? HitSensorColor : BaseSensorColor;
                        Gizmos.DrawWireCube(SensorPerceiveOutputs[x, y].EndPositionWorld, new Vector2(CellSize.x, CellSize.y));
                    }
                }
            }
        }
    }

    private float CustomRound(float value) {
        return Mathf.RoundToInt(value);
    }
}
