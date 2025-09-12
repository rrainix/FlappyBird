using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Header("Pipe Settings")]
    public static List<GameObject> pipes = new List<GameObject>();
    private Vector2 lastPipePosition => pipes.Count > 0 ? pipes[0].transform.position : Vector2.positiveInfinity;
    private float nextPipeX;
    [SerializeField] private GameObject pipePrefab;
    [SerializeField] private float pipeDistance = 5;
    [SerializeField] private float pipeGenerationDst = 100;
    [SerializeField] private float pipeDestructionDst = 100;
    [SerializeField] private Vector2 pipePosYRange = new Vector2(-5, 5);

    [Space(5)]
    [Header("BG Settings")]
    private static List<GameObject> bgs = new List<GameObject>();
    [SerializeField] private float bgGenerationDst = 5;
    [SerializeField] private float bgDestructionDst = 5;
    [SerializeField] private GameObject bgPrefab;
    [SerializeField] private float bgDistance = 10;
    [SerializeField] private float nextBgX;
    [SerializeField] private float bgPosY = 1;
    private Vector2 lastBgPosition => bgs.Count > 0 ? bgs[0].transform.position : Vector2.positiveInfinity;
    private float furthestBgX => bgs.Count > 0 ? bgs[bgs.Count - 1].transform.position.x : nextBgX;


    [Space(5)]
    [Header("Floor Tile Settings")]
    private static List<FloorTileData> floorTiles = new List<FloorTileData>();
    [SerializeField] private GameObject[] topFloorTileVariations;
    [SerializeField] private GameObject[] bottomFloorTileVariations;
    [SerializeField] private float bottomTileGenerationDst = 5;
    [SerializeField] private float bottomTileDestructionDst = 5;
    [SerializeField] private float bottomTileDistance = 1;
    [SerializeField] private float nextBottomTileX;
    [SerializeField] private float bottomTilePosY = -4.5f;
    private Vector2 lastBottomTilePosition => floorTiles.Count > 0 ? floorTiles[0].top.transform.position : Vector2.positiveInfinity;

    public static WorldGenerator instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        bgs.Clear();
        pipes.Clear();
        floorTiles.Clear();

        for (int i = 0; i < bgGenerationDst * 2; i++)
        {
            UpdateBg();
        }
        for (int i = 0; i < bottomTileGenerationDst * 2; i++)
        {
            UpdateTiles();
        }
    }

    private void OnEnable()
    {
        GameController.OnGameStart += OnStart;
    }

    private void OnDisable()
    {
        GameController.OnGameStart -= OnStart;
    }

    public void OnRetry()
    {
        int pipesCount = pipes.Count, bgsCount = bgs.Count, floorTilesCount = floorTiles.Count;
        for (int i = 0; i < pipesCount; i++)
            DestructLastPipe();
        for (int i = 0; i < bgsCount; i++)
            DestructLastBg();
        for (int i = 0; i < floorTilesCount; i++)
            DestructLastBottomTile();
    }

    private void OnStart()
    {
        nextPipeX = player.transform.position.x + (Camera.main.orthographicSize * 2f);
    }

    private void Update()
    {
        if (GameController.hasStarted)
        {
            UpdatePipes();
        }

        UpdateBg();
        UpdateTiles();
    }

    private void UpdatePipes()
    {
        float furthestPipeDst = nextPipeX - player.transform.position.x;
        float lastPipeDst = player.transform.position.x - lastPipePosition.x;

        if (furthestPipeDst <= pipeGenerationDst)
        {
            AddNextPipe();
        }
        if (lastPipeDst >= pipeDestructionDst)
        {
            DestructLastPipe();
        }
    }

    private void UpdateBg()
    {
        float furthestPipeDst = furthestBgX - player.transform.position.x;
        float lastPipeDst = player.transform.position.x - lastBgPosition.x;

        if (furthestPipeDst <= bgGenerationDst)
        {
            AddNextBg();
        }
        if (lastPipeDst >= bgDestructionDst)
        {
            DestructLastBg();
        }
    }
    private void UpdateTiles()
    {
        float furthestPipeDst = nextBottomTileX - player.transform.position.x;
        float lastPipeDst = player.transform.position.x - lastBottomTilePosition.x;

        if (furthestPipeDst <= bottomTileGenerationDst)
        {
            AddNextBottomTile();
        }
        if (lastPipeDst >= bottomTileDestructionDst)
        {
            DestructLastBottomTile();
        }
    }


    private void AddNextPipe()
    {
        float posY = Random.Range(pipePosYRange.x, pipePosYRange.y);
        GameObject newPipe = PoolManager.instance.mapObjectPool.Get(pipePrefab, new Vector3(nextPipeX, posY), Quaternion.identity);
        pipes.Add(newPipe);
        nextPipeX += pipeDistance;
    }
    private void DestructLastPipe()
    {
        GameObject pipe = pipes[0];
        pipes.Remove(pipe);
        PoolManager.instance.mapObjectPool.Release(pipePrefab, pipe);
    }

    private void AddNextBg()
    {
        float realNextBgX = furthestBgX + bgDistance;
        GameObject newBg = PoolManager.instance.mapObjectPool.Get(bgPrefab, new Vector3(realNextBgX, bgPosY), Quaternion.identity);
        newBg.GetComponent<ParallaxEffect>().Start();
        bgs.Add(newBg);
    }
    private void DestructLastBg()
    {
        GameObject bg = bgs[0];
        bgs.Remove(bg);
        PoolManager.instance.mapObjectPool.Release(bgPrefab, bg);
    }

    private void AddNextBottomTile()
    {
        var bottomVariation = bottomFloorTileVariations[Random.Range(0, bottomFloorTileVariations.Length)];
        var topVariation = topFloorTileVariations[Random.Range(0, topFloorTileVariations.Length)];

        GameObject topBottomTile = PoolManager.instance.mapObjectPool.Get(topVariation, new Vector3(nextBottomTileX, bottomTilePosY), Quaternion.identity);
        GameObject bottomTile = PoolManager.instance.mapObjectPool.Get(bottomVariation, new Vector3(nextBottomTileX, bottomTilePosY - bottomTileDistance), Quaternion.identity);
        floorTiles.Add(new FloorTileData { top = topBottomTile, bottom = bottomTile });
        nextBottomTileX += bottomTileDistance;
    }
    private void DestructLastBottomTile()
    {
        var floorTile = floorTiles[0];
        floorTiles.Remove(floorTile);
        PoolManager.instance.mapObjectPool.Release(GetPrefabFromArray(floorTile.top, topFloorTileVariations), floorTile.top);
        PoolManager.instance.mapObjectPool.Release(GetPrefabFromArray(floorTile.bottom, bottomFloorTileVariations), floorTile.bottom);
    }

    public GameObject GetPrefabFromArray(GameObject search, GameObject[] array)
    {
        string searchname = search.name.Replace("(Clone)", "");

        foreach (var item in array)
        {
            if (searchname == item.name)
            {
                return item;
            }
        }
        return null;
    }
}

public class FloorTileData
{
    public GameObject top, bottom;
}