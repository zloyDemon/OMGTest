using System.Collections.Generic;
using UnityEngine;

public class BalloonController : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _leftPoint;
    [SerializeField] private Transform _rightPoint;
    [SerializeField] private Balloon[] _balloons;
    [SerializeField] private Sprite _blueBalloonSprite;
    [SerializeField] private Sprite _orangeBalloonSprite;

    public enum BalloonDirection
    {
        Right = 1, 
        Left = -1,
    }

    private Queue<Balloon> _balloonsPool = new(3);
    private float _timer;

    private void Awake()
    {
        foreach (var balloon in _balloons)
        {
            balloon.OnBallonReachedPoint += OnBalloonReachedPoint;
            _balloonsPool.Enqueue(balloon);
            balloon.gameObject.SetActive(false);
        }
        
        UpdateTimer();
    }

    private void OnDestroy()
    {
        foreach (var balloon in _balloons)
        {
            balloon.OnBallonReachedPoint -= OnBalloonReachedPoint;
        }
    }

    private void Update()
    {
        if (_timer <= 0)
        {
            if (_balloonsPool.Count > 0)
            {
                SetupNewBalloon();
            }
            
            UpdateTimer();
        }

        _timer -= Time.deltaTime;
    }

    private void OnBalloonReachedPoint(Balloon balloon)
    {
        _balloonsPool.Enqueue(balloon);
    }

    private void SetupNewBalloon()
    {
        if (_balloonsPool.Count > 0)
        {
            float frequency = Random.Range(1, 2);
            float amplitude = Random.Range(0.6f, 1f);
            float speed = Random.Range(0.5f ,2);;
            float height = Random.Range(1, 3);;
            BalloonDirection _direction = Random.value > 0.5 ? BalloonDirection.Left : BalloonDirection.Right;
            Sprite skin = Random.value > 0.5 ? _blueBalloonSprite : _orangeBalloonSprite;
            Vector3 endPoint = _direction == BalloonDirection.Right ? _rightPoint.position : _leftPoint.position;
            var balloon = _balloonsPool.Dequeue();
            balloon.transform.position =
                _direction == BalloonDirection.Right ? _leftPoint.position : _rightPoint.position;
            balloon.SetBalloon(frequency,amplitude, speed, height, endPoint, _direction, skin);
        }
    }

    private void UpdateTimer()
    {
        _timer = Random.Range(3, 5);
    }
}
