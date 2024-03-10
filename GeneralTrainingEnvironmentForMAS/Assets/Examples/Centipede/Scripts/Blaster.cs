using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class Blaster : MonoBehaviour
{
    private Rigidbody2D blasterRigidbody;
    private Vector2 direction;
    public float speed = 20f;
    private void Awake()
    {
        blasterRigidbody = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        direction.x = Input.GetAxis("Horizontal");
        direction.y = Input.GetAxis("Vertical");

    }
    private void FixedUpdate()
    {
        Vector2 position = blasterRigidbody.position;
        position += direction.normalized * speed *Time.fixedDeltaTime;
        blasterRigidbody.MovePosition(position);
    }
}
