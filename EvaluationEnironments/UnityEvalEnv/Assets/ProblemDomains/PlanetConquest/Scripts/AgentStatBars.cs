using UnityEngine;
using UnityEngine.UI;

namespace Problems.PlanetConquest
{
    public class AgentStatBars : MonoBehaviour
    {
        [SerializeField] Canvas StatBarsCanvas;
        [SerializeField] float StatBarsCanvasBaseRotattion = -90f;
        [SerializeField] Image HealthBar;
        [SerializeField] Image EnergyBar;

        private void Awake()
        {
            StatBarsCanvas.worldCamera = Camera.main;
        }

        private void Update()
        {
            StatBarsCanvas.transform.rotation = Quaternion.Euler(StatBarsCanvasBaseRotattion, transform.rotation.y * -1, transform.rotation.z);
        }

        public void SetStats(float health, float energy)
        {
            HealthBar.fillAmount = health / PlanetConquestEnvironmentController.MAX_HEALTH;
            EnergyBar.fillAmount = energy / PlanetConquestEnvironmentController.MAX_ENERGY;
        }
    }
}