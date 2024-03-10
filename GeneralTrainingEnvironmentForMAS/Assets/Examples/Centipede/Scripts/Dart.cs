using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Dart : MonoBehaviour
{
    private  Rigidbody2D dartRigidBody;
    private  Collider2D dartCollider;
    private Transform parent;
    public  float speed=40f;
    private void Awake()
    {
        dartRigidBody= GetComponent<Rigidbody2D>();
        dartRigidBody.bodyType = RigidbodyType2D.Kinematic;
        dartCollider = GetComponent<Collider2D>();
        dartCollider.enabled = false;
        parent = transform.parent;
       
    }
    private void Update()
    {
        if (dartRigidBody.isKinematic && Input.GetButton("Fire1"))
        {
            transform.SetParent(null);
            dartRigidBody.bodyType = RigidbodyType2D.Dynamic;
            dartCollider.enabled=true;
        }
    }
    private void FixedUpdate()
    {
        if (!dartRigidBody.isKinematic)
        {
            Vector2 position = dartRigidBody.position;
            position += Vector2.up * speed * Time.fixedDeltaTime;
            dartRigidBody.MovePosition(position);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        transform.SetParent(parent);
        transform.localPosition = new Vector3(0f, 0.5f, 0f);
        dartRigidBody.bodyType = RigidbodyType2D.Kinematic;
        dartCollider.enabled = false;

    }
}
