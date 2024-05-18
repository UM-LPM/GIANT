using UnityEngine;

public class PheromoneNodeComponent : MonoBehaviour
{
    public float intensity; 
    public Vector3 position;
    public float creationTime; 
    public float evaporationRate;
    public PheromoneNodeComponent next { get; set; }
    public PheromoneNodeComponent previous { get; set; }

    public void Initialize(float intensity, Vector3 position,float evaporationRate, PheromoneNodeComponent next, PheromoneNodeComponent previous)
    {
        this.intensity = intensity;
        this.position = position;
        this.evaporationRate = evaporationRate;
        this.creationTime = Time.time; 
        this.next = next;
        this.previous = previous;
    }
    public void Evaporate()
    {
        float elapsedTime = Time.time - creationTime;
        intensity -= evaporationRate * elapsedTime;
        creationTime = Time.time; 
    }

    public bool IsEvaporated()
    {
        return intensity <= 0;
    }

}
