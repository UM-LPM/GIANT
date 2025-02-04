using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Problems.Moba_game
{
    public class AgentStatBars : MonoBehaviour
    {
        [SerializeField] Canvas StatBarsCanvas;
        [SerializeField] float StatBarsCanvasBaseRotattion = -90f;
        [SerializeField] Image HealthBar;
        [SerializeField] Image ShieldBar;
        [SerializeField] Image AmmoBar;
        [SerializeField] Image EnvironmentColor;

        private void Awake()
        {
            StatBarsCanvas.worldCamera = Camera.main;
        }

        private void Update()
        {
            StatBarsCanvas.transform.rotation = Quaternion.Euler(StatBarsCanvasBaseRotattion, transform.rotation.y * -1, transform.rotation.z);

        }

        public void SetStats(float health, float shield, float ammo)
        {
            HealthBar.fillAmount = health / Moba_gameEnvironmentController.MAX_HEALTH;
            ShieldBar.fillAmount = shield / Moba_gameEnvironmentController.MAX_SHIELD;
            AmmoBar.fillAmount = ammo / Moba_gameEnvironmentController.MAX_AMMO;
        }

        public void SetEnvironmentColor(Color color)
        {
            EnvironmentColor.color = color;
        }
    }
}