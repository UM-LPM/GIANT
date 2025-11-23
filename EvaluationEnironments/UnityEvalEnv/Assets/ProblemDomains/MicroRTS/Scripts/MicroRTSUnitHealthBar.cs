using UnityEngine;

namespace Problems.MicroRTS
{
    public class MicroRTSUnitHealthBar : MonoBehaviour
    {
        [Header("Health Bar Settings")]
        [SerializeField] private float barWidth = 0.6f;
        [SerializeField] private float barHeight = 0.05f;
        [SerializeField] private float bottomOffset = -0.15f;
        [SerializeField] private Color healthBarColor = Color.red;
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private bool showAtFullHealth = false;

        private MicroRTSUnitComponent unitComponent;
        private MicroRTSBarRenderer healthBar;
        private bool isInitialized = false;

        void Awake()
        {
            unitComponent = GetComponent<MicroRTSUnitComponent>();
            if (unitComponent == null)
            {
                unitComponent = GetComponentInParent<MicroRTSUnitComponent>();
            }
        }

        void Start()
        {
            InitializeHealthBar();
        }

        void LateUpdate()
        {
            if (!isInitialized) return;
            UpdateHealthBar();
        }

        private void InitializeHealthBar()
        {
            if (unitComponent == null) return;

            GameObject barObj = new GameObject("HealthBar");
            barObj.transform.SetParent(transform, false);
            barObj.transform.localPosition = new Vector3(0f, bottomOffset, 0f);

            healthBar = barObj.AddComponent<MicroRTSBarRenderer>();
            healthBar.Initialize(barWidth, barHeight, healthBarColor, backgroundColor);
            healthBar.SetVisible(true);

            isInitialized = true;
            UpdateHealthBar();
        }

        private void UpdateHealthBar()
        {
            if (healthBar == null || unitComponent == null) return;

            int hitPoints = unitComponent.HitPoints;
            int maxHitPoints = unitComponent.MaxHitPoints;

            if (maxHitPoints <= 0) return;

            float healthPercentage = (float)hitPoints / maxHitPoints;
            bool isFullHealth = healthPercentage >= 1.0f;

            healthBar.SetProgress(healthPercentage);
            healthBar.SetColor(healthBarColor);

            if (isFullHealth && !showAtFullHealth)
            {
                healthBar.SetVisible(false);
            }
            else
            {
                healthBar.SetVisible(true);
            }
        }

        void OnDestroy()
        {
            if (healthBar != null && healthBar.gameObject != null)
            {
                Destroy(healthBar.gameObject);
            }
        }
    }
}

