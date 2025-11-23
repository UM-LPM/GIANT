using UnityEngine;
using Problems.MicroRTS.Core;

namespace Problems.MicroRTS
{
    public class MicroRTSProgressBar : MonoBehaviour
    {
        [Header("Progress Bar Settings")]
        [SerializeField] private float barWidth = 0.5f;
        [SerializeField] private float barHeight = 0.05f;
        [SerializeField] private float verticalOffset = 0.0f;
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Color fillColor = Color.blue;

        private MicroRTSBarRenderer barRenderer;
        private MicroRTSEnvironmentController environmentController;
        private int currentDirection = -1;
        private bool isMovementBar = false;

        void Awake()
        {
            barRenderer = gameObject.AddComponent<MicroRTSBarRenderer>();
            barRenderer.Initialize(barWidth, barHeight, fillColor, backgroundColor);
            environmentController = GetComponentInParent<MicroRTSEnvironmentController>();
            if (environmentController == null)
            {
                environmentController = FindFirstObjectByType<MicroRTSEnvironmentController>();
            }
        }

        public void SetProgress(float progress)
        {
            if (barRenderer != null)
            {
                barRenderer.SetProgress(progress, isMovementBar, currentDirection);
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

        public void SetPosition(Vector3 worldPosition, int direction = -1, bool isMovement = false)
        {
            currentDirection = direction;
            isMovementBar = isMovement;

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
