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
        [SerializeField] Image HealthBar;
        [SerializeField] Image EnergyBar;

        private void Awake()
        {
            StatBarsCanvas.worldCamera = Camera.main;
        }

        private void Update()
        {

        }

        public void SetStats(float health, float energy)
        {
            HealthBar.fillAmount = health / Moba_gameEnvironmentController.MAX_HEALTH;
            EnergyBar.fillAmount = energy / Moba_gameEnvironmentController.MAX_ENERGY;
        }
    }
}