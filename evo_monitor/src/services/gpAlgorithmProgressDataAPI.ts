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
        // Parse the JSON response in chunks to read big files (over 1GB)
        const reader = response.body?.getReader();
        if (!reader) {
            throw new Error('Response body is null');
        }

        let decoder = new TextDecoder();
        let data = '';
        let data2 = '';

        while (true) {
            const { done, value } = await reader.read();
            if (done) {
                break;
            }
            if(data.length > 500000000)
            {
                data2 += decoder.decode(value);
                //console.log("data2 read so far: ", data2.length);
            }
            else{
                data += decoder.decode(value);
                //console.log("data read so far: ", data.length);
            }
        }

        const combinedJson = `${data}${data2}`;
        
        const jsonData = JSON.parse(combinedJson);

        toast.success(`Data loaded from ${PROGRESS_DATA_SOURCE}`);
        return jsonData as GPAlgorithmMultiConfigurationsProgressData;

        /*const data = await response.json();
        toast.success(`Data loaded from ${PROGRESS_DATA_SOURCE}`);
        return data as GPAlgorithmMultiConfigurationsProgressData;*/
    }
    catch (err) {
        console.error(err);
        return null;
    }
};

export { fetchGpAlgorithmProgressDataFromJsonFile }
