using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
    private readonly int IsAliveAnimationHash = Animator.StringToHash("IsAlive");
    
    [SerializeField] private SpriteRenderer _tileView;
    [SerializeField] private InputSwipe _inputSwipe;
    [SerializeField] private FieldController.TileType _tileType;
    [SerializeField] private Animator _animator;

    public float SizeX => _tileView.bounds.size.x;
    public float SizeY => _tileView.bounds.size.y;
    public FieldController.TileType TileType => _tileType;
    public event Action<GameTile, SwipeDirection> OnTileSwiped;

    private void Awake()
    {
        _inputSwipe.Swiped += OnSwipeDetected;
    }

    private void OnDestroy()
    {
        _inputSwipe.Swiped -= OnSwipeDetected;
    }

    public void SetAliveState(bool isAlive)
    {
        _animator.SetBool(IsAliveAnimationHash, isAlive);
    }

    private void OnSwipeDetected(SwipeDirection direction)
    {
        OnTileSwiped?.Invoke(this, direction);
    }
}
