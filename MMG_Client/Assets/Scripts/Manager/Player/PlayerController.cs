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

    [SerializeField] private float smoothingFactor = 10f; // 속도 보간 정도
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float _rawSpeed = 0f;
    [SerializeField] private float _smoothedSpeed = 0f;
    [SerializeField] private bool _isLocalPlayer = false;
    public bool isLocalPlayer { get { return _isLocalPlayer; } }

    private Vector3 _networkTargetPos;
    private Vector3 _networkDirection;
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
        Debug.Log($"파괴되었습니다.");
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
        _rawSpeed = distance / Time.deltaTime;  // 실제 속도 측정
        _previousPosition = currentPosition;

        // 보간 처리 (부드러운 감속/가속)
        _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, _rawSpeed, smoothingFactor * Time.deltaTime);

        // 0~1 사이로 정규화 후 Animator에 전달
        float normalizedSpeed = Mathf.Clamp01(_smoothedSpeed / maxSpeed);
        
        // _animator.SetFloat("speed", normalizedSpeed, 0.1f, Time.deltaTime);
    }
    Vector3 _lastSentPos;
    Vector3 _lastSentDir;
    void HandleMove(Vector2 input)
    {
        float vertical = input.y;    // W/S
        float horizontal = input.x;  // A/D

        // 1. 회전 처리 (좌/우 회전)
        float rotationSpeed = 180f; // 회전 속도 (도/초)
        transform.Rotate(Vector3.up, horizontal * rotationSpeed * Time.deltaTime);

        // 2. 이동 처리 (전/후진)
        _moveDir = transform.forward * vertical;
        if (Mathf.Abs(vertical) > 0.01f)
        {
            transform.position += _moveDir * speed * Time.deltaTime;
        }

        if (isLocalPlayer == false)
            return;
        if (NetworkManager.Instance == null)
            return;
        // 여기서 보내면 될듯 싶다. 
        Vector3 direction = transform.forward;

        if (Vector3.Distance(transform.position, _lastSentPos) > 0.01f || Vector3.Distance(direction, _lastSentDir) > 0.01f)
        {
            //NetworkManager.Instance.Send_Move(transform.position, direction, _rawSpeed);
            _lastSentPos = transform.position;
            _lastSentDir = direction;
        }
    }

    void HandleAttack()
    {
        Debug.Log("Attacked!");
        // 애니메이션 or 서버 패킷 등
    }
    void HandleRunStart()
    {
        speed = runSpeed;
    }
    void HandleRunStop()
    {
        speed = walkSpeed;
    }

    #region None-Local Player Characeter 관련
    public void NoneLocalPlayer_Move(S_BroadcastMove movePacket)
    {
        _networkTargetPos.x = movePacket.PosX;
        _networkTargetPos.y = movePacket.PosY;
        _networkTargetPos.z = movePacket.PosZ;
        _networkDirection.y = movePacket.DirY;
        _networkSpeed = movePacket.Speed;

        Debug.Log($"움직일게 {isLocalPlayer} : {_networkSpeed} : {_networkTargetPos.x} : {_networkTargetPos.y} : {_networkTargetPos.z}");
    }
    private void NotLocalPlayerMove()
    {
        // 부드러운 이동 처리
        transform.position = Vector3.Lerp(transform.position, _networkTargetPos, Time.deltaTime * _networkSpeed);

        // 회전 방향 적용 (optional)
        if (_networkDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(_networkDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }
    #endregion
    public void DebugInventory(int id)
    {
        Debug.Log("아이템 선택 " + id);

    }
}
