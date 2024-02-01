using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraSensor))]
public class CameraSensorEditor : Editor {
    public override void OnInspectorGUI() {
        CameraSensor cameraSensor = (CameraSensor)target;

        // Draw default inspector property editor
        DrawDefaultInspector();

        if (cameraSensor.DrawGizmos) {
            // Draw the texture
            if (cameraSensor.Texture != null) {
                float TargetScale = 256 / cameraSensor.Texture.width;
                // Create a scaled version of the texture for display
                Texture2D scaledTexture = ScaleTexture(cameraSensor.Texture, TargetScale);

                // Display the scaled texture
                GUILayout.Label(scaledTexture);

                // Clean up
                DestroyImmediate(scaledTexture);
            }
        }
    }

    private Texture2D ScaleTexture(Texture2D source, float scaleFactor) {
        int targetWidth = Mathf.RoundToInt(source.width * scaleFactor);
        int targetHeight = Mathf.RoundToInt(source.height * scaleFactor);
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
        float incX = (float)source.width / targetWidth;
        float incY = (float)source.height / targetHeight;
        for (int y = 0; y < targetHeight; y++) {
            for (int x = 0; x < targetWidth; x++) {
                int oldX = Mathf.FloorToInt(x * incX);
                int oldY = Mathf.FloorToInt(y * incY);
                result.SetPixel(x, y, source.GetPixel(oldX, oldY));
            }
        }
        result.Apply();
        return result;
    }


}
