import { useToast } from 'vue-toastification';
import { GPAlgorithmMultiConfigurationsProgressData } from '../models/gpAlgorithmData';

const toast = useToast();

const PROGRESS_DATA_SOURCE = import.meta.env.VITE_PROGRESS_DATA_SOURCE;

const fetchGpAlgorithmProgressDataFromJsonFile = async ()  =>  {
    try {
        const response = await fetch(PROGRESS_DATA_SOURCE);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        toast.success(`Data loaded from ${PROGRESS_DATA_SOURCE}`);
        return data as GPAlgorithmMultiConfigurationsProgressData;
    }
    catch (err) {
        console.error(err);
        return null;
    }
};

export { fetchGpAlgorithmProgressDataFromJsonFile }
