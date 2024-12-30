<script setup lang="ts">
  import LineChart from "@/components/LineChart.vue";
  import PieChart from "@/components/PieChart.vue";
  import RadarChart from "@/components/RadarChart.vue";
  import BarChart from "@/components/BarChart.vue";
  import type { ChartConfig } from "@/models/chartConfig";
  import { onMounted, reactive, ref } from "vue";
  import { useGpAlgorithProgressDataStore } from "../stores/gpAlgorithmProgressData";
  import { FinalIndividualFitness } from "../models/finalIndividualFitness";
  

  const lineChartConfigBestAvgFitnessIndividual: ChartConfig = reactive({
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
      plugins: {
          zoom: {
            zoom: {
              wheel: {
                enabled: true,
              },
              pinch: {
                enabled: true
              },
              mode: 'x',
            }
          }
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
      plugins: {
          zoom: {
            zoom: {
              wheel: {
                enabled: true,
              },
              pinch: {
                enabled: true
              },
              mode: 'x',
            }
          }
        },
    }
  });

  const lineChartConfigBestIndividualAvgFitness: ChartConfig = reactive({
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
      plugins: {
          zoom: {
            zoom: {
              wheel: {
                enabled: true,
              },
              pinch: {
                enabled: true
              },
              mode: 'x',
            }
          }
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
      labels: [],
        datasets: [],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false
    }
  });

  const pieChartConfig: ChartConfig = reactive({
    id: 'pie-chart-1',
    data: {
      labels: [],
        datasets: []
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

    // 4. Reload RadarChart config
    gpAlgorithProgressDataStore._selectedElements = [];
    gpAlgorithProgressDataStore._lastSelectedElement = null;
    reloadRadarChartSelectedIndividualsConfig();

    reloadPieChartSelectedIndividualsConfig();
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
    lineChartConfigBestAvgFitnessIndividual.data = gpAlgorithProgressDataStore.reloadLineChartDatasetBestIndividual(gpAlgorithProgressDataStore.configurationNum - 1, gpAlgorithProgressDataStore.runNum - 1);
    lineChartConfigBestIndividualRank.data = gpAlgorithProgressDataStore.reloadLineChartDatasetBestIndividualRank(gpAlgorithProgressDataStore.configurationNum - 1, gpAlgorithProgressDataStore.runNum - 1);

    lineChartConfigBestIndividualAvgFitness.data = gpAlgorithProgressDataStore.reloadLineChartDatasetBestIndividualAvgFitness(gpAlgorithProgressDataStore.configurationNum - 1, gpAlgorithProgressDataStore.runNum - 1);
  };

  const reloadRadarChartSelectedIndividualsConfig = () => {
    console.log(gpAlgorithProgressDataStore.selectedElements);
    radarChartConfig.data = gpAlgorithProgressDataStore.reloadRadarChartSelectedIndividualsConfig();
  };

  const reloadPieChartSelectedIndividualsConfig = () => {
    pieChartConfig.data = gpAlgorithProgressDataStore.reloadPieChartSelectedIndividualsConfig();
  };

  const lineChartBestIndividualClick = (element) => {
    if(element != null && element.value.individual != null) {
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

      // Reset PieChart config
      reloadPieChartSelectedIndividualsConfig();
    }
  };
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
        <h5 class="q-pa-sm q-ma-xs">Progress based on individual avg best fitness</h5>
      </div>
      <div class="col-4 col-md-12 q-pa-sm">
        <LineChart :chart-config="lineChartConfigBestAvgFitnessIndividual" @lineChartClick="lineChartBestIndividualClick"/>       
      </div>
    </div>

    <div class="row">
      <div class="col-4 col-md-12 q-pa-sm">
        <h5 class="q-pa-sm q-ma-xs">Progress based on best individual Rating</h5>
      </div>
      <div class="col-4 col-md-12 q-pa-sm">
        <LineChart :chart-config="lineChartConfigBestIndividualRank" @lineChartClick="lineChartBestIndividualClick"/>
      </div>
    </div>

    <div class="row">
      <div class="col-4 col-md-12 q-pa-sm">
        <h5 class="q-pa-sm q-ma-xs">Progress based on best individual avg Fitness</h5>
      </div>
      <div class="col-4 col-md-12 q-pa-sm">
        <LineChart :chart-config="lineChartConfigBestIndividualAvgFitness" @lineChartClick="lineChartBestIndividualClick"/>
      </div>
    </div>

    <div class="row">
      <div v-if="gpAlgorithProgressDataStore.selectedElements.length > 0" class="col-4 col-md-6 q-pa-sm">
        <RadarChart :chart-config="radarChartConfig" />
      </div>
      <div v-if="gpAlgorithProgressDataStore.lastSelectedElement != null" class="col-4 col-md-6 q-pa-sm">
        <q-field>Individual ID: {{ gpAlgorithProgressDataStore.lastSelectedElement.value.individual.individualId }}</q-field>
        <q-field>Changes count: {{ gpAlgorithProgressDataStore.lastSelectedElement.value.individual.changesCount }}</q-field>
        <q-field>Function nodes: {{ gpAlgorithProgressDataStore.lastSelectedElement.value.individual.functionNodes }}</q-field>

        <q-field>Terminal nodes: {{ gpAlgorithProgressDataStore.lastSelectedElement.value.individual.terminalNodes }}</q-field>
        <q-field>Tree size: {{ gpAlgorithProgressDataStore.lastSelectedElement.value.individual.treeSize }}</q-field>
        <q-field>Tree depth: {{ gpAlgorithProgressDataStore.lastSelectedElement.value.individual.treeDepth }}</q-field>
        <q-field>Fitness: {{ gpAlgorithProgressDataStore.lastSelectedElement.value.individual.objectives[0] }}, {{ gpAlgorithProgressDataStore.lastSelectedElement.value.individual.finalIndividualFitness.additionalValues["StdDeviation"] }}</q-field>
        <q-field>Avg raw fitness: {{ FinalIndividualFitness.avgIndividualMatchResultsString(gpAlgorithProgressDataStore.lastSelectedElement.value.individual.finalIndividualFitness.avgMatchResult) }}</q-field>
      </div>
      <div v-if="gpAlgorithProgressDataStore.lastSelectedElement != null" class="col-4 col-md-6 q-pa-sm">
        <PieChart :chart-config="pieChartConfig" />
      </div>
    </div>

    <!--<q-separator class="q-ma-sm"></q-separator>

    <div><BarChart :chart-config="barChartConfig" /></div>-->
    
  </q-page>
</template>
