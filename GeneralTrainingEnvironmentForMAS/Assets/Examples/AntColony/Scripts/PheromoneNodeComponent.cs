using UnityEngine;

public class PheromoneNodeComponent : MonoBehaviour
{
    public float intensity; 
    public Vector3 position;
    public PheromoneNodeComponent next { get; set; }
    public PheromoneNodeComponent previous { get; set; }

    public  PheromoneNodeComponent(float intensity, Vector3 position,PheromoneNodeComponent next,PheromoneNodeComponent previous)
    {
        this.intensity = intensity;
        this.position = position;
        this.next = next;
        this.previous = previous;
    }

  
}
