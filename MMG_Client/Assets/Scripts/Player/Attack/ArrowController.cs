using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public int _attackerId = 0;

    public float _speed;
    public float _maxDistance;

    private Vector3 _direction;
    private Vector3 startPos;

    public void Init(int attackerId, Vector3 direction, float speed, float maxDistance, float damage, Action onComplete)
    {
        _attackerId =attackerId;
        _speed = speed;
        _maxDistance = maxDistance;
        _direction = direction;
        startPos = transform.position;
    }

    private void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;

        if (Vector3.Distance(startPos, transform.position) > _maxDistance)
        {
            ArrowPool.Instance.ReturnArrow(gameObject);
        }
    }
}
