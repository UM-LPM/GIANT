
class FinalIndividualFitness {
    individualID: number;
    value : number;

    individualMatchResults: IndividualMatchResult[];
    additionalValues: { [id: string] : number; };
}

class IndividualMatchResult {
    opponentsIDs: number[];
    matchName: string;
    value: number;
    individualValues: { [id: string] : number; };
}

export { FinalIndividualFitness, IndividualMatchResult }