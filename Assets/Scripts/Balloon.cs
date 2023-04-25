using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _skin;
    
    private bool _isMove;
    private float _frequency = 2f;
    private float _amplitude = 0.5f;
    private float _speed;
    private float _height;
    private Vector3 _endPosition;
    private BalloonController.BalloonDirection _direction;

    public BalloonController.BalloonDirection Direction => _direction;
    public event Action<Balloon> OnBallonReachedPoint;

    public void SetBalloon(float frequency, float amplitude, float speed, float height, Vector3 endPosition, BalloonController.BalloonDirection direction, Sprite skin)
    {
        _isMove = true;
        _frequency = frequency;
        _amplitude = amplitude;
        _skin.sprite = skin;
        _direction = direction;
        _height = height;
        _speed = speed;
        _endPosition = endPosition;
        gameObject.SetActive(true);
    }

    public void Stop()
    {
        _isMove = false;
        gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (!_isMove)
        {
            return;
        }
        
        var x = transform.position.x;
        x += _speed * Time.deltaTime * (int)_direction;
        transform.position = new Vector3(x,   _height + Mathf.Sin(Time.time * _frequency) * _amplitude , transform.position.z);

        if (_direction == BalloonController.BalloonDirection.Right && transform.position.x > _endPosition.x ||
            _direction == BalloonController.BalloonDirection.Left && transform.position.x < _endPosition.x)
        {
            Stop();
            OnBallonReachedPoint?.Invoke(this);
        }
    }
}
