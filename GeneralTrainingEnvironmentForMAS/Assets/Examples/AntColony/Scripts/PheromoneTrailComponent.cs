using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PheromoneTrailComponent : MonoBehaviour
{
    public float weakTrailTreshold = 40;
    public PheromoneNodeComponent firstNode { get; set; }
    public PheromoneNodeComponent lastNode { get; set; }

    public PheromoneType pheromoneType;

    private Dictionary<PheromoneType, GameObject> pheromonePrefabs;

    public float CalculateTotalIntensity()
    {
        PheromoneNodeComponent currentNode = firstNode;
        float totalIntensity = 0;
        int length = 0;
        while (currentNode != null)
        {
            currentNode.Evaporate();
            totalIntensity += currentNode.intensity;
            length++;
            currentNode = currentNode.next;

        }

        return (totalIntensity % length);
    }
    // Function to load prefabs using AssetDatabase
    private GameObject LoadPrefab(string path)
    {
#if UNITY_EDITOR
        return (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
#else
                    return null;  
#endif
    }
    public bool IsTrailWeak()
    {
        return CalculateTotalIntensity() < weakTrailTreshold;
    }
    private void Awake()
    {
        firstNode = null;
        lastNode = null;
        pheromonePrefabs = new Dictionary<PheromoneType, GameObject>
        {
            { PheromoneType.Food, LoadPrefab("Assets/Examples/AntColony/Prefabs/foodPheromonePrefab.prefab") },
            { PheromoneType.Water, LoadPrefab("Assets/Examples/AntColony/Prefabs/waterPheromonePrefab.prefab") },
            { PheromoneType.Threat,  LoadPrefab("Assets/Examples/AntColony/Prefabs/threatPheromonePrefab.prefab") }
        };
    }
    public void AddPheromone(Vector3 position, float strength, float evaporationRate)
    {
        GameObject selectedPrefab = pheromonePrefabs[pheromoneType];

        GameObject newPheromoneObject = Instantiate(selectedPrefab, position, Quaternion.identity);
        PheromoneNodeComponent newNode = newPheromoneObject.AddComponent<PheromoneNodeComponent>();
        newNode.Initialize(100, position, evaporationRate, this.pheromoneType, null, null);
        if (firstNode == null)
        {
            firstNode = newNode;
            lastNode = newNode;

        }
        else
        {
            lastNode.next = newNode;
            newNode.previous = lastNode;
            lastNode = newNode;
        }
    }
    public void UpdatePheromones()
    {
        PheromoneNodeComponent currentNode = firstNode;
        while (currentNode != null)
        {
            currentNode.Evaporate();
            if (currentNode.IsEvaporated())
            {
                RemovePheromoneNode(currentNode);
            }
            currentNode = currentNode.next;
        }
    }

    private void RemovePheromoneNode(PheromoneNodeComponent node)
    {
        if (node.previous != null)
        {
            node.previous.next = node.next;
        }
        else
        {
            firstNode = node.next;
        }

        if (node.next != null)
        {
            node.next.previous = node.previous;
        }
        else
        {
            lastNode = node.previous;
        }
            
        Destroy(node.gameObject);
    }

}