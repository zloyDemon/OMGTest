using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _tileView;
    [SerializeField] private InputSwipe _inputSwipe;
    
    private FieldController.TileType _currentType;

    public float SizeX => _tileView.bounds.size.x;
    public float SizeY => _tileView.bounds.size.y;
    public FieldController.TileType TileType => _currentType;
    public event Action<GameTile, SwipeDirection> OnTileSwiped;

    private void Awake()
    {
        _inputSwipe.Swiped += OnSwipeDetected;
    }

    private void OnDestroy()
    {
        _inputSwipe.Swiped -= OnSwipeDetected;
    }

    public void Init(FieldController.TileType type)
    {
        _currentType = type;
    }

    public void MoveToFieldCell(FieldCell cell)
    {
        transform.position = cell.transform.position;
    }
    
    private void OnSwipeDetected(SwipeDirection direction)
    {
        OnTileSwiped?.Invoke(this, direction);
    }
}
