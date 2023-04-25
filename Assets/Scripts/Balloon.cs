using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    [SerializeField] private float _frequency;
    [SerializeField] private float _ampl;
    
    void Update()
    {
        var x = transform.position.x;
        x += 1 * Time.deltaTime;
        transform.position = new Vector3(x, transform.position.y + Mathf.Sin(Time.time * _frequency) * _ampl , transform.position.z);
    }
}
