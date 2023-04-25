using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ModestTree;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

public class FieldController : MonoBehaviour
{
    [SerializeField] private Transform _fieldCellParent;
    [SerializeField] private Transform _fieldCellContainer;
    [SerializeField] private FieldCell _fieldCellOriginPrefab;
    [SerializeField] private GameTile _gameTileOriginPrefab;
    [SerializeField] private GameTilesManager _gameTilesManager;

    private Queue<FieldCell> _fieldCellsContainer = new Queue<FieldCell>();
    
    private FieldCell[,] _field;
    private Sequence _swipeSequence;

    private List<FieldData> _fieldDatas;
    private FieldData _currentLevelData;

    public bool IsInputEnable { get; private set; } = false;

    public enum TileType
    {
        Empty = 0,
        Water = 1,
        Fire = 2,
    }

    private void Awake()
    {
        InitGame().Forget();
    }

    private async UniTask InitGame()
    {
        await _gameTilesManager.Init();

        for (int i = 0; i < Constants.StartFieldCellsCount; i++)
        {
            AddFieldCellToContainer();
            await UniTask.DelayFrame(1);
        }

        string text = await File.ReadAllTextAsync(Constants.FilePath);
        _fieldDatas = JsonConvert.DeserializeObject<List<FieldData>>(text);

        LoadLevel(_fieldDatas.First().fieldNumber);
    }

    private void LoadLevel(int levelNum)
    {
        IsInputEnable = false;
        
        DeactivateField();
        _currentLevelData = _fieldDatas.FirstOrDefault(l => l.fieldNumber == levelNum);
        if (_currentLevelData == null)
        {
            Debug.LogError($"Level {levelNum} does not exist.");
            return;
        }
        
        BuildField(_currentLevelData.tiles);
        IsInputEnable = true;
    }
    
    public void NextLevel()
    {
        int currentLevel = _currentLevelData.fieldNumber;
        currentLevel++;
        if (currentLevel > _fieldDatas.Count - 1)
        {
            currentLevel = 1;
        }
        
        LoadLevel(currentLevel);
    }

    private void BuildField(int[,] tiles)
    {
        int rows = tiles.GetLength(0);
        int columns = tiles.GetLength(1);
        float startX = -(columns - 1);
        float startY = -7;
        float spaceX = _gameTileOriginPrefab.SizeX;
        float spaceY = _gameTileOriginPrefab.SizeX;
        float zValue = 0f;

        _field = new FieldCell[rows, columns + 2];

        for (int row = 0; row < rows; row++)
        {
            var newRowStartX = startX;
            
            for(int column = 0; column < columns + 2; column++)
            {
                zValue -= 0.1f;
                if (column == 0)
                {
                    var position = new Vector3(newRowStartX - 2, startY, 0);
                    var newFieldCell = SpawnFieldCell(position, row, column, zValue);
                    _field[row, column] = newFieldCell;
                    continue;
                }

                if (column == (columns + 1))
                {
                    var position = new Vector3(newRowStartX, startY, 0);
                    var newFieldCell = SpawnFieldCell(position, row, column, zValue);
                    _field[row, column] = newFieldCell;
                    continue;
                }

                var cellPosition = new Vector3(newRowStartX, startY, 0);
                var newCell = SpawnFieldCell(cellPosition, row, column, zValue);
                newCell.SetInnerTileToCellCenter();
                _field[row, column] = newCell;
                
                var type = (TileType) tiles[row, column - 1];
                if (type != TileType.Empty)
                {
                    var newTile = _gameTilesManager.GetTileByType(type);
                    newTile.OnTileSwiped += OnTileWasSwiped;
                    newTile.SetAliveState(true);
                    newCell.SetTile(newTile);
                    newCell.SetInnerTileToCellCenter();
                }
                
                newRowStartX += spaceX - 1;
            }
            
            startY += spaceY - 1;
        }
    }

    private FieldCell SpawnFieldCell(Vector3 position, int row, int column, float zValue)
    {
        if (_fieldCellsContainer.Count == 0)
        {
            AddFieldCellToContainer();
        }

        FieldCell fieldCell = _fieldCellsContainer.Dequeue();
        fieldCell.transform.SetParent(_fieldCellParent);
        fieldCell.gameObject.SetActive(true);
        fieldCell.name = $"Field_R_{row}_C_{column}";
        fieldCell.Init(row, column, zValue);
        fieldCell.transform.localPosition = position;
        return fieldCell;
    }

    private void DeactivateField()
    {
        if (_field == null)
        {
            return;
        }
        
        foreach (var fieldCell in _field)
        {
            DeactivateTileInCell(fieldCell);
            
            fieldCell.transform.SetParent(_fieldCellContainer);
            fieldCell.gameObject.SetActive(false);
            fieldCell.transform.localPosition = Vector3.zero;
            _fieldCellsContainer.Enqueue(fieldCell);
        }

        _field = null;
    }

    private void AddFieldCellToContainer()
    {
        var fieldCell = Instantiate(_fieldCellOriginPrefab, _fieldCellContainer);
        fieldCell.gameObject.SetActive(false);
        _fieldCellsContainer.Enqueue(fieldCell);
    }

    private void DeactivateTileInCell(FieldCell cell)
    {
        var tile = cell.TileOnCell;

        if (tile != null)
        {
            tile.OnTileSwiped -= OnTileWasSwiped;
            _gameTilesManager.ReturnToPool(tile);
        }

        cell.SetTile(null);
    }

    private void OnTileWasSwiped(GameTile tile, SwipeDirection direction)
    {
        if (!IsInputEnable)
        {
            return;
        }
        
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

            if ((nextCellRow < 0 || nextCellRow > _field.GetLength(0) - 1) ||
                (nextCellColumn < 0 || nextCellColumn > _field.GetLength(1) - 1))
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
        IsInputEnable = false;
        
        SwapTileBetweenCell(cellA, cellB);
        cellA.MoveToCell();
        cellB.MoveToCell();
        await UniTask.Delay(500);
        CheckFallTiles();
        await UniTask.Delay(500);
        
        bool isDone = false;

        while (!isDone)
        {
            var set = CheckMatch();
            isDone = set.IsEmpty();

            if (CheckFallTiles())
            {
                await UniTask.Delay(Constants.FallTilesMilliseconds);
            }

            foreach (var fieldCell in set)
            {
                fieldCell.TileOnCell.SetAliveState(false);
            }

            await UniTask.Delay(Constants.WaitAfterDestroyAnimationMilliseconds);
            
            foreach (var fieldCell in set)
            {
                DeactivateTileInCell(fieldCell);
            }
            
            if (CheckFallTiles())
            {
                await UniTask.Delay(Constants.FallTilesMilliseconds);
            }
        }

        if (IsEndGame())
        {
            NextLevel();
        }

        IsInputEnable = true;
    }

    private void SwapTileBetweenCell(FieldCell cellA, FieldCell cellB)
    {
        var bufForSwap = cellB.TileOnCell;
        cellB.SetTile(cellA.TileOnCell);
        cellA.SetTile(bufForSwap);
    }

    private bool CheckFallTiles()
    {
        bool wasFall = false;
        for (int column = 0; column < _field.GetLength(1); column++)
        {
            for (int row = 0; row < _field.GetLength(0); row++)
            {
                var currentCell = _field[row, column];
                if (currentCell.TileOnCell == null && currentCell.Row < _field.GetLength(0) - 1)
                {
                    for (int rowUp = currentCell.Row; rowUp < _field.GetLength(0); rowUp++)
                    {
                        var cell = _field[rowUp, column];
                        if (cell.TileOnCell != null)
                        {
                            SwapTileBetweenCell(currentCell, cell);
                            currentCell.MoveToCell();
                            wasFall = true;
                            break;
                        }
                    }
                }
            }
        }

        return wasFall;
    }

    private HashSet<FieldCell> CheckMatch()
    {
        List<FieldCell> horizontal = new List<FieldCell>();
        List<FieldCell> vertical = new List<FieldCell>();

        HashSet<FieldCell> fieldCells = new HashSet<FieldCell>();

        TileType currentType = TileType.Empty;

        for (int i = 0; i < _field.GetLength(0); i++)
        {
            for (int j = 0; j < _field.GetLength(1); j++)
            {
                var currentFieldCell = _field[i, j];

                if (currentFieldCell.IsEmptyCell)
                {
                    currentType = TileType.Empty;
                    AddRangeToMain(horizontal, fieldCells);
                    continue;
                }

                if (currentType == TileType.Empty)
                {
                    currentType = currentFieldCell.TileOnCell.TileType;
                }

                if (currentFieldCell.TileOnCell.TileType != currentType)
                {
                    AddRangeToMain(horizontal, fieldCells);
                    currentType = currentFieldCell.TileOnCell.TileType;
                }

                horizontal.Add(currentFieldCell);
                
                if (fieldCells.Contains(currentFieldCell))
                {
                    continue;
                }
                
                vertical.Add(currentFieldCell);

                for (int rowUp = currentFieldCell.Row + 1; rowUp < _field.GetLength(0); rowUp++)
                {
                    var upColCell = _field[rowUp, j];

                    if (upColCell.IsEmptyCell)
                    {
                        AddRangeToMain(vertical, fieldCells);
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

                AddRangeToMain(vertical, fieldCells);
            }
            
            horizontal.Clear();
        }

        return fieldCells;
    }

    private void AddRangeToMain(List<FieldCell> cells, HashSet<FieldCell> cellsSet)
    {
        if (cells.Count > Constants.FieldsCountForMatch)
        {
            cellsSet.AddRange(cells);
        }
        
        cells.Clear();
    }

    private bool IsEndGame()
    {
        bool isWin = true;
        foreach (var fieldCell in _field)
        {
            if (fieldCell.TileOnCell != null)
            {
                isWin = false;
                break;
            }
        }

        return isWin;
    }
}
