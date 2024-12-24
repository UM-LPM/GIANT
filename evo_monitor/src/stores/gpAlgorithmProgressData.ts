import { defineStore, acceptHMRUpdate } from 'pinia';
import { GPAlgorithmMultiConfigurationsProgressData } from "../models/gpAlgorithmData";
import { fetchGpAlgorithmProgressDataFromJsonFile } from '../services/gpAlgorithmProgressDataAPI';
import { ChartData } from '../models/chartConfig';

export const useGpAlgorithProgressDataStore = defineStore({
    id: 'gpAlgorithProgressData',
    state: () => ({
      _gpAlgorithProgressData: GPAlgorithmMultiConfigurationsProgressData,
      _configurationNum: 1,
      _maxConfigurationNum: 0,
      _runNum: 1,
      _maxRunNum: 0
    }),
    getters: {
        gpAlgorithProgressData: (state) => state._gpAlgorithProgressData,
        configurationNum: (state) => state._configurationNum,
        maxConfigurationNum: (state) => state._maxConfigurationNum,
        runNum: (state) => state._runNum,
        maxRunNum: (state) => state._maxRunNum,
    },
    actions: {
        async fetchGpAlgorithmProgressDataFromJsonFile() {
            this._gpAlgorithProgressData = await fetchGpAlgorithmProgressDataFromJsonFile();
        },
        setConfigurationNum(configurationNum: number) {
            this._configurationNum = configurationNum;
        },
        setMaxConfigurationNum(maxConfigurationNum: number) {
            this._maxConfigurationNum = maxConfigurationNum;
        },
        setRunNum(runNum: number) {
            this._runNum = runNum;
        },
        setMaxRunNum(maxRunNum: number) {
            this._maxRunNum = maxRunNum;
        },
        reloadLineChartDatasetBestIndividual(configurationNum: number, runNum: number): ChartData {
            const runProgressData = this._gpAlgorithProgressData.multiConfigurationProgressData[configurationNum].multiRunProgressData[runNum];

            // Create datasets for the line chart (Best individual for each generation in runProgressData)
            let data: ChartData = new ChartData([], []);

            let bestIndividualDataset = {
                label: 'Best Individual',
                backgroundColor: '#21BA45',
                data: []
            };

            let avgIndividualDataset = {
                label: 'Average Individual',
                backgroundColor: '#31CCEC',
                data: []
            };

            let worstIndividualDataset = {
                label: 'Worst Individual',
                backgroundColor: '#C10015',
                data: []
            };

            let lastPhase: string = "";
            for (let genProgressData of runProgressData.gensProgressData) {
                if(lastPhase !== genProgressData.executionPhaseName) {
                    data.labels.push(genProgressData.generation.toString() +"(" + genProgressData.executionPhaseName + ")");
                    lastPhase = genProgressData.executionPhaseName;
                }
                else{
                    data.labels.push(genProgressData.generation.toString());
                }

                // Find the best individual in the generation (Sum of individualMatchResults.value)
                let bestFitness = (genProgressData.population[0].finalIndividualFitness.individualMatchResults.reduce((sum, result) => sum + result.value, 0)) / genProgressData.population[0].finalIndividualFitness.individualMatchResults.length;
                let avgFitness = 0;
                let worstFitness = bestFitness;
                
                for (let individual of genProgressData.population) {
                    // Match results
                    let currentIndividualFitness = (individual.finalIndividualFitness.individualMatchResults.reduce((sum, result) => sum + result.value, 0)) / individual.finalIndividualFitness.individualMatchResults.length;

                    avgFitness += currentIndividualFitness;

                    if(currentIndividualFitness < bestFitness) {
                        bestFitness = currentIndividualFitness;
                    }

                    if(currentIndividualFitness > worstFitness) {
                        worstFitness = currentIndividualFitness;
                    }
                }

                // Add the best individual's fitness to the dataset
                bestIndividualDataset.data.push(bestFitness);
                avgIndividualDataset.data.push(avgFitness/genProgressData.population.length);
                worstIndividualDataset.data.push(worstFitness);
            }

            data.datasets.push(bestIndividualDataset);
            data.datasets.push(avgIndividualDataset);
            data.datasets.push(worstIndividualDataset);
            return data;    
        },
        reloadLineChartDatasetBestIndividualRank(configurationNum: number, runNum: number): ChartData {
            const runProgressData = this._gpAlgorithProgressData.multiConfigurationProgressData[configurationNum].multiRunProgressData[runNum];

            // Create datasets for the line chart (Best individual for each generation in runProgressData)
            let data: ChartData = new ChartData([], []);

            let bestIndividualDataset = {
                label: 'Best Individual',
                backgroundColor: '#21BA45',
                data: []
            };

            let avgIndividualDataset = {
                label: 'Average Individual',
                backgroundColor: '#31CCEC',
                data: []
            };

            let worstIndividualDataset = {
                label: 'Worst Individual',
                backgroundColor: '#C10015',
                data: []
            };

            let lastPhase: string = "";
            for (let genProgressData of runProgressData.gensProgressData) {
                if(lastPhase !== genProgressData.executionPhaseName) {
                    data.labels.push(genProgressData.generation.toString() +"(" + genProgressData.executionPhaseName + ")");
                    lastPhase = genProgressData.executionPhaseName;
                }
                else{
                    data.labels.push(genProgressData.generation.toString());
                }

                // Find the best individual in the generation (Sum of individualMatchResults.value)
                let bestFitness = genProgressData.population[0].finalIndividualFitness.additionalValues["Rating"];
                let avgFitness = 0;
                let worstFitness = bestFitness;
                
                for (let individual of genProgressData.population) {                   
                    // Rating
                    let currentIndividualFitness = individual.finalIndividualFitness.additionalValues["Rating"];

                    avgFitness += currentIndividualFitness;

                    if(currentIndividualFitness < bestFitness) {
                        bestFitness = currentIndividualFitness;
                    }

                    if(currentIndividualFitness > worstFitness) {
                        worstFitness = currentIndividualFitness;
                    }
                }

                // Add the best individual's fitness to the dataset
                bestIndividualDataset.data.push(bestFitness);
                avgIndividualDataset.data.push(avgFitness/genProgressData.population.length);
                worstIndividualDataset.data.push(worstFitness);
            }

            data.datasets.push(bestIndividualDataset);
            data.datasets.push(avgIndividualDataset);
            data.datasets.push(worstIndividualDataset);
            return data;    
        },
    },
});

/*if (import.meta.hot) {
    import.meta.hot.accept(acceptHMRUpdate(useDevicesStore, import.meta.hot))
}*/