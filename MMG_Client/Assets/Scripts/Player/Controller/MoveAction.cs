
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMG
{
    public class MoveAction : ActionBase<MoveData>
    {
        public float speed = 5f;
        private Vector3 _moveDir;
        Vector3 _lastSentPos;
        private float _lastSentDirY;
        [SerializeField] private float _rawSpeed = 0f;
        [SerializeField] private float walkSpeed = 3;
        [SerializeField] private float runSpeed = 5;
        Vector3 _previousPosition;

        [SerializeField] private float _smoothedSpeed = 0f;
        [SerializeField] private float smoothingFactor = 10f; // �ӵ� ���� ����

        private Vector3 _networkTargetPos;
        private float _networkDirY;
        private float _networkSpeed;

        [SerializeField] private float maxSpeed = 10f;
        private PlayerAnimator playerAnimator;

        private Vector3 _lastPosition;
        public override void Initialize(bool isLocal, IInputBase input)
        {
            base.Initialize(isLocal, input);
            playerAnimator = GetComponent<PlayerAnimator>();
            maxSpeed = runSpeed;
            speed = walkSpeed;
        }
        #region Public
        private void Update()
        {
            Update_Move();
            if (playerAnimator != null)
            {
                UpdateAnimator();
            }
        }
        private void Update_Move()
        {
            UpdateSpeed();
            if (IsLocal == false)
            {
                NotLocalPlayerMove();
            }
        }
        public void UpdateAnimator()
        {
            float speed, dir;

            if (IsLocal)
            {
                // 1. �ӵ� ����ȭ (0~1)
                speed = Mathf.Clamp01(_smoothedSpeed / maxSpeed);

                // 2. ���� ���: �����̸� +1, �����̸� -1
                dir = Mathf.Sign(Vector3.Dot(transform.forward, _moveDir.normalized));
            }
            else
            {
                float deltaDist = Vector3.Distance(transform.position, _lastPosition);
                speed = Mathf.Clamp01(deltaDist / Time.deltaTime / maxSpeed); // �ִ� �ӵ� ���� ����ȭ

                // ���� ���
                Vector3 forward = transform.forward;
                Vector3 toTarget = (_networkTargetPos - transform.position).normalized;
                dir = Mathf.Sign(Vector3.Dot(forward, toTarget));
            }
            playerAnimator.UpdateMoveAnimation(speed, dir);
        }
        #endregion
        protected override void Action(MoveData input)
        {
            float vertical = input.Direction.y;    // W/S
            float horizontal = input.Direction.x;  // A/D

            // 1. ȸ�� ó�� (��/�� ȸ��)
            float rotationSpeed = 180f; // ȸ�� �ӵ� (��/��)
            transform.Rotate(Vector3.up, horizontal * rotationSpeed * Time.deltaTime);

            // 2. �̵� ó�� (��/����)
            _moveDir = transform.forward * vertical;
            speed = input.IsRunning ? runSpeed : walkSpeed;
            if (Mathf.Abs(vertical) > 0.01f)
            {
                transform.position += _moveDir * speed * Time.deltaTime;
            }

            if (IsLocal == false)
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
        private void UpdateSpeed()
        {
            Vector3 currentPosition = transform.position;
            float distance = Vector3.Distance(currentPosition, _previousPosition);
            _rawSpeed = distance / Time.deltaTime;  // ���� �ӵ� ����
            _previousPosition = currentPosition;

            // ���� ó�� (�ε巯�� ����/����)
            _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, _rawSpeed, smoothingFactor * Time.deltaTime);


            // _animator.SetFloat("speed", normalizedSpeed, 0.1f, Time.deltaTime);
        }
        private void NotLocalPlayerMove()
        {
            Vector3 currentPos = transform.position;

            // 1. �̵� ó��
            transform.position = Vector3.Lerp(transform.position, _networkTargetPos, Time.deltaTime * _networkSpeed);

            // 2. ȸ�� ó��
            Quaternion targetRot = Quaternion.Euler(0f, _networkDirY, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);

            // 3. �ӵ� ���� (���� �����ӿ��� ����)
            _lastPosition = currentPos;

        }
        public override void SetMove(Vector3 goalPos, float dirY, float speed)
        {
            NoneLocalPlayer_Move(goalPos, dirY, speed);
        }
        private void NoneLocalPlayer_Move(Vector3 targetPos, float dirY, float speed)
        {
            _networkTargetPos = targetPos;
            _networkDirY = dirY;
            _networkSpeed = speed;
        }
    }


}