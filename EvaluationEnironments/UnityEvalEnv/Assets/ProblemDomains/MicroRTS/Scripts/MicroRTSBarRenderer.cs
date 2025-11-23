using UnityEngine;
using UnityEngine.UI;
using Problems.MicroRTS.Core;

namespace Problems.MicroRTS
{
    public class MicroRTSBarRenderer : MonoBehaviour
    {
        private Canvas canvas;
        private RectTransform fillRect;
        private RectTransform backgroundRect;
        private Camera mainCamera;
        private float currentProgress = 0f;
        private float barWidth = 0.5f;
        private float barHeight = 0.05f;
        private Color fillColor = Color.blue;
        private Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);
        private const float CANVAS_SCALE = 0.01f;
        private bool isMovementBar = false;
        private int currentDirection = -1;

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

        public void Initialize(float width, float height, Color fill, Color background)
        {
            barWidth = width;
            barHeight = height;
            fillColor = fill;
            backgroundColor = background;

            mainCamera = Camera.main;
            SetupCanvas();
            SetupImages();
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

            backgroundRect = bgObj.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0f, 0f);
            backgroundRect.anchorMax = new Vector2(0f, 1f);
            backgroundRect.pivot = new Vector2(0f, 0.5f);
            backgroundRect.sizeDelta = new Vector2(barWidth * 100f, barHeight * 90f);
            backgroundRect.anchoredPosition = new Vector2(barWidth * 5f, 0f);
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

        public void SetProgress(float progress, bool isMovement = false, int direction = -1)
        {
            currentProgress = Mathf.Clamp01(progress);
            isMovementBar = isMovement;
            currentDirection = direction;

            if (fillRect != null)
            {
                if (isMovementBar && direction >= 0)
                {
                    UpdateMovementBarProgress();
                }
                else
                {
                    float fillWidth = barWidth * 90f * currentProgress;
                    fillRect.sizeDelta = new Vector2(fillWidth, fillRect.sizeDelta.y);
                    fillRect.anchoredPosition = new Vector2(barWidth * 5f, 0f);
                }
            }
        }

        private void UpdateMovementBarProgress()
        {
            float tileSize = 1f;
            var parent = GetComponentInParent<MicroRTSEnvironmentController>();
            if (parent != null)
            {
                var walkableTilemap = parent.GetComponentInChildren<UnityEngine.Tilemaps.Tilemap>();
                if (walkableTilemap != null)
                {
                    tileSize = walkableTilemap.cellSize.x;
                }
            }

            float startOffset = tileSize / 6f;
            float fullBarLength = tileSize;
            float progressLength = fullBarLength * currentProgress;

            float thickness = barHeight * 90f;
            float startPosition = startOffset * 100f;
            float fillLength = progressLength * 100f;
            float bgLength = fullBarLength * 100f;

            Vector2 anchoredPos = Vector2.zero;
            Vector2 sizeDelta = Vector2.zero;
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Vector2 anchorMin = new Vector2(0.5f, 0.5f);
            Vector2 anchorMax = new Vector2(0.5f, 0.5f);

            switch (currentDirection)
            {
                case MicroRTSUtils.DIRECTION_UP:
                    sizeDelta = new Vector2(thickness, fillLength);
                    anchoredPos = new Vector2(0f, startPosition);
                    pivot = new Vector2(0.5f, 0f);
                    break;

                case MicroRTSUtils.DIRECTION_RIGHT:
                    sizeDelta = new Vector2(fillLength, thickness);
                    anchoredPos = new Vector2(startPosition, 0f);
                    pivot = new Vector2(0f, 0.5f);
                    break;

                case MicroRTSUtils.DIRECTION_DOWN:
                    sizeDelta = new Vector2(thickness, fillLength);
                    anchoredPos = new Vector2(0f, -startPosition);
                    pivot = new Vector2(0.5f, 1f);
                    break;

                case MicroRTSUtils.DIRECTION_LEFT:
                    sizeDelta = new Vector2(fillLength, thickness);
                    anchoredPos = new Vector2(-startPosition, 0f);
                    pivot = new Vector2(1f, 0.5f);
                    break;

                default:
                    sizeDelta = new Vector2(fillLength, thickness);
                    anchoredPos = new Vector2(startPosition, 0f);
                    pivot = new Vector2(0f, 0.5f);
                    break;
            }

            fillRect.anchorMin = anchorMin;
            fillRect.anchorMax = anchorMax;
            fillRect.pivot = pivot;
            fillRect.sizeDelta = sizeDelta;
            fillRect.anchoredPosition = anchoredPos;

            if (backgroundRect != null)
            {
                Vector2 bgSizeDelta = Vector2.zero;
                Vector2 bgPivot = pivot;
                Vector2 bgAnchorMin = anchorMin;
                Vector2 bgAnchorMax = anchorMax;
                Vector2 bgAnchoredPos = anchoredPos;

                switch (currentDirection)
                {
                    case MicroRTSUtils.DIRECTION_UP:
                        bgSizeDelta = new Vector2(thickness, bgLength);
                        bgPivot = new Vector2(0.5f, 0f);
                        bgAnchoredPos = new Vector2(0f, startPosition);
                        break;
                    case MicroRTSUtils.DIRECTION_RIGHT:
                        bgSizeDelta = new Vector2(bgLength, thickness);
                        bgPivot = new Vector2(0f, 0.5f);
                        bgAnchoredPos = new Vector2(startPosition, 0f);
                        break;
                    case MicroRTSUtils.DIRECTION_DOWN:
                        bgSizeDelta = new Vector2(thickness, bgLength);
                        bgPivot = new Vector2(0.5f, 1f);
                        bgAnchoredPos = new Vector2(0f, -startPosition);
                        break;
                    case MicroRTSUtils.DIRECTION_LEFT:
                        bgSizeDelta = new Vector2(bgLength, thickness);
                        bgPivot = new Vector2(1f, 0.5f);
                        bgAnchoredPos = new Vector2(-startPosition, 0f);
                        break;
                    default:
                        bgSizeDelta = new Vector2(bgLength, thickness);
                        bgPivot = new Vector2(0f, 0.5f);
                        bgAnchoredPos = new Vector2(startPosition, 0f);
                        break;
                }

                backgroundRect.anchorMin = bgAnchorMin;
                backgroundRect.anchorMax = bgAnchorMax;
                backgroundRect.pivot = bgPivot;
                backgroundRect.sizeDelta = bgSizeDelta;
                backgroundRect.anchoredPosition = bgAnchoredPos;
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

        public void SetVisible(bool visible)
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }

            if (canvas != null)
            {
                canvas.enabled = visible;
                if (visible && canvas.worldCamera == null)
                {
                    canvas.worldCamera = Camera.main;
                }
            }
        }

        public void UpdateSize(float width, float height)
        {
            barWidth = width;
            barHeight = height;

            if (canvas != null)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    canvasRect.sizeDelta = new Vector2(barWidth * 100f, barHeight * 100f);
                }
            }

            Transform bgTransform = transform.Find("Background");
            if (bgTransform != null)
            {
                RectTransform bgRect = bgTransform.GetComponent<RectTransform>();
                if (bgRect != null)
                {
                    bgRect.sizeDelta = new Vector2(barWidth * 100f, barHeight * 90f);
                    bgRect.anchoredPosition = new Vector2(barWidth * 5f, 0f);
                }
            }

            if (fillRect != null)
            {
                if (isMovementBar && currentDirection >= 0)
                {
                    UpdateMovementBarProgress();
                }
                else
                {
                    float fillWidth = barWidth * 90f * currentProgress;
                    fillRect.sizeDelta = new Vector2(fillWidth, barHeight * 90f);
                    fillRect.anchoredPosition = new Vector2(barWidth * 5f, 0f);
                }
            }
        }

        public float GetProgress()
        {
            return currentProgress;
        }
    }
}

