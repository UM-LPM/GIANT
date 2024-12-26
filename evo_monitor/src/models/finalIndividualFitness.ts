
class FinalIndividualFitness {
    individualID: number;
    value : number;

    individualMatchResults: IndividualMatchResult[];
    additionalValues: { [id: string] : number; };

    public static avgIndividualMatchResults(individualMatchResults: IndividualMatchResult[]): { [id: string] : number; }{
        let avgIndividualValues: { [id: string] : number; } = {};
        for(let matchResult of individualMatchResults){
            for(let id in matchResult.individualValues){
                if(avgIndividualValues[id] === undefined){
                    avgIndividualValues[id] = 0;
                }
                avgIndividualValues[id] += matchResult.individualValues[id];
            }
        }
        for(let id in avgIndividualValues){
            avgIndividualValues[id] = avgIndividualValues[id] / individualMatchResults.length;
        }
        return avgIndividualValues;
    }

    public static avgIndividualMatchResultsString(individualMatchResults: IndividualMatchResult[]): string{
        let avgIndividualValues: { [id: string] : number; } = this.avgIndividualMatchResults(individualMatchResults);
        let avgIndividualValuesString = "";
        let sum = 0;
        for(let id in avgIndividualValues){
            sum += avgIndividualValues[id];
            avgIndividualValuesString += id + ": " + avgIndividualValues[id].toFixed(4) + ",";
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