<script setup lang="ts">
    import { ChartConfig } from '@/models/chartConfig';
    import {
        Chart as ChartJS,
        Title,
        Tooltip,
        Legend,
        BarElement,
        CategoryScale,
        LinearScale
    } from 'chart.js';
    import { Bar } from 'vue-chartjs';

    ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

    const props = defineProps({
        chartConfig: ChartConfig,
    });

</script>

<template>
    <Bar :id="props.chartConfig.id" :options="props.chartConfig.options" :data="props.chartConfig.data" />
</template>