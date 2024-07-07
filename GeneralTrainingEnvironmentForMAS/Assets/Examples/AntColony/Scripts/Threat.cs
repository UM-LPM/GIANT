using UnityEngine;

public class Threat : MonoBehaviour
{
    public float health = 50f;
    public float radius = 5f;
    public float speed = 2f;
    private Vector3 center;

    private void Start()
    {
        center = transform.position;
    }

    private void Update()
    {
        // Walking in a circle
        float x = Mathf.Cos(Time.time * speed) * radius;
        float y = Mathf.Sin(Time.time * speed) * radius;
        transform.position = center + new Vector3(x, y, 0);
    }
}
