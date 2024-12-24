import { defineStore, acceptHMRUpdate } from 'pinia';
import { GPAlgorithmMultiConfigurationsProgressData, GPProgramSolutionSimple } from "../models/gpAlgorithmData";
import { fetchGpAlgorithmProgressDataFromJsonFile } from '../services/gpAlgorithmProgressDataAPI';
import { ChartData } from '../models/chartConfig';

export const useGpAlgorithProgressDataStore = defineStore({
    id: 'gpAlgorithProgressData',
    state: () => ({
      _gpAlgorithProgressData: GPAlgorithmMultiConfigurationsProgressData,
      _configurationNum: 1,
      _maxConfigurationNum: 0,
      _runNum: 1,
      _maxRunNum: 0,
      _selectedElements: [] as SelectedElement[],
      _lastSelectedElement: null as SelectedElement | null,
    }),
    getters: {
        gpAlgorithProgressData: (state) => state._gpAlgorithProgressData,
        configurationNum: (state) => state._configurationNum,
        maxConfigurationNum: (state) => state._maxConfigurationNum,
        runNum: (state) => state._runNum,
        maxRunNum: (state) => state._maxRunNum,
        selectedElements: (state) => state._selectedElements,
        lastSelectedElement: (state) => state._lastSelectedElement,
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
                let dataLabel = genProgressData.generation.toString();
                if(lastPhase !== genProgressData.executionPhaseName) {
                    dataLabel = genProgressData.generation.toString() +"(" + genProgressData.executionPhaseName + ")";
                    data.labels.push(dataLabel);
                    lastPhase = genProgressData.executionPhaseName;

                }
                else{
                    data.labels.push(genProgressData.generation.toString());
                }

                // Find the best individual in the generation (Sum of individualMatchResults.value)
                let bestFitness = (genProgressData.population[0].finalIndividualFitness.individualMatchResults.reduce((sum, result) => sum + result.value, 0)) / genProgressData.population[0].finalIndividualFitness.individualMatchResults.length;
                let bestIndividual = genProgressData.population[0];
                let avgFitness = 0;
                let worstFitness = bestFitness;
                let worstIndividual = genProgressData.population[0];
                
                for (let individual of genProgressData.population) {
                    // Match results
                    let currentIndividualFitness = (individual.finalIndividualFitness.individualMatchResults.reduce((sum, result) => sum + result.value, 0)) / individual.finalIndividualFitness.individualMatchResults.length;

                    avgFitness += currentIndividualFitness;

                    if(currentIndividualFitness < bestFitness) {
                        bestFitness = currentIndividualFitness;
                        bestIndividual = individual;
                    }

                    if(currentIndividualFitness > worstFitness) {
                        worstFitness = currentIndividualFitness;
                        worstIndividual = individual;
                    }
                }

                // Add the best individual's fitness to the dataset
                bestIndividualDataset.data.push({fitness: bestFitness, generation: dataLabel, individual: bestIndividual});
                avgIndividualDataset.data.push({fitness: avgFitness/genProgressData.population.length, generation: dataLabel, individual: null});
                worstIndividualDataset.data.push({fitness: worstFitness, generation: dataLabel, individual: worstIndividual});
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
                let dataLabel = genProgressData.generation.toString();
                if(lastPhase !== genProgressData.executionPhaseName) {
                    dataLabel = genProgressData.generation.toString() +"(" + genProgressData.executionPhaseName + ")";
                    data.labels.push(dataLabel);
                    lastPhase = genProgressData.executionPhaseName;

                }
                else{
                    data.labels.push(genProgressData.generation.toString());
                }

                // Find the best individual in the generation (Sum of individualMatchResults.value)
                let bestFitness = genProgressData.population[0].finalIndividualFitness.additionalValues["Rating"];
                let bestIndividual = genProgressData.population[0];
                let avgFitness = 0;
                let worstFitness = bestFitness;
                let worstIndividual = genProgressData.population[0];
                
                for (let individual of genProgressData.population) {                   
                    // Rating
                    let currentIndividualFitness = individual.finalIndividualFitness.additionalValues["Rating"];

                    avgFitness += currentIndividualFitness;

                    if(currentIndividualFitness < bestFitness) {
                        bestFitness = currentIndividualFitness;
                        bestIndividual = individual;
                    }

                    if(currentIndividualFitness > worstFitness) {
                        worstFitness = currentIndividualFitness;
                        worstIndividual = individual;
                    }
                }

                // Add the best individual's fitness to the dataset
                bestIndividualDataset.data.push({fitness: bestFitness, generation: dataLabel, individual: bestIndividual});
                avgIndividualDataset.data.push({fitness: avgFitness/genProgressData.population.length, generation: dataLabel, individual: null});
                worstIndividualDataset.data.push({fitness: worstFitness, generation: dataLabel, individual: worstIndividual});
            }

            data.datasets.push(bestIndividualDataset);
            data.datasets.push(avgIndividualDataset);
            data.datasets.push(worstIndividualDataset);
            return data;    
        },
        reloadRadarChartSelectedIndividualsConfig(): ChartData {
            let data: ChartData = new ChartData([], []);
            
            let selectedIndividuals: GPProgramSolutionSimple[] = [];

            // TODO Implement
            /*for (let selectedElement of this._selectedElements) {
                selectedIndividuals.push(this._gpAlgorithProgressData.multiConfigurationProgressData[selectedElement.configurationNum].multiRunProgressData[selectedElement.runNum].gensProgressData[selectedElement.generationNum].population[selectedElement.value]);
            }*/

            return data;
        },
    },
});

/*if (import.meta.hot) {
    import.meta.hot.accept(acceptHMRUpdate(useDevicesStore, import.meta.hot))
}*/