using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

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
    private Sequence _swipeSequence;

    private HashSet<FieldCell> _mainHor;
    private HashSet<FieldCell> _mainVert;

    public enum TileType
    {
        Empty,
        Water,
        Fire,
    }

    private void Awake()
    {
        _mainHor = new HashSet<FieldCell>();
        _mainVert = new HashSet<FieldCell>();
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

                var random = Random.value > 0.5f;
                var spawnedObj = random ? _gameTileFireOriginPrefab : _gameTileWaterOriginPrefab;
                GameTile newTile = Instantiate(spawnedObj, _gameTilesParent);
                newTile.Init(random ? TileType.Fire : TileType.Water);
                newTile.name += $"_R_{i}_C_{j}";
                newTile.OnTileSwiped += OnTileWasSwiped;
                
                var positionF = new Vector3(newRowStartX, startY, 0);
                var newFieldCellF = SpawnFieldCell(positionF, i, j, zValue);
                newFieldCellF.SetTile(newTile);
                newFieldCellF.SetInnerTileToCellCenter();
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
            SwipeTilesAsync(newCell, cellOfTile).Forget();
            
        }
    }

    private async UniTask SwipeTilesAsync(FieldCell cellA, FieldCell cellB)
    {
        SwapTileBetweenCell(cellA, cellB);
        cellA.MoveToCell();
        cellB.MoveToCell();
        await UniTask.Delay(500);
        CheckFallTiles();
        await UniTask.Delay(500);
        CheckMatch_N();
        
        foreach (var fieldCell in _mainHor)
        {
            fieldCell.transform.localScale = Vector3.one * 0.6f;
            fieldCell.SetTile(null);
        }
        
        foreach (var fieldCell in _mainVert)
        {
            fieldCell.transform.localScale = Vector3.one * 0.6f;
            fieldCell.SetTile(null);
        }
        
        _mainHor.Clear();
        _mainVert.Clear();

        Debug.Log("Fall ended");
    }

    private void SwapTileBetweenCell(FieldCell cellA, FieldCell cellB)
    {
        var bufForSwap = cellB.TileOnCell;
        cellB.SetTile(cellA.TileOnCell);
        cellA.SetTile(bufForSwap);
    }

    private void CheckFallTiles()
    {
        for (int column = 0; column < _field.GetLength(1); column++)
        {
            for (int row = 0; row < _field.GetLength(0); row++)
            {
                var currentCell = _field[row, column];
                if (currentCell.TileOnCell == null && currentCell.Row < _field.GetLength(0) - 1)
                {
                    for (int rowUp = currentCell.Row; rowUp < _field.GetLength(1); rowUp++)
                    {
                        var cell = _field[rowUp, column];
                        if (cell.TileOnCell != null)
                        {
                            SwapTileBetweenCell(currentCell, cell);
                            currentCell.MoveToCell();
                            break;
                        }
                    }
                }
            }
        }
    }

    private void CheckMatch_N()
    {
        List<FieldCell> horizontal = new List<FieldCell>();
        List<FieldCell> vertical = new List<FieldCell>();

        TileType currentType = TileType.Empty;

        for (int i = 0; i < _field.GetLength(0); i++)
        {
            for (int j = 0; j < _field.GetLength(1); j++)
            {
                var currentFieldCell = _field[i, j];

                if (currentFieldCell.IsEmptyCell)
                {
                    currentType = TileType.Empty;
                    
                    if (horizontal.Count > 2)
                    {
                        _mainHor.AddRange(horizontal);
                    }
                    
                    horizontal.Clear();
                    
                    continue;
                }

                if (currentType == TileType.Empty)
                {
                    currentType = currentFieldCell.TileOnCell.TileType;
                }

                if (currentFieldCell.TileOnCell.TileType != currentType)
                {
                    if (horizontal.Count > 2)
                    {
                        _mainHor.AddRange(horizontal);
                    }
                    
                    horizontal.Clear();
                    currentType = currentFieldCell.TileOnCell.TileType;
                }
                
                horizontal.Add(currentFieldCell);
                
                if (_mainVert.Contains(currentFieldCell))
                {
                    continue;
                }
                
                vertical.Add(currentFieldCell);

                for (int rowUp = currentFieldCell.Row + 1; rowUp < _field.GetLength(1); rowUp++)
                {
                    var upColCell = _field[rowUp, j];

                    if (upColCell.IsEmptyCell)
                    {
                        break;
                    }

                    if (upColCell.TileOnCell.TileType == currentFieldCell.TileOnCell.TileType)
                    {
                        vertical.Add(upColCell);
                    }
                    else
                    {
                        break;
                    }
                }

                if (vertical.Count > 2)
                {
                    _mainVert.AddRange(vertical);
                }
                
                vertical.Clear();
            }
        }
    }
}
