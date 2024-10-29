using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Visualizer {
    public static void DisplayBox(Vector3 center, Vector3 HalfExtend, Quaternion rotation, float Duration = 0) {
        Vector3[] Vertices = new Vector3[8];
        int i = 0;
        for (int x = -1; x < 2; x += 2) {
            for (int y = -1; y < 2; y += 2) {
                for (int z = -1; z < 2; z += 2) {
                    Vertices[i] = center + new Vector3(HalfExtend.x * x, HalfExtend.y * y, HalfExtend.z * z);
                    i++;
                }
            }
        }

        Vertices = RotateObject(Vertices, rotation.eulerAngles, center);

        Debug.DrawLine(Vertices[0], Vertices[1], Color.white, Duration);
        Debug.DrawLine(Vertices[1], Vertices[3], Color.white, Duration);
        Debug.DrawLine(Vertices[2], Vertices[3], Color.white, Duration);
        Debug.DrawLine(Vertices[2], Vertices[0], Color.white, Duration);
        Debug.DrawLine(Vertices[4], Vertices[0], Color.white, Duration);
        Debug.DrawLine(Vertices[4], Vertices[6], Color.white, Duration);
        Debug.DrawLine(Vertices[2], Vertices[6], Color.white, Duration);
        Debug.DrawLine(Vertices[7], Vertices[6], Color.white, Duration);
        Debug.DrawLine(Vertices[7], Vertices[3], Color.white, Duration);
        Debug.DrawLine(Vertices[7], Vertices[5], Color.white, Duration);
        Debug.DrawLine(Vertices[1], Vertices[5], Color.white, Duration);
        Debug.DrawLine(Vertices[4], Vertices[5], Color.white, Duration);
    }

    static Vector3[] RotateObject(Vector3[] ObjToRotate, Vector3 DegreesToRotate, Vector3 Around)//rotates a set of dots counterclockwise
    {
        for (int i = 0; i < ObjToRotate.Length; i++) {
            ObjToRotate[i] -= Around;
        }
        DegreesToRotate.z = Mathf.Deg2Rad * DegreesToRotate.z;
        DegreesToRotate.x = Mathf.Deg2Rad * DegreesToRotate.x;
        DegreesToRotate.y = -Mathf.Deg2Rad * DegreesToRotate.y;

        for (int i = 0; i < ObjToRotate.Length; i++) {
            float H = Vector3.Distance(Vector3.zero, ObjToRotate[i]);
            if (H != 0) {
                float CosA = ObjToRotate[i].x / H;
                float SinA = ObjToRotate[i].y / H;
                float cosB = Mathf.Cos(DegreesToRotate.z);
                float SinB = Mathf.Sin(DegreesToRotate.z);
                ObjToRotate[i] = new Vector3(H * (CosA * cosB - SinA * SinB), H * (SinA * cosB + CosA * SinB), ObjToRotate[i].z);
            }
        }

        for (int i = 0; i < ObjToRotate.Length; i++) {
            float H = Vector3.Distance(Vector3.zero, ObjToRotate[i]);
            if (H != 0) {
                float CosA = ObjToRotate[i].y / H;
                float SinA = ObjToRotate[i].z / H;
                float cosB = Mathf.Cos(DegreesToRotate.x);
                float SinB = Mathf.Sin(DegreesToRotate.x);
                ObjToRotate[i] = new Vector3(ObjToRotate[i].x, H * (CosA * cosB - SinA * SinB), H * (SinA * cosB + CosA * SinB));
            }
        }

        for (int i = 0; i < ObjToRotate.Length; i++) {
            float H = Vector3.Distance(Vector3.zero, ObjToRotate[i]);
            if (H != 0) {
                float CosA = ObjToRotate[i].x / H;
                float SinA = ObjToRotate[i].z / H;
                float cosB = Mathf.Cos(DegreesToRotate.y);
                float SinB = Mathf.Sin(DegreesToRotate.y);
                ObjToRotate[i] = new Vector3((CosA * cosB - SinA * SinB) * H, ObjToRotate[i].y, H * (SinA * cosB + CosA * SinB));
            }
        }

        for (int i = 0; i < ObjToRotate.Length; i++) {
            ObjToRotate[i] += Around;
        }



        return ObjToRotate;
    }
}