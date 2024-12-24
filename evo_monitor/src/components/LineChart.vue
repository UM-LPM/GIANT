<script setup lang="ts">
    import { Chart as ChartJS, CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend } from 'chart.js';
    import { ChartComponentRef, getElementAtEvent, Line } from 'vue-chartjs';
    import { ChartConfig } from '../models/chartConfig';
import { InteractionItem } from 'chart.js/dist/core/core.interaction';
import { ref } from 'vue';

    ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend);

    const props = defineProps({
        chartConfig: ChartConfig,
    });

    const emit = defineEmits(['lineChartClick']);

    const chartRef = ref<ChartComponentRef>(null)
    
    const elementAtEvent = (element: InteractionItem[]) => {
      if (!element.length) return null;

      const { datasetIndex, index } = element[0];

      return ({
        index: index,
        label: props.chartConfig.data.labels[index],
        value: props.chartConfig.data.datasets[datasetIndex].data[index]
      })
    }

    const handleClick = (event: MouseEvent) => {
        const {
            value: { chart }
        } = chartRef

        if (!chart) {
            return
        }

        let element = elementAtEvent(getElementAtEvent(chart, event));

        if(element != null) {
            emit('lineChartClick', element); // Emit the click event
        }
    };
</script>

<template>
    <Line :id="props.chartConfig.id" :options="props.chartConfig.options" :data="props.chartConfig.data" @click="handleClick" ref="chartRef"/>
</template>