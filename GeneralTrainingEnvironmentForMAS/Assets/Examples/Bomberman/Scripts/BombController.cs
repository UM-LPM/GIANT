using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombController : MonoBehaviour
{
    [Header("Bomb")]
    [SerializeField] GameObject BombPrefab;
    [SerializeField] KeyCode BombKey = KeyCode.Space;
    [SerializeField] float BombFuseTime = 3f;
    [SerializeField] int MaxPlayerBombAmout = 1;

    [Header("Explosion")]
    [SerializeField] Explosion ExplosionPrefab;
    [SerializeField] LayerMask ExplosionMask;
    [SerializeField] float ExplosionDuration = 1f;
    [SerializeField] public int ExplosionRadius = 1;

    [Header("Destructible")]
    [SerializeField] Tilemap DestructibleTiles;
    [SerializeField] Destructible DestructiblePrefab;

    int BombsRemaining;

    private void OnEnable() {
        BombsRemaining = MaxPlayerBombAmout;
    }

    private void Update() {
        if(BombsRemaining > 0 && Input.GetKeyDown(BombKey)) {
            StartCoroutine(PlaceBomb());
        }
    }

    private IEnumerator PlaceBomb() {
        Vector2 position = transform.position;
        position.x = MathF.Round(position.x);
        position.y = MathF.Round(position.y);

        GameObject bomb = Instantiate(BombPrefab, position, Quaternion.identity);
        BombsRemaining--;

        yield return new WaitForSeconds(BombFuseTime);

        position = bomb.transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        // Handle explosion logic
        Explosion explosion = Instantiate(ExplosionPrefab, position, Quaternion.identity);
        explosion.SetActiveRenderer(explosion.Start);
        Destroy(explosion.gameObject, ExplosionDuration);

        Explode(position, Vector2.up, ExplosionRadius);
        Explode(position, Vector2.down, ExplosionRadius);
        Explode(position, Vector2.left, ExplosionRadius);
        Explode(position, Vector2.right, ExplosionRadius);

        Destroy(bomb);
        BombsRemaining++;

    }

    public void Explode(Vector2 position, Vector2 direction, int length) {
        if(length <= 0)
            return;

        position += direction;

        if(Physics2D.OverlapBox(position, Vector2.one / 2f, 0f, ExplosionMask)) {
            ClearDestructible(position);
            return;
        }

        Explosion explosion = Instantiate(ExplosionPrefab, position, Quaternion.identity);
        explosion.SetActiveRenderer(length > 1? explosion.Middle: explosion.End);
        explosion.SetDirection(direction);
        Destroy(explosion.gameObject, ExplosionDuration);

        Explode(position, direction, length - 1);
    }

    public void ClearDestructible(Vector2 position) {
        Vector3Int cell = DestructibleTiles.WorldToCell(position);
        TileBase tile = DestructibleTiles.GetTile(cell);

        if(tile != null) {
            Instantiate(DestructiblePrefab, position, Quaternion.identity);
            DestructibleTiles.SetTile(cell, null);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.name.Contains("Bomb")){
            other.isTrigger = false;
        }
    }

    public void AddBomb() {
        MaxPlayerBombAmout++;
        BombsRemaining++;
    }


}
