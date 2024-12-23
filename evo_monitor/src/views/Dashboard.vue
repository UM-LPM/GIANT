<script setup lang="ts">
  import LineChart from "@/components/LineChart.vue";
  import PieChart from "@/components/PieChart.vue";
  import RadarChart from "@/components/RadarChart.vue";
  import BarChart from "@/components/BarChart.vue";
  import type { ChartConfig } from "@/models/chartConfig";
  import { onMounted, reactive } from "vue";
  import { useGpAlgorithProgressDataStore } from "../stores/gpAlgorithmProgressData";

  const options = reactive({
    configurationNum: 1,
    maxConfigurationNum: 0,
    runNum: 1,
    maxRunNum: 0
  });

  const lineChartConfig: ChartConfig = reactive({
    id: 'line-chart-1',
    data: {
      labels: ['0', '1', '2', '3', '4', '5', '6'],
      datasets: [
          {
          label: 'Best Individual',
          backgroundColor: '#00ff00',
          data: [80, 39, 50, 70, 39, 25, 82]
          },
          {
          label: 'Average Individual',
          backgroundColor: '#00ffff',
          data: [40, 12, 24, 50, 29, 20, 80]
          },
          {
          label: 'Worst Individual',
          backgroundColor: '#ff0000',
          data: [30, 10, 12, 13, 23, 15, 60]
          }
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false
    }
  });

  const barChartConfig: ChartConfig = reactive({
    id: 'bar-chart-1',
    data: {
      labels: ['January','February','March','April','May','June','July','August','September','October','November','December'
        ],
      datasets: [
            {
            label: 'Data One',
            backgroundColor: '#f87979',
            data: [40, 20, 12, 39, 10, 40, 39, 80, 40, 20, 12, 11]
            }
        ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false
    }
  });

  const radarChartConfig: ChartConfig = reactive({
    id: 'radar-chart-1',
    data: {
      labels: [
            'SectorExploration',
            'PowerUp_Pickup_Health',
            'PowerUp_Pickup_Ammo',
            'PowerUp_Pickup_Shield',
            'MissilesFired',
            'MissilesFiredAccuracy',
            'SurvivalBonus',
            'OpponentTrackingBonus',
            'OpponentDestroyedBonus',
            'DamageTakenPenalty',
        ],
        datasets: [
            {
            label: 'My First dataset',
            backgroundColor: 'rgba(179,181,198,0.2)',
            borderColor: 'rgba(179,181,198,1)',
            pointBackgroundColor: 'rgba(179,181,198,1)',
            pointBorderColor: '#fff',
            pointHoverBackgroundColor: '#fff',
            pointHoverBorderColor: 'rgba(179,181,198,1)',
            data: [65, 59, 90, 81, 56, 55, 40, 34, 53, 62],
            },
            {
            label: 'My Second dataset',
            backgroundColor: 'rgba(255,99,132,0.2)',
            borderColor: 'rgba(255,99,132,1)',
            pointBackgroundColor: 'rgba(255,99,132,1)',
            pointBorderColor: '#fff',
            pointHoverBackgroundColor: '#fff',
            pointHoverBorderColor: 'rgba(255,99,132,1)',
            data: [28, 48, 40, 19, 96, 27, 100, 34, 83, 22],
            },
        ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false
    }
  });

  const pieChartConfig: ChartConfig = reactive({
    id: 'pie-chart-1',
    data: {
      labels: ['VueJs', 'EmberJs', 'ReactJs', 'AngularJs'],
        datasets: [
            {
            backgroundColor: ['#41B883', '#E46651', '#00D8FF', '#DD1B16'],
            data: [40, 20, 80, 10]
            }
        ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false
    }
  });

  const validationInputErrors = reactive({});

  const gpAlgorithProgressDataStore = useGpAlgorithProgressDataStore();
  onMounted(() => {
    // 1. Load evolutionary algorithm progress data from .json file -> Read data every X seconds (or just on load?)
    loadData();

    // 2. fill chartConfig with data

    // 3. Display data in the charts
  });

  const loadData = async () => {
    // Load data from .json file
    await gpAlgorithProgressDataStore.fetchGpAlgorithmProgressDataFromJsonFile();
    
    options.maxConfigurationNum = gpAlgorithProgressDataStore.gpAlgorithProgressData.multiConfigurationProgressData.length;
    options.maxRunNum = gpAlgorithProgressDataStore.gpAlgorithProgressData.multiConfigurationProgressData[options.configurationNum - 1].multiRunProgressData.length;
  }

  const validateInput = (val, min, max, key) => {
        if (val < min || val > max) {
          // Save the error message
          validationInputErrors[key] = `${key} must be between ${min} and ${max}!`;

          return false;
        }
        // Remove the error message
        delete validationInputErrors[key];

        reloadChartDatasets();
        
        return true
  }

  const validateInputRules = (min, max, key) => [
    (val) => validateInput(val, min, max, key)
  ];

  const reloadChartDatasets = () => {
    // Reset chart data
    lineChartConfig.data = gpAlgorithProgressDataStore.reloadLineChartDatasetBestIndividual(options.configurationNum - 1, options.runNum - 1);

  }
</script>

<template>
  <q-page class="q-pa-sm">
    <div class="row">
      <div class="col-4 col-md-2 q-pa-sm">
        <q-btn
          color="primary"
          label="Load Progress Data"
          @click="loadData">
        </q-btn>
      </div>
      <div class="col-4 col-md-1 q-pa-sm">
        <q-field borderless stack-label dense>
          <template v-slot:control>
            <div class="self-center full-width no-outline" tabindex="0">Configuration:</div>
          </template>
        </q-field>
      </div>
      <div class="col-4 col-md-1 q-pa-sm">
        <q-input filled type="number" label="" dense
          v-model.number="options.configurationNum"
          :rules="validateInputRules(1, options.maxConfigurationNum, 'configurationNum')"
          hide-bottom-space
        ></q-input>
      </div>
      <div class="col-4 col-md-1 q-pa-sm">
        <q-field borderless stack-label dense>
          <template v-slot:control>
            <div class="self-center full-width no-outline" tabindex="0">Run:</div>
          </template>
        </q-field>
      </div>
      <div class="col-4 col-md-1 q-pa-sm">
        <q-input filled type="number" label="" dense
          v-model.number="options.runNum"
          :rules="validateInputRules(1, options.maxRunNum, 'runNum')"
          hide-bottom-space
        ></q-input>
      </div>
    </div>

    <div class="row" v-if="Object.keys(validationInputErrors).length > 0">
      <div class="col-4 col-md-12 q-pa-sm">
        <q-field v-for="(value, index) in Object.keys(validationInputErrors)" :key="index" borderless stack-label dense color="negative">
          {{ validationInputErrors[value] }}
        </q-field>
      </div>
    </div>

    <div><LineChart :chart-config="lineChartConfig"/></div>

    <q-separator class="q-ma-sm"></q-separator>

    <div><RadarChart :chart-config="radarChartConfig" /></div>

    <q-separator class="q-ma-sm"></q-separator>

    <div><PieChart :chart-config="pieChartConfig" /></div>

    <q-separator class="q-ma-sm"></q-separator>

    <div><BarChart :chart-config="barChartConfig" /></div>
    
  </q-page>
</template>
