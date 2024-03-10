using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentepedeSegment : MonoBehaviour
{
    public SpriteRenderer spriteRenderer { get; private set; }
    public Centepede centepede { get;  set; }
    public CentepedeSegment ahead {  get;  set; }
    public CentepedeSegment behind { get; set; }
    public bool isHead => ahead == null;
    
    private Vector2 direction = Vector2.right + Vector2.down;
    private Vector2 targetPosition;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();    
        targetPosition= transform.position;
    }
    private void Update()
    {
        if(isHead && Vector2.Distance(transform.position,targetPosition)<0.1f)
        {
            UpdateHeadSegment();
        }
        //Move
        Vector2 currentPosition=transform.position;
        transform.position=Vector2.MoveTowards(currentPosition, targetPosition, centepede.speed* Time.deltaTime);
        //Rotation
        Vector2 movementDirection = (targetPosition - currentPosition).normalized;
        float angle= Mathf.Atan2(movementDirection.y,movementDirection.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
        spriteRenderer.sprite = isHead ? centepede.headSprite : centepede.bodySprite;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.enabled && collision.gameObject.layer == LayerMask.NameToLayer("Dart"))
        {
            collision.collider.enabled = false;
            centepede.Remove(this);
        }
    }
    public void UpdateHeadSegment()
    {
        Vector2 gridPosition = GridPosition(transform.position);

        targetPosition = gridPosition;
        targetPosition.x += direction.x;

        if (Physics2D.OverlapBox(targetPosition,Vector2.zero,0f,centepede.collisionMask))
        {
            direction.x = -direction.x;
            targetPosition.x = gridPosition.x;
            targetPosition.y = gridPosition.y+ direction.y;

            Bounds homeBounds=centepede.homeArea.bounds;

            if((direction.y == 1f && targetPosition.y>homeBounds.max.y)
              || (direction.y == -1f && targetPosition.y < homeBounds.min.y))
            {
                direction.y = -direction.y;
                targetPosition.y = gridPosition.y + direction.y;
            }

        }
        if (behind != null)
        {
            behind.UpdateBodySegment();
        }
    }
    private void UpdateBodySegment()
    {
        targetPosition = GridPosition(ahead.transform.position);
        direction = ahead.direction;
        if (behind != null)
        {
            behind.UpdateBodySegment();
        }
    }

    private Vector2 GridPosition(Vector2 position)
    {
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);
        return position;
    }
}
