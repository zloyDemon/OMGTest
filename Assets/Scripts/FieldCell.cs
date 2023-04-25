using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class FieldCell : MonoBehaviour
{
    private const float MoveDuration = 0.5f;
    
    private int _row;
    private int _column;
    private GameTile _tileOnCell;
    private float _zValue;
    private Tween _moveToCellTween;

    public int Row => _row;
    public int Column => _column;
    public GameTile TileOnCell => _tileOnCell;
    public bool IsEmptyCell => _tileOnCell == null;

    public void Init(int row, int column, float zValue)
    {
        _row = row;
        _column = column;
        _zValue = zValue;
    }

    public void SetTile(GameTile tile)
    {
        _tileOnCell = tile;
        
        if (tile != null)
        {
            tile.transform.SetParent(transform);
        }
    }

    public void MoveToCell()
    {
        if (_tileOnCell == null)
        {
            return;
        }

        KillAndNullTween();

        _moveToCellTween = _tileOnCell.transform.DOLocalMove(new Vector3(0, 0), MoveDuration).OnComplete(() =>
        {
            var position = _tileOnCell.transform.position;
            position.z = _zValue;
            _tileOnCell.transform.position = position;
        });
    }

    public void SetInnerTileToCellCenter()
    {
        if (_tileOnCell == null)
        {
            return;
        }

        _tileOnCell.transform.localPosition = new Vector3(0, 0, _zValue);
    }

    private void OnDisable()
    {
        KillAndNullTween();
    }

    private void KillAndNullTween()
    {
        if (_moveToCellTween != null)
        {
            _moveToCellTween.Kill();
            _moveToCellTween = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    
}
