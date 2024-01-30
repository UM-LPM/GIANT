using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombExplosionController : MonoBehaviour {
    [Header("Bomb")]
    [SerializeField] BombComponent BombPrefab;
    [SerializeField] float BombFuseTime = 3f;

    [Header("Explosion")]
    [SerializeField] ExplosionComponent ExplosionPrefab;
    [SerializeField] float ExplosionDuration = 1f;

    [Header("Destructible")]
    [SerializeField] Tilemap DestructibleTiles;
    [SerializeField] DestructibleComponent DestructiblePrefab;

    Util Util;

    private void Awake() {
        Util = GetComponent<Util>();
    }

    public IEnumerator PlaceBomb(BombermanAgentComponent agent) {
        agent.AgentFitness.Fitness.UpdateFitness(BombermanFitness.BOMB_PLACED);

        Vector2 position = agent.transform.position;
        position.x = MathF.Round(position.x);
        position.y = MathF.Round(position.y);

        BombComponent bomb = Instantiate(BombPrefab, position, Quaternion.identity, transform);

        EnvironmentControllerBase.SetLayerRecursively(bomb.gameObject, gameObject.layer);
        agent.BombsRemaining--;

        yield return new WaitForSeconds(BombFuseTime);

        position = bomb.transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        // Handle explosion logic
        ExplosionComponent explosion = Instantiate(ExplosionPrefab, position, Quaternion.identity, transform);
        explosion.Parent = agent;
        EnvironmentControllerBase.SetLayerRecursively(explosion.gameObject, gameObject.layer);

        explosion.SetActiveRenderer(explosion.Start);
        Destroy(explosion.gameObject, ExplosionDuration);

        Explode(position, Vector2.up, agent.ExplosionRadius, agent);
        Explode(position, Vector2.down, agent.ExplosionRadius, agent);
        Explode(position, Vector2.left, agent.ExplosionRadius, agent);
        Explode(position, Vector2.right, agent.ExplosionRadius, agent);

        Destroy(bomb.gameObject);
        agent.BombsRemaining++;

    }

    public void Explode(Vector2 position, Vector2 direction, int length, BombermanAgentComponent agent) {
        if (length <= 0)
            return;

        position += direction;

        Collider2D collider = Physics2D.OverlapBox(position, Vector2.one / 2f, 0f, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer)));
        if(collider != null && !collider.isTrigger) {
            BombermanAgentComponent hitAgent;
            collider.TryGetComponent<BombermanAgentComponent>(out hitAgent);
            if (!hitAgent) {
                ClearDestructible(position, agent);
                return;
            }
        }

        ExplosionComponent explosion = Instantiate(ExplosionPrefab, position, Quaternion.identity, transform);
        explosion.Parent = agent;
        EnvironmentControllerBase.SetLayerRecursively(explosion.gameObject, gameObject.layer);

        explosion.SetActiveRenderer(length > 1 ? explosion.Middle : explosion.End);
        explosion.SetDirection(direction);
        Destroy(explosion.gameObject, ExplosionDuration);

        Explode(position, direction, length - 1, agent);
    }

    public void ClearDestructible(Vector2 position, BombermanAgentComponent agent) {
        Vector3Int cell = DestructibleTiles.WorldToCell(position);
        TileBase tile = DestructibleTiles.GetTile(cell);

        if (tile != null) {
            DestructibleComponent destructible = Instantiate(DestructiblePrefab, position, Quaternion.identity, transform);
            destructible.Util = Util;
            EnvironmentControllerBase.SetLayerRecursively(destructible.gameObject, gameObject.layer);

            DestructibleTiles.SetTile(cell, null);

            agent.AgentFitness.Fitness.UpdateFitness(BombermanFitness.BLOCK_DESTROYED);
        }
    }

}