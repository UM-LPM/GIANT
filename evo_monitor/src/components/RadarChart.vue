<script setup lang="ts">
    import { ChartConfig } from '@/models/chartConfig';
    import {Chart as ChartJS, RadialLinearScale,PointElement, LineElement, Filler, Tooltip, Legend } from 'chart.js';
    import { Radar } from 'vue-chartjs';

    ChartJS.register(
        RadialLinearScale,
        PointElement,
        LineElement,
        Filler,
        Tooltip,
        Legend
    )

    const props = defineProps({
        chartConfig: ChartConfig,
    });

</script>

<template>
    <Radar id="my-chart-id" :options="props.chartConfig.options" :data="props.chartConfig.data" />
</template>