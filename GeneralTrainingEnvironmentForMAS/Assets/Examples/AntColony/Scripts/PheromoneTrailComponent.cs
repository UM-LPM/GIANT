using UnityEngine;

public abstract  class PheromoneTrailComponent:MonoBehaviour
{
    public float weakTrailTreshold=40;
    public PheromoneNodeComponent firstNode { get; set; }
    public PheromoneNodeComponent lastNode { get; set; }

     public PheromoneType pheromoneType;

    public float CalculateTotalIntensity()
    {
        PheromoneNodeComponent currentNode = firstNode;
        float totalIntensity = 0;
        int length=0;
        while (currentNode != null)
        {
            currentNode.Evaporate();
            totalIntensity += currentNode.intensity;
            length++;
            currentNode = currentNode.next;

        }

        return (totalIntensity% length);
    }

    public bool IsTrailWeak()
    {
        return CalculateTotalIntensity() < weakTrailTreshold;
    }
    private void Awake()
    {
        firstNode = null;
        lastNode = null;
    }
    public void AddPheromone(Vector3 position, float strength, float evaporationRate)
    {
        PheromoneNodeComponent newNode = gameObject.AddComponent<PheromoneNodeComponent>();
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
                RemovePheromone(currentNode);
            }
            currentNode = currentNode.next;
        }
    }

    private void RemovePheromone(PheromoneNodeComponent node)
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

        Destroy(node);
    }

}