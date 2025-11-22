using UnityEngine;
using UnityEngine.UI;

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

        private Canvas canvas;
        private RectTransform fillRect;
        private Camera mainCamera;
        private float currentProgress = 0f;
        private const float CANVAS_SCALE = 0.01f;

        void Awake()
        {
            mainCamera = Camera.main;
            SetupCanvas();
            SetupImages();
        }

        void Start()
        {
            if (canvas != null && canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }
        }

        void LateUpdate()
        {
            if (mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
            }
        }

        private void SetupCanvas()
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;
            canvas.worldCamera = Camera.main;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(barWidth * 100f, barHeight * 100f);
            canvasRect.localScale = Vector3.one * CANVAS_SCALE;
        }

        private void SetupImages()
        {
            Sprite whiteSprite = CreateWhiteSprite();

            CreateBackground(whiteSprite);
            CreateFill(whiteSprite);
        }

        private Sprite CreateWhiteSprite()
        {
            Texture2D whiteTexture = Texture2D.whiteTexture;
            return Sprite.Create(whiteTexture, new Rect(0, 0, whiteTexture.width, whiteTexture.height), new Vector2(0.5f, 0.5f));
        }

        private void CreateBackground(Sprite sprite)
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(transform, false);

            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.sprite = sprite;
            bgImage.color = backgroundColor;

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0f);
            bgRect.anchorMax = new Vector2(0f, 1f);
            bgRect.pivot = new Vector2(0f, 0.5f);
            bgRect.sizeDelta = new Vector2(barWidth * 100f, barHeight * 90f);
            bgRect.anchoredPosition = new Vector2(barWidth * 5f, 0f);
        }

        private void CreateFill(Sprite sprite)
        {
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(transform, false);

            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.sprite = sprite;
            fillImage.color = fillColor;

            fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.sizeDelta = new Vector2(0f, barHeight * 90f);
            fillRect.anchoredPosition = new Vector2(barWidth * 5f, 0f);
        }

        public void SetProgress(float progress)
        {
            currentProgress = Mathf.Clamp01(progress);
            if (fillRect != null)
            {
                float fillWidth = barWidth * 90f * currentProgress;
                fillRect.sizeDelta = new Vector2(fillWidth, fillRect.sizeDelta.y);
            }
        }

        public void SetVisible(bool visible)
        {
            if (canvas != null)
            {
                canvas.enabled = visible;
            }
        }

        public void SetColor(Color color)
        {
            fillColor = color;
            Image fillImage = fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = color;
            }
        }

        public void SetPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition + Vector3.up * verticalOffset;
        }

        public float GetProgress()
        {
            return currentProgress;
        }
    }
}
