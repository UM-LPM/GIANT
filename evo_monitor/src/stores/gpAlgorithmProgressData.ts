import { defineStore, acceptHMRUpdate } from 'pinia';
import { GPAlgorithmMultiConfigurationsProgressData, GPProgramSolutionSimple } from "../models/gpAlgorithmData";
import { fetchGpAlgorithmProgressDataFromJsonFile } from '../services/gpAlgorithmProgressDataAPI';
import { ChartData } from '../models/chartConfig';
import { FinalIndividualFitness } from '../models/finalIndividualFitness';

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
        getSelectedIndividuals: (state) => () => {
            let selectedIndividuals: GPProgramSolutionSimple[] = [];
            for (let selectedElement of state._selectedElements) {
                selectedIndividuals.push(selectedElement.value.individual);
            }
            return selectedIndividuals;
        },
        getLastSelectedIndividual: (state) => () => {
            if(state._lastSelectedElement !== null) {
                return state._lastSelectedElement.value.individual;
            }
            return null;
        },
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
                        
            let selectedIndividuals: GPProgramSolutionSimple[] = this.getSelectedIndividuals();

            let avgIndividualsValues: { [id: string] : number; }[] = [];

            for (let selectedIndividual of selectedIndividuals) {
                avgIndividualsValues.push(FinalIndividualFitness.avgIndividualMatchResults(selectedIndividual.finalIndividualFitness.individualMatchResults));
            }

            let labels: string[] = [];
            let datasets: any[] = [];

            for (let avgIndividualValues of avgIndividualsValues) {
                for (let id in avgIndividualValues) {
                    // Check if id already exists in labels
                    if (!labels.includes(id)) {
                        labels.push(id);
                    } 
                }
            }

            // Order labels alphabetically
            labels = labels.sort();
            
            for(let i = 0; i < selectedIndividuals.length; i++) {
                let color = '#'+(0x1000000+(Math.random())*0xffffff).toString(16).substr(1,6);
                let dataset = {
                    label: 'Individual_' + this._selectedElements[i].configurationNum + '_' + this._selectedElements[i].runNum + '_' + this._selectedElements[i].label,
                    backgroundColor: 'rgba(179,181,198,0.2)',
                    borderColor: color,
                    pointBackgroundColor: color,
                    pointBorderColor: '#fff',
                    pointHoverBackgroundColor: '#fff',
                    pointHoverBorderColor: 'rgba(179,181,198,1)',
                    data: []
                };

                for (let id in labels) {
                    if(avgIndividualsValues[i][labels[id]] === undefined) {
                        dataset.data.push(0);
                    }else{
                        dataset.data.push(Math.abs(avgIndividualsValues[i][labels[id]]));
                    }
                }
                datasets.push(dataset);
            }

            data.labels = labels;
            data.datasets = datasets;

            return data;
        },
        reloadPieChartSelectedIndividualsConfig(): ChartData {
            let data: ChartData = new ChartData([], []);

            let selectedIndividuals: GPProgramSolutionSimple = this.getLastSelectedIndividual();

            if(selectedIndividuals === null) {
                return data;
            }

            // for each node count in the last selected individual
            let dataset: any = {
                backgroundColor: [],
                data: []
            };

            for (let nodeCount in selectedIndividuals.nodeCounts) {
                data.labels.push(nodeCount);
                
                dataset.backgroundColor.push('#'+(0x1000000+(Math.random())*0xffffff).toString(16).substr(1,6));
                dataset.data.push(selectedIndividuals.nodeCounts[nodeCount]);
            }

            data.datasets.push(dataset);

            return data;
        },
    },
});

/*if (import.meta.hot) {
    import.meta.hot.accept(acceptHMRUpdate(useDevicesStore, import.meta.hot))
}*/