using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base
{
    public class Layer
    {
        public LayerData[] LayerData { get; set; }
        // 0 - Available; 1 - Reserverd; 2 - In Use
        public int[] LayerAvailability { get; set; }
        public int BatchSize { get; set; }

        public Layer(int minlayerId, int maxLayerId, int batchSize)
        {
            LayerData = new LayerData[maxLayerId - minlayerId + 1];
            LayerAvailability = new int[LayerData.Length];

            for (int i = 0; i < LayerData.Length; i++)
            {
                LayerData[i] = new LayerData(minlayerId + i, -1);
                LayerAvailability[i] = 0; // All are default available
            }
            BatchSize = batchSize;
        }

        public bool IsBatchExecuted(string gameSceneName)
        {
            if (NumberOfUsedLayeres() == 0)
            {
                if (SceneManager.sceneCount > 1)
                    SceneManager.UnloadSceneAsync(gameSceneName);
                SceneManager.LoadScene(gameSceneName);
            }
            return true;
        }

        public int NumberOfUsedLayeres()
        {
            int counter = 0;
            foreach (int layerAvailability in LayerAvailability)
            {
                if (layerAvailability > 0)
                    counter++;
            }
            return counter;
        }

        public int GetAndReserveAvailableLayer(int matchIndex, string gameSceneName, string agentSceneName)
        {
            if (CanUseAnotherLayer())
            {
                for (int i = 0; i < LayerAvailability.Length; i++)
                {
                    if (LayerAvailability[i] == 0)
                    {
                        LayerAvailability[i] = 1; // Set Layer to reserved
                        LayerData[i].MatchIndex = matchIndex;
                        LayerData[i].GameSceneName = gameSceneName;
                        LayerData[i].AgentSceneName = agentSceneName;
                        return i;
                    }
                }
            }
            return -1;
        }

        public bool CanUseAnotherLayer()
        {
            if (NumberOfUsedLayeres() >= BatchSize)
                return false;
            return true;
        }

        public void ReleaseLayer(int layerId)
        {
            for (int i = 0; i < LayerData.Length; i++)
            {
                if (LayerData[i].LayerId == layerId)
                {
                    LayerAvailability[i] = 0;
                }
            }
        }

        public LayerData GetReservedLayer()
        {
            for (int i = 0; i < LayerAvailability.Length; i++)
            {
                if (LayerAvailability[i] == 1)
                {
                    LayerAvailability[i] = 2;
                    return LayerData[i];
                }
            }
            Debug.LogError("GetReservedLayer() method returned null");
            return null;
        }

    }

    public class LayerData
    {
        public int LayerId;
        public int MatchIndex;
        public string GameSceneName;
        public string AgentSceneName;

        public LayerData(int layerId, int btIndex)
        {
            LayerId = layerId;
            MatchIndex = btIndex;
        }
    }
}