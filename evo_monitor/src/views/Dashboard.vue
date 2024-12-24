<script setup lang="ts">
  import LineChart from "@/components/LineChart.vue";
  import PieChart from "@/components/PieChart.vue";
  import RadarChart from "@/components/RadarChart.vue";
  import BarChart from "@/components/BarChart.vue";
  import type { ChartConfig } from "@/models/chartConfig";
  import { onMounted, reactive, ref } from "vue";
  import { useGpAlgorithProgressDataStore } from "../stores/gpAlgorithmProgressData";
import { ChartComponentRef } from "vue-chartjs";

  const lineChartBestIndividualRef = ref<ChartComponentRef>(null)
  const lineChartConfigBestIndividual: ChartConfig = reactive({
    id: 'line-chart-1',
    data: {
      labels: [],
      datasets: [],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      parsing: {
        xAxisKey: 'generation',
        yAxisKey: 'fitness'
      },
    }
  });

  const lineChartConfigBestIndividualRank: ChartConfig = reactive({
    id: 'line-chart-1',
    data: {
      labels: [],
      datasets: [],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      parsing: {
        xAxisKey: 'generation',
        yAxisKey: 'fitness'
      },
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
  });

  const loadData = async () => {
    // Load data from .json file
    await gpAlgorithProgressDataStore.fetchGpAlgorithmProgressDataFromJsonFile();
    
    // 2. Update input fields
    gpAlgorithProgressDataStore.setMaxConfigurationNum(gpAlgorithProgressDataStore.gpAlgorithProgressData.multiConfigurationProgressData.length);
    gpAlgorithProgressDataStore.setMaxRunNum(gpAlgorithProgressDataStore.gpAlgorithProgressData.multiConfigurationProgressData[gpAlgorithProgressDataStore.configurationNum - 1].multiRunProgressData.length);
    // 3. Reload chart data
    reloadLineChartBestIndividualDatasets();
  }

  const validateInput = (val, min, max, key) => {
        if (val < min || val > max) {
          // Save the error message
          validationInputErrors[key] = `${key} must be between ${min} and ${max}!`;

          return false;
        }
        // Remove the error message
        delete validationInputErrors[key];

        reloadLineChartBestIndividualDatasets();
        
        return true
  }

  const validateInputRules = (min, max, key) => [
    (val) => validateInput(val, min, max, key)
  ];

  const reloadLineChartBestIndividualDatasets = () => {
    // Reset chart data
    console.log("Reloading chart data", gpAlgorithProgressDataStore.configurationNum, gpAlgorithProgressDataStore.runNum);
    lineChartConfigBestIndividual.data = gpAlgorithProgressDataStore.reloadLineChartDatasetBestIndividual(gpAlgorithProgressDataStore.configurationNum - 1, gpAlgorithProgressDataStore.runNum - 1);
    lineChartConfigBestIndividualRank.data = gpAlgorithProgressDataStore.reloadLineChartDatasetBestIndividualRank(gpAlgorithProgressDataStore.configurationNum - 1, gpAlgorithProgressDataStore.runNum - 1);
  }

  const reloadRadarChartSelectedIndividualsConfig = () => {
    radarChartConfig.data = gpAlgorithProgressDataStore.reloadRadarChartSelectedIndividualsConfig();
  }

  const lineChartBestIndividualClick = (element) => {
    console.log(element);
    if(element != null) {
      // Check if this element is already selected
      let index = gpAlgorithProgressDataStore.selectedElements.findIndex((el) => el.generationNum == element.index && el.configurationNum == gpAlgorithProgressDataStore.configurationNum && el.runNum == gpAlgorithProgressDataStore.runNum && el.value == element.value);

      if(index > -1) {
        // Remove the element
        gpAlgorithProgressDataStore._selectedElements.splice(index, 1);
      } else {
        // Add the element
        gpAlgorithProgressDataStore._selectedElements.push({
          configurationNum: gpAlgorithProgressDataStore.configurationNum,
          runNum: gpAlgorithProgressDataStore.runNum,
          generationNum: element.index,
          label: element.label,
          value: element.value
        });
      }

      gpAlgorithProgressDataStore._lastSelectedElement = {
        configurationNum: gpAlgorithProgressDataStore.configurationNum,
        runNum: gpAlgorithProgressDataStore.runNum,
        generationNum: element.index,
        label: element.label,
        value: element.value
      }

      // Reset RadarChart config
      reloadRadarChartSelectedIndividualsConfig();

      console.log(gpAlgorithProgressDataStore.selectedElements);
    }
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
          v-model.number="gpAlgorithProgressDataStore._configurationNum"
          :rules="validateInputRules(1, gpAlgorithProgressDataStore.maxConfigurationNum, 'configurationNum')"
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
          v-model.number="gpAlgorithProgressDataStore._runNum"
          :rules="validateInputRules(1, gpAlgorithProgressDataStore.maxRunNum, 'runNum')"
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

    <div class="row">
      <div class="col-4 col-md-12 q-pa-sm">
        <h5 class="q-pa-sm q-ma-xs">Progress based on individual fitness (For Individual Match)</h5>
      </div>
      <div class="col-4 col-md-12 q-pa-sm">
        <LineChart :chart-config="lineChartConfigBestIndividual" @lineChartClick="lineChartBestIndividualClick" ref="lineChartBestIndividualRef"/>       
      </div>
    </div>

    <div class="row">
      <div class="col-4 col-md-12 q-pa-sm">
        <h5 class="q-pa-sm q-ma-xs">Progress based on individual Rating</h5>
      </div>
      <div class="col-4 col-md-12 q-pa-sm">
        <LineChart :chart-config="lineChartConfigBestIndividualRank"/>
      </div>
    </div>

    <div class="row">
      <div class="col-4 col-md-5 q-pa-sm">
        <RadarChart :chart-config="radarChartConfig" />
      </div>
      <div class="col-4 col-md-5 q-pa-sm">
        <PieChart :chart-config="pieChartConfig" />
      </div>
    </div>

    <q-separator class="q-ma-sm"></q-separator>

    <div><BarChart :chart-config="barChartConfig" /></div>
    
  </q-page>
</template>
