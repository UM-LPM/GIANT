using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

public class CameraSensor : Sensor<Texture> {

    [Header("Camera Sensor Configuration")]
    [SerializeField] Camera Camera;
    [Range(10, 1000)]
    [SerializeField] int Width;
    [Range(10, 1000)]
    [SerializeField] int Height;
    [SerializeField] bool Grayscale;

    [HideInInspector] public Texture2D Texture;

    public CameraSensor() : base("Camera Sensor") { }

    public override Texture Perceive() {
        Texture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        Camera.cullingMask = LayerMask;

        // Create a new RenderTexture with the desired resolution
        RenderTexture rt = new RenderTexture(Width, Height, 24);
        Camera.targetTexture = rt;

        // Render the camera's view to the RenderTexture
        Camera.Render();

        // Read pixels to the Texture2D
        RenderTexture.active = rt;
        Texture2D tempTexture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tempTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tempTexture.Apply();

        // Resize the texture
        Texture = Resize(tempTexture, Width, Height);

        // Clean up
        RenderTexture.active = null;
        Camera.targetTexture = null;
        if(Application.isPlaying) {
            Destroy(rt);
            Destroy(tempTexture);
        }

        if (Grayscale) {
            Texture = ConvertToGrayscale(Texture);
        }

        return Texture;
    }


    private Texture2D Resize(Texture2D source, int newWidth, int newHeight) {
        source.filterMode = FilterMode.Bilinear;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Bilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D nTex = new Texture2D(newWidth, newHeight);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        nTex.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return nTex;
    }

    private Texture2D ConvertToGrayscale(Texture2D texture) {
        Color32[] pixels = texture.GetPixels32();
        for (int i = 0; i < pixels.Length; i++) {
            Color32 pixel = pixels[i];
            byte gray = (byte)(0.3f * pixel.r + 0.59f * pixel.g + 0.11f * pixel.b);
            pixels[i] = new Color32(gray, gray, gray, pixel.a);
        }
        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }

    private void OnDrawGizmosSelected() {
        if(DrawGizmos) {
            Perceive();
        }
    }
}