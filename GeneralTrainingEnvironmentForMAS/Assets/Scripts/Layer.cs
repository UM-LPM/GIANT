using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Layer {
    public LayerBTIndex[] LayerIds { get; set; }
    // 0 - Available; 1 - Reserverd; 2 - In Use
    public int[] LayerAvailability { get; set; }
    public int BatchSize { get; set; }

    public Layer(int minlayerId, int maxLayerId, int batchSize) {
        LayerIds = new LayerBTIndex[maxLayerId - minlayerId + 1];
        LayerAvailability = new int[LayerIds.Length];

        for (int i = 0; i < LayerIds.Length; i++) {
            LayerIds[i] = new LayerBTIndex(minlayerId + i, -1);
            LayerAvailability[i] = 0; // All are default available
        }
        BatchSize = batchSize;
    }

    public bool IsBatchExecuted(string gameSceneName) {
        if (NumberOfUsedLayeres() == 0) {
            if (SceneManager.sceneCount > 1)
                SceneManager.UnloadSceneAsync(gameSceneName);
            SceneManager.LoadScene(gameSceneName);
        }
        return true;
    }

    public int NumberOfUsedLayeres() {
        int counter = 0;
        foreach (int layerAvailability in LayerAvailability) {
            if (layerAvailability > 0)
                counter++;
        }
        return counter;
    }

    public int GetAndReserveAvailableLayer(int BtIndex) {
        if (CanUseAnotherLayer()) {
            for (int i = 0; i < LayerAvailability.Length; i++) {
                if (LayerAvailability[i] == 0) {
                    LayerAvailability[i] = 1; // Set Layer to reserved
                    LayerIds[i].BTIndex = BtIndex;
                    return i;
                }
            }
        }
        return -1;
    }

    public bool CanUseAnotherLayer() {
        if (NumberOfUsedLayeres() >= BatchSize)
            return false;
        return true;
    }

    public void ReleaseLayer(int layerId) {
        for (int i = 0; i < LayerIds.Length; i++) {
            if (LayerIds[i].LayerId == layerId) {
                LayerAvailability[i] = 0;
            }
        }
    }

    public LayerBTIndex GetReservedLayer() {
        for (int i = 0; i < LayerAvailability.Length; i++) {
            if (LayerAvailability[i] == 1) {
                LayerAvailability[i] = 2;
                return LayerIds[i];
            }
        }
        Debug.LogError("GetReservedLayer() method returned null");
        return null;
    }

}