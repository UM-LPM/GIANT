using UnityEngine;

namespace Problems.MicroRTS
{
    public class MicroRTSProgressBar : MonoBehaviour
    {
        [Header("Progress Bar Settings")]
        [SerializeField] private float barWidth = 0.5f;
        [SerializeField] private float barHeight = 0.05f;
        [SerializeField] private float verticalOffset = 0.25f;
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Color fillColor = Color.blue;

        private MicroRTSBarRenderer barRenderer;

        void Awake()
        {
            barRenderer = gameObject.AddComponent<MicroRTSBarRenderer>();
            barRenderer.Initialize(barWidth, barHeight, fillColor, backgroundColor);
        }

        public void SetProgress(float progress)
        {
            if (barRenderer != null)
            {
                barRenderer.SetProgress(progress);
            }
        }

        public void SetVisible(bool visible)
        {
            if (barRenderer != null)
            {
                barRenderer.SetVisible(visible);
            }
        }

        public void SetColor(Color color)
        {
            if (barRenderer != null)
            {
                barRenderer.SetColor(color);
            }
        }

        public void SetPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition + Vector3.up * verticalOffset;
        }

        public float GetProgress()
        {
            if (barRenderer != null)
            {
                return barRenderer.GetProgress();
            }
            return 0f;
        }
    }
}
