using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FieldController : MonoBehaviour
{
    [SerializeField] private int _columns;
    [SerializeField] private int _rows;
    [SerializeField] private Transform _fieldCellParent;
    [SerializeField] private Transform _gameTilesParent;
    [SerializeField] private GameTile _gameTileFireOriginPrefab;
    [SerializeField] private GameTile _gameTileWaterOriginPrefab;
    [SerializeField] private FieldCell _fieldCellOriginPrefab;

    private FieldCell[,] _field;

    public enum TileType
    {
        Empty,
        Water,
        Fire,
    }

    private void Awake()
    {
        BuildField();
    }

    private void BuildField()
    {
        float startX = -(_columns - 1);
        float startY = -7;
        float spaceX = _gameTileFireOriginPrefab.SizeX;
        float spaceY = _gameTileFireOriginPrefab.SizeX;
        float zValue = -0f;

        _field = new FieldCell[_rows, _columns + 2];

        for (int i = 0; i < _rows; i++)
        {
            var newRowStartX = startX;
            
            for(int j = 0; j < _columns + 2; j++)
            {
                zValue -= 0.1f;
                if (j == 0)
                {
                    var position = new Vector3(newRowStartX - 2, startY, 0);
                    var newFieldCell = SpawnFieldCell(position, i, j, zValue);
                    _field[i, j] = newFieldCell;
                    continue;
                }

                if (j == (_columns + 1))
                {
                    var position = new Vector3(newRowStartX, startY, 0);
                    var newFieldCell = SpawnFieldCell(position, i, j, zValue);
                    _field[i, j] = newFieldCell;
                    continue;
                }

                var spawnedObj = Random.value > 0.5f ? _gameTileFireOriginPrefab : _gameTileWaterOriginPrefab;
                GameTile newTile = Instantiate(spawnedObj, _gameTilesParent);
                newTile.name += $"_R_{i}_C_{j}";
                newTile.OnTileSwiped += OnTileWasSwiped;
                
                var positionF = new Vector3(newRowStartX, startY, 0);
                var newFieldCellF = SpawnFieldCell(positionF, i, j, zValue);
                newFieldCellF.SetTile(newTile);
                _field[i, j] = newFieldCellF;

                newRowStartX += spaceX - 1;
            }
            
            startY += spaceY - 1;
        }
    }
    
    private FieldCell SpawnFieldCell(Vector3 position, int row, int column, float zValue)
    {
        FieldCell fieldCell = Instantiate(_fieldCellOriginPrefab, _fieldCellParent);
        fieldCell.name += $"_R_{row}_C_{column}";
        fieldCell.Init(row, column, zValue);
        fieldCell.transform.localPosition = position;
        return fieldCell;
    }

    private void OnTileWasSwiped(GameTile tile, SwipeDirection direction)
    {
        FieldCell cellOfTile = null;

        foreach (var fieldCell in _field)
        {
            if (fieldCell.TileOnCell == tile)
            {
                cellOfTile = fieldCell;
                break;
            }
        }

        if (cellOfTile != null)
        {
            int nextCellRow = cellOfTile.Row;
            int nextCellColumn = cellOfTile.Column;
            
            switch (direction)
            {
                case SwipeDirection.Up:
                    nextCellRow += 1;
                    break;
                case SwipeDirection.Right:
                    nextCellColumn += 1;
                    break;
                case SwipeDirection.Down:
                    nextCellRow -= 1;
                    break;
                case SwipeDirection.Left:
                    nextCellColumn -= 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            if ((nextCellRow < 0 || nextCellRow > _field.GetLength(1) - 1) ||
                (nextCellColumn < 0 || nextCellColumn > _field.GetLength(0) - 1))
            {
                return;
            }

            if (direction == SwipeDirection.Up && _field[cellOfTile.Row + 1, cellOfTile.Column].TileOnCell == null)
            {
                return;
            }
            
            var newCell = _field[nextCellRow, nextCellColumn];
            var bufForSwap = newCell.TileOnCell;
            newCell.SetTile(cellOfTile.TileOnCell);
            cellOfTile.SetTile(bufForSwap);
        }
    }
}
