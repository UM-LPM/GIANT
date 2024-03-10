using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Centepede : MonoBehaviour
{
    public CentepedeSegment segmentPrefab;
    private List<CentepedeSegment> segments=new List<CentepedeSegment>();
    public Sprite headSprite;
    public Sprite bodySprite;
    public int size = 12;
    public float speed=1f;
    public Mushroom mushroomPrefab;

    public LayerMask collisionMask;
    public BoxCollider2D homeArea;
    private void Start()
    {
        Respawn();
    }
    private void Respawn()
    {
        foreach (CentepedeSegment segment in segments)
        {
            Destroy(segment.gameObject);
        }
        segments.Clear();

        for(int i = 0; i < size; i++)
        {
            Vector2 position = GridPosition(transform.position) + (Vector2.left*i);
            CentepedeSegment segment = Instantiate(segmentPrefab, position,Quaternion.identity);
            segment.spriteRenderer.sprite = i == 0 ? headSprite : bodySprite;
            segment.centepede = this;
            segments.Add(segment);
        }
        for (int i = 0; i < segments.Count; i++)
        {
            CentepedeSegment segment = segments[i];
            segment.ahead = GetSegmentAt(i - 1);
            segment.behind=GetSegmentAt(i + 1);
        }
    }
    private CentepedeSegment GetSegmentAt(int index)
    {
        if(index>=0 && index < segments.Count)
        {
            return segments[index];
        }
        else
        {
            return null;
        }
    }
    public void Remove(CentepedeSegment segment)
    {
        Vector3 position=GridPosition(segment.transform.position);
        Instantiate(mushroomPrefab, position, Quaternion.identity);

        
        if (segment.ahead != null)
        {
            segment.ahead.behind = null;
        }
        if(segment.behind != null)
        {
            segment.behind.ahead = null;
            segment.behind.spriteRenderer.sprite = headSprite;
            segment.behind.UpdateHeadSegment();
        }

        segments.Remove(segment);
        Destroy(segment.gameObject);

    }
    private Vector2 GridPosition(Vector2 position)
    {
        position.x=Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);
        return position;
    }
}
