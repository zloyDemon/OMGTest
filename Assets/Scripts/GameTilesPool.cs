using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class GameTilesPool : MonoBehaviour
{
    [SerializeField] private int _countSpawnTiles = 20;
    [SerializeField] private GameTile _waterGameTilePrefab;
    [SerializeField] private GameTile _fireGameTilePrefab;

    private Dictionary<FieldController.TileType, Queue<GameTile>> _tiles = new Dictionary<FieldController.TileType, Queue<GameTile>>(2);

    public async UniTask Init()
    {
        _tiles.Add(FieldController.TileType.Fire, new Queue<GameTile>());
        _tiles.Add(FieldController.TileType.Water, new Queue<GameTile>());

        for (int i = 0; i < _countSpawnTiles; i++)
        {
            _tiles[FieldController.TileType.Water].Enqueue(SpawnWaterTile());
            _tiles[FieldController.TileType.Fire].Enqueue(SpawnFireTile());

            await UniTask.DelayFrame(1);
        }
    }

    public GameTile GetTileByType(FieldController.TileType type)
    {
        if (_tiles[type].Count == 0)
        {
            _tiles[type].Enqueue(SpawnTileByType(type));
        }

        var tile = _tiles[type].Dequeue();
        tile.gameObject.SetActive(true);
        return tile;
    }

    public void ReturnToPool(GameTile tile)
    {
        tile.transform.SetParent(transform);
        tile.transform.localPosition = Vector3.zero;
        tile.gameObject.SetActive(false);
        _tiles[tile.TileType].Enqueue(tile);
    }

    private GameTile SpawnTileByType(FieldController.TileType type)
    {
        GameTile result = type switch
        {
            FieldController.TileType.Water => SpawnWaterTile(),
            FieldController.TileType.Fire => SpawnFireTile(),
        };

        return result;
    }

    private GameTile SpawnFireTile()
    {
        var fireTile = Instantiate(_fireGameTilePrefab, transform);
        fireTile.gameObject.SetActive(false);
        return fireTile;
    }

    private GameTile SpawnWaterTile()
    {
        var waterTile = Instantiate(_waterGameTilePrefab, transform);
        waterTile.gameObject.SetActive(false);
        return waterTile;
    }
}
