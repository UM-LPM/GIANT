<script setup lang="ts">
    import { ChartConfig } from '@/models/chartConfig';
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js';
    import { Pie } from 'vue-chartjs';

    ChartJS.register(ArcElement, Tooltip, Legend);

    const props = defineProps({
        chartConfig: ChartConfig,
    });

</script>

<template>
    <Pie :id="props.chartConfig.id"  :data="props.chartConfig.data" :options="props.chartConfig.options" />
</template>