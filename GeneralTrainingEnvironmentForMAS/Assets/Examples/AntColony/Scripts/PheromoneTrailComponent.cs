using UnityEngine;

public class PheromoneTrailComponent:MonoBehaviour
{
    public PheromoneNodeComponent firstNode { get; set; }
    public PheromoneNodeComponent lastNode { get; set; }
    public PheromoneTrailComponent()
    {
        this.firstNode = null;
        this.lastNode = null;
    }
    public void AddPheromone(Vector3 position)
    {
        PheromoneNodeComponent newNode = new PheromoneNodeComponent(100,position,null,null);

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

}