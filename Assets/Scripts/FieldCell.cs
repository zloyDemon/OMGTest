using System;
using UnityEngine;
using Zenject;

public class FieldCell : MonoBehaviour
{
    private int _row;
    private int _column;
    private GameTile _tileOnCell;
    private float _zValue;

    public int Row => _row;
    public int Column => _column;
    public GameTile TileOnCell => _tileOnCell;

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
            tile.transform.localPosition = new Vector3(0, 0, _zValue);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
