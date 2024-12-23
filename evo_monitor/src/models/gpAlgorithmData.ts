import type { FinalIndividualFitness } from "./finalIndividualFitness";
import * as fs from 'fs';

class GPAlgorithmMultiConfigurationsProgressData {
    multiConfigurationProgressData: GPAlgorithmMultiConfigurationsProgressData[];
}

class GPAlgorithmMultiRunProgressData {
    multiRunProgressData: GPAlgorithmRunProgressData[];
}


class GPAlgorithmRunProgressData {
    gensProgressData: GPAlgorithmGenProgressData[];
}

class GPAlgorithmGenProgressData {
    generation: number;
    population: GPProgramSolutionSimple;
}

class GPProgramSolutionSimple {
    individualId: number;
    objectives: number[];
    finalIndividualFitness: FinalIndividualFitness;
    changesCount: number;
    treeSize: number;
    treeDepth: number;
    terminalNodes: number;
    functionNodes: number;
    treeDotString: string;
    nodeCounts: { [id: string] : number; };
}

export { GPAlgorithmMultiConfigurationsProgressData, GPAlgorithmMultiRunProgressData, GPAlgorithmRunProgressData, GPAlgorithmGenProgressData, GPProgramSolutionSimple }