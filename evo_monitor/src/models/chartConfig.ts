class ChartConfig {
    id: string;
    data: ChartData;
    options: any;

    constructor(id: string, data: ChartData, options: any) {
        this.id = id;
        this.data = data;
        this.options = options;
    }
}

class ChartData {
    labels: string[];
    datasets: any[];

    constructor(labels: string[], datasets: any[]) {
        this.labels = labels;
        this.datasets = datasets;
    }
}

export { ChartConfig, ChartData };