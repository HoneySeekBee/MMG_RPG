using DevionGames.InventorySystem;
using Packet;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private Animator _animator;

    public float speed = 5f;
    [SerializeField] private float walkSpeed = 3;
    [SerializeField] private float runSpeed = 5;
    private Vector3 _lastDirection;
    private Vector3 _moveDir;

    Vector3 _previousPosition;

    [SerializeField] private float smoothingFactor = 10f; // �ӵ� ���� ����
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float _rawSpeed = 0f;
    [SerializeField] private float _smoothedSpeed = 0f;
    [SerializeField] private bool _isLocalPlayer = false;
    public bool isLocalPlayer { get { return _isLocalPlayer; } }

    private Vector3 _networkTargetPos;
    private float _networkDirY;
    private float _networkSpeed;
    private bool _networkDirty = false;
    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        speed = walkSpeed;
        maxSpeed = runSpeed;
    }
    private void OnDestroy()
    {
        Debug.Log($"�ı��Ǿ����ϴ�.");
    }
    private void Update()
    {
        UpdateSpeed();
        if (_isLocalPlayer == false)
        {
            NotLocalPlayerMove();
        }
    }
    public void Initialize(bool isLocal)
    {
        _isLocalPlayer = isLocal;
        if (_isLocalPlayer)
        {
            InputManager.OnMoveInput += HandleMove;
            InputManager.OnAttackInput += HandleAttack;
            InputManager.OnRuninputDown += HandleRunStart;
            InputManager.OnRuninputUp += HandleRunStop;

            this.tag = "Player";
            InputManager.Instance.localController = this;
        }
    }
    void OnDisable()
    {
        InputManager.OnMoveInput -= HandleMove;
        InputManager.OnAttackInput -= HandleAttack;
        InputManager.OnRuninputDown -= HandleRunStart;
        InputManager.OnRuninputUp -= HandleRunStop;
    }
    private void UpdateSpeed()
    {
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(currentPosition, _previousPosition);
        _rawSpeed = distance / Time.deltaTime;  // ���� �ӵ� ����
        _previousPosition = currentPosition;

        // ���� ó�� (�ε巯�� ����/����)
        _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, _rawSpeed, smoothingFactor * Time.deltaTime);

        // 0~1 ���̷� ����ȭ �� Animator�� ����
        float normalizedSpeed = Mathf.Clamp01(_smoothedSpeed / maxSpeed);
        
        // _animator.SetFloat("speed", normalizedSpeed, 0.1f, Time.deltaTime);
    }
    Vector3 _lastSentPos;
    private float _lastSentDirY;
    void HandleMove(Vector2 input)
    {
        float vertical = input.y;    // W/S
        float horizontal = input.x;  // A/D

        // 1. ȸ�� ó�� (��/�� ȸ��)
        float rotationSpeed = 180f; // ȸ�� �ӵ� (��/��)
        transform.Rotate(Vector3.up, horizontal * rotationSpeed * Time.deltaTime);

        // 2. �̵� ó�� (��/����)
        _moveDir = transform.forward * vertical;
        if (Mathf.Abs(vertical) > 0.01f)
        {
            transform.position += _moveDir * speed * Time.deltaTime;
        }

        if (isLocalPlayer == false)
            return;
        if (NetworkManager.Instance == null)
            return;
        // ���⼭ ������ �ɵ� �ʹ�. 
        Vector3 direction = transform.forward;
        float dirY = transform.eulerAngles.y;

        if (Vector3.Distance(transform.position, _lastSentPos) > 0.01f ||
            Mathf.Abs(dirY - _lastSentDirY) > 0.5f) // 0.5�� �̻� ȸ���ϸ� ����
        {
            NetworkManager.Instance.Send_Move(transform.position, dirY, _rawSpeed);
            _lastSentPos = transform.position;
            _lastSentDirY = dirY; // float ������ ����
        }
    }

    void HandleAttack()
    {
        Debug.Log("Attacked!");
        // �ִϸ��̼� or ���� ��Ŷ ��
    }
    void HandleRunStart()
    {
        speed = runSpeed;
    }
    void HandleRunStop()
    {
        speed = walkSpeed;
    }

    #region None-Local Player Characeter ����
    public void NoneLocalPlayer_Move(Vector3 targetPos, float dirY, float speed)
    {
        _networkTargetPos = targetPos;
        _networkDirY = dirY;  // �� float �� ���� ����
        _networkSpeed = speed;

        Debug.Log($"[NoneLocalPlayer_Move] ���� ȸ���� Y : {dirY}");
    }
    private void NotLocalPlayerMove()
    {
        // �ε巯�� �̵� ó��
        transform.position = Vector3.Lerp(transform.position, _networkTargetPos, Time.deltaTime * _networkSpeed);

        // y ȸ������ �ݿ��ؼ� ȸ��
        Quaternion targetRot = Quaternion.Euler(0f, _networkDirY, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
    }
    #endregion
    public void DebugInventory(int id)
    {
        Debug.Log("������ ���� " + id);

    }
}
