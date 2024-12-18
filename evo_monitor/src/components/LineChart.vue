<script setup lang="ts">
    import { ChartConfig } from '@/models/chartConfig';
    import { Chart as ChartJS, CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend } from 'chart.js';
    import { Line } from 'vue-chartjs';

    ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend);

    const props = defineProps({
        chartConfig: ChartConfig,
    });

</script>

<template>
    <Line :id="props.chartConfig.id" :options="props.chartConfig.options" :data="props.chartConfig.data" />
</template>