
class FinalIndividualFitness {
    individualID: number;
    value : number;

    individualMatchResults: IndividualMatchResult[];
    avgMatchResult: IndividualMatchResult;
    additionalValues: { [id: string] : number; };

    public static avgMatchResultSum(avgMatchResult: IndividualMatchResult): number {
        let sum = 0;
        for(let id in avgMatchResult.individualValues){
            sum += avgMatchResult.individualValues[id];
        }
        return sum;
    }

    public static avgIndividualMatchResultsString(avgMatchResult: IndividualMatchResult): string{
        let avgIndividualValuesString = "";
        let sum = 0;
        console.log(avgMatchResult);
        for(let id in avgMatchResult.individualValues){
            sum += avgMatchResult.individualValues[id];
            avgIndividualValuesString += id + ": " + avgMatchResult.individualValues[id].toFixed(4) + ",";
        }

        return sum.toFixed(4) + ";" + avgIndividualValuesString;
    }
}

class IndividualMatchResult {
    opponentsIDs: number[];
    matchName: string;
    value: number;
    individualValues: { [id: string] : number; };
}

export { FinalIndividualFitness, IndividualMatchResult }