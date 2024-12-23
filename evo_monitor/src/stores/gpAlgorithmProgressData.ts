import { defineStore, acceptHMRUpdate } from 'pinia';
import { GPAlgorithmMultiConfigurationsProgressData } from "../models/gpAlgorithmData";
import { fetchGpAlgorithmProgressDataFromJsonFile } from '../services/gpAlgorithmProgressDataAPI';
import { ChartData } from '../models/chartConfig';

export const useGpAlgorithProgressDataStore = defineStore({
    id: 'gpAlgorithProgressData',
    state: () => ({
      _gpAlgorithProgressData: GPAlgorithmMultiConfigurationsProgressData,
    }),
    getters: {
        gpAlgorithProgressData: (state) => state._gpAlgorithProgressData,
    },
    actions: {
        async fetchGpAlgorithmProgressDataFromJsonFile() {
            this._gpAlgorithProgressData = await fetchGpAlgorithmProgressDataFromJsonFile();
        },
        reloadLineChartDatasetBestIndividual(configurationNum: number, runNum: number): ChartData {
            console.log(`reloadLineChartDatasetBestIndividual(${configurationNum}, ${runNum})`);
            const runProgressData = this._gpAlgorithProgressData.multiConfigurationProgressData[configurationNum].multiRunProgressData[runNum];

            // Create datasets for the line chart (Best individual for each generation in runProgressData)
            let data: ChartData = new ChartData([], []);

            let bestIndividualDataset = {
                label: 'Best Individual',
                backgroundColor: '#00ff00',
                data: []
            };

            for (let genProgressData of runProgressData.gensProgressData) {
                // TODO Implement this (Find the best individual in genProgressData)
                throw new Error("Not implemented yet");
                //bestIndividualDataset.data.push(genProgressData.population[0].finalIndividualFitness.additionalValues["Rating"]);
            }

            data.datasets.push(bestIndividualDataset);

            return data;    
        }
    },
});

/*if (import.meta.hot) {
    import.meta.hot.accept(acceptHMRUpdate(useDevicesStore, import.meta.hot))
}*/