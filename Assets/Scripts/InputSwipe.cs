using System;
using UnityEngine;

public class InputSwipe : MonoBehaviour
{
    private Vector2 _startPosition;
    private bool _isTouching;
    private float _currentMiliseconds;

    public event Action<SwipeDirection> Swiped; 

    private void OnMouseDown()
    {
        _startPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _isTouching = true;
    }

    private void Update()
    {
        if (_isTouching)
        {
            _currentMiliseconds += Time.deltaTime;
        }
    }

    private void OnMouseUp()
    {
        _isTouching = false;
        
        var upPosition = (Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var dir = upPosition - _startPosition;
        
        if (_currentMiliseconds > 1 || dir.magnitude < 1)
        {
            _currentMiliseconds = 0;
            return;
        }
        
        var angleValue = Vector2.SignedAngle(Vector2.up, dir.normalized);
        _currentMiliseconds = 0;
        var direction = GetSwipeDirection(angleValue);
        Swiped?.Invoke(direction);
    }
    
    private SwipeDirection GetSwipeDirection(float angleValue)
    {
        SwipeDirection result = SwipeDirection.Up;

        if (angleValue is < 45 and > -45)
        {
            result = SwipeDirection.Up;
        }
        else if (angleValue is > -135 and < -45)
        {
            result = SwipeDirection.Right;
        }
        else if(angleValue is < -135 and > -180 || angleValue is > 135 and < 180)
        {
            result = SwipeDirection.Down;
        }
        else if(angleValue is > 45 and < 135)
        {
            result = SwipeDirection.Left;
        }
        
        return result;
    }
}
