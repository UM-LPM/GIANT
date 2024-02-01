using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;


public class VisionSensor : Sensor<SensorPerceiveOutput[]> {

    [Header("Vision Sensor Configuration")]
    [Range(0f, 100f)]
    [SerializeField] float Distance = 10f;
    [Range(0f, 180f)]
    [SerializeField] float Angle = 30f;
    [Range(0f, 10f)]
    [SerializeField] float StartHeight = 1f;
    [Range(0f, 10f)]
    [SerializeField] float EndHeight = 1f;
    [Range(1, 1000)]
    [SerializeField] int Segments = 10;
    [Range(0,1000)]
    [SerializeField] int ScanFrequency = 0;
    [SerializeField] Vector3 StartOffset;
    [SerializeField] Vector3 EndOffset;

    Collider[] colliders = new Collider[50]; // All objects that are inside the certain radius
    Mesh mesh;
    int count;
    float scanInterval;
    float scanTimer;

    MeshCollider meshCollider;

    public VisionSensor() : base ("Vision Sensor") { }

    private void Start() {
        scanInterval = ScanFrequency > 0 ? 1f / ScanFrequency : 0;

        if (gameObject.TryGetComponent<MeshCollider>(out meshCollider))
            meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = CreateWedgeMesh();
    }

    private void FixedUpdate() {
        if (ScanFrequency > 0) {
            scanTimer -= Time.fixedDeltaTime;
            if (scanTimer < 0) {
                scanTimer += scanInterval;
                Perceive();
            }
        }
    }


    public override SensorPerceiveOutput[] Perceive() {
        if (ScanFrequency <= 0) {
            List<SensorPerceiveOutput> outputs = new List<SensorPerceiveOutput>();

            count = Physics.OverlapSphereNonAlloc(transform.position, Distance, colliders, LayerMask, QueryTriggerInteraction.Collide);

            outputs.Clear();
            for (int i = 0; i < count; i++) {
                GameObject obj = colliders[i].gameObject;
                if (IsInSight(obj)) {
                    outputs.Add(new SensorPerceiveOutput {
                        HasHit = true,
                        StartPositionWorld = obj.transform.position,
                        HitGameObjects = new GameObject[] { obj },
                    });
                }
            }

            SensorPerceiveOutputs = outputs.ToArray();
        }

        return SensorPerceiveOutputs == null ? new SensorPerceiveOutput[0] : SensorPerceiveOutputs.ToArray();
    }

    bool IsInSight(GameObject obj) {
        bool isInSight = false;

        Bounds bounds = obj.GetComponent<Collider>().bounds;
        Vector3 halfExtents = bounds.extents;

        Collider[] colliders = Physics.OverlapBox(obj.transform.position, halfExtents, obj.transform.rotation);
        foreach (Collider collider in colliders) {
            VisionSensor visionSensor = collider.GetComponent<VisionSensor>();
            if(visionSensor != null && visionSensor == this && obj != gameObject && collider.isTrigger && collider is MeshCollider) {
                isInSight = true;
            }
        }

        return isInSight;
    }

    Mesh CreateWedgeMesh() {
        mesh = new Mesh();

        int numTriangles = (Segments * 4) + 2 + 2;
        int numVertices = numTriangles * 3;

        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];

        Vector3 bottomCenter = Vector3.zero - StartOffset;
        Vector3 bottomLeft = (Quaternion.Euler(0, -Angle, 0) * Vector3.forward * Distance) - EndOffset;
        Vector3 bottomRight = (Quaternion.Euler(0, Angle, 0) * Vector3.forward * Distance) - EndOffset;

        Vector3 topCenter = bottomCenter + Vector3.up * StartHeight;
        Vector3 topRight = bottomRight + Vector3.up * EndHeight;
        Vector3 topLeft = bottomLeft + Vector3.up * EndHeight;

        int vert = 0;

        // left side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = bottomLeft;
        vertices[vert++] = topLeft;

        vertices[vert++] = topLeft;
        vertices[vert++] = topCenter;
        vertices[vert++] = bottomCenter;

        // right side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = topCenter;
        vertices[vert++] = topRight;

        vertices[vert++] = topRight;
        vertices[vert++] = bottomRight;
        vertices[vert++] = bottomCenter;

        float currentAngle = -Angle;
        float deltaAngle = (Angle * 2) / Segments;

        for (int i = 0; i < Segments; i++) {
            bottomLeft = (Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * Distance) - EndOffset;
            bottomRight = (Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * Distance) - EndOffset;

            topRight = bottomRight + Vector3.up * EndHeight;
            topLeft = bottomLeft + Vector3.up * EndHeight;

            // far side
            vertices[vert++] = bottomLeft;
            vertices[vert++] = bottomRight;
            vertices[vert++] = topRight;

            vertices[vert++] = topRight;
            vertices[vert++] = topLeft;
            vertices[vert++] = bottomLeft;


            // top
            vertices[vert++] = topCenter;
            vertices[vert++] = topLeft;
            vertices[vert++] = topRight;

            // bottom
            vertices[vert++] = bottomCenter;
            vertices[vert++] = bottomRight;
            vertices[vert++] = bottomLeft;

            currentAngle += deltaAngle;
        }

        for (int i = 0; i < numVertices; i++) {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private void OnValidate() {
        if(meshCollider == null) {
            gameObject.TryGetComponent<MeshCollider>(out meshCollider);
            if (meshCollider == null)
                meshCollider = gameObject.AddComponent<MeshCollider>();
        }

        mesh = CreateWedgeMesh();
        scanInterval = 1f / ScanFrequency;

        meshCollider.sharedMesh = mesh;
        meshCollider.convex = true;
        meshCollider.isTrigger = true;
    }

    private void OnDrawGizmosSelected() {
        if (DrawGizmos) {
            Perceive();
            if (mesh) {
                Gizmos.color = BaseSensorColor;
                Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
            }

            if (SensorPerceiveOutputs != null) {
                Gizmos.color = HitSensorColor;
                for (int i = 0; i < SensorPerceiveOutputs.Length; i++) {
                    Gizmos.DrawSphere(SensorPerceiveOutputs[i].HitGameObjects[0].transform.position, 1f);
                }
            }
        }
    }

}