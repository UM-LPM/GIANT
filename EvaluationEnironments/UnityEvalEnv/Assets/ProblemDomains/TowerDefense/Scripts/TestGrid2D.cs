using Problems.TowerDefense;
using Problems.Utils.GridSystem;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Grid2D))]
public class TestGrid2D : MonoBehaviour
{
    [Header("Base Configuration")]
    [SerializeField] Vector3Int BasePosition;
    [SerializeField] GameObject BasePrefab;

    [Header("Tower Configuration")]
    [SerializeField] GameObject TowerPrefab;
    [SerializeField] GameObject TowerBulletPrefab;

    [Header("Enemy Configuration")]
    [SerializeField] Vector3Int EnemySpawnPointFlagPosition;
    [SerializeField] Vector3Int EnemySpawnPosition;
    [SerializeField] GameObject EnemySpawnPointFlagPrefab;

    [SerializeField] float EnemyMoveInterval;

    [SerializeField] GameObject EnemyWeakPrefab;
    [SerializeField] GameObject EnemyNormalPrefab;
    [SerializeField] GameObject EnemyStrongPrefab;

    private Grid2D Grid2D;

    private BaseComponent Base;

    private List<EnemyComponent> Enemys;

    private float currentEnemyUpdateTime = 0f;

    private void Awake() 
    {
        Grid2D = GetComponent<Grid2D>();

        if(Grid2D == null)
        {
            Debug.LogError("Grid2D component not found.");
            // TODO Add error handling
        }

        Enemys = new List<EnemyComponent>();
    }

    private void Start()
    {
        SpawnBase();

        SpawnEnemySpawnPoint();
        SpawnEnemy(EnemyWeakPrefab);
    }

    private void Update() 
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button click
        {
            int gridX, gridY;
            if (Grid2D.IsMousePositionInGrid(out gridX, out gridY))
            {
                Debug.Log($"Clicked on grid cell: ({gridX}, {gridY})");

                GameObject g = Grid2D.GetGridObject(gridX, gridY);
                if(g != null)
                {
                    Debug.Log($"Grid already contains an object: {g.name}");
                }
                else
                {
                    SpawnTower(gridX, gridY);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        currentEnemyUpdateTime += Time.fixedDeltaTime;

        if(currentEnemyUpdateTime >= EnemyMoveInterval)
        {
            MoveEnemy();
            currentEnemyUpdateTime = 0f;
        }

    }

    private void SpawnBase()
    {
        GameObject baseGO = Grid2D.AddGridObject(BasePosition.x, BasePosition.y, BasePrefab, false);
        Base = baseGO.GetComponent<BaseComponent>();
    }

    private void SpawnTower(int gridX, int gridY)
    {
        Grid2D.AddGridObject(gridX, gridY, TowerPrefab, false);
    }

    public void SpawnEnemySpawnPoint()
    {
        Grid2D.AddGridObject(EnemySpawnPointFlagPosition.x, EnemySpawnPointFlagPosition.y, EnemySpawnPointFlagPrefab, false);
    }

    public void SpawnEnemy(GameObject enemyPrefab)
    {
        GameObject enemyGO = Grid2D.AddGridObject(EnemySpawnPosition.x, EnemySpawnPosition.y, enemyPrefab, false);

        Enemys.Add(enemyGO.GetComponent<EnemyComponent>());
    }

    public void MoveEnemy()
    {
        Vector3Int newEnemyPosition = Vector3Int.zero;
        Vector3Int enemyCurrPosition;
        Vector3Int basePosition = Vector3Int.FloorToInt(Base.transform.position);

        foreach (EnemyComponent enemy in Enemys)
        {
            enemyCurrPosition = Grid2D.GetCellFromWorldPosition(enemy.transform.position.x, enemy.transform.position.y);
            newEnemyPosition = Grid2D.FindPath(enemyCurrPosition, basePosition);
            Grid2D.MoveGridObject(enemyCurrPosition, newEnemyPosition);
        }
    }
}