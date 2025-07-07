
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
        [SerializeField] private float smoothingFactor = 10f; // 속도 보간 정도

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
                // 1. 속도 정규화 (0~1)
                speed = Mathf.Clamp01(_smoothedSpeed / maxSpeed);

                // 2. 방향 계산: 전진이면 +1, 후진이면 -1
                dir = Mathf.Sign(Vector3.Dot(transform.forward, _moveDir.normalized));
            }
            else
            {
                float deltaDist = Vector3.Distance(transform.position, _lastPosition);
                speed = Mathf.Clamp01(deltaDist / Time.deltaTime / maxSpeed); // 최대 속도 기준 정규화

                // 방향 계산
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

            // 1. 회전 처리 (좌/우 회전)
            float rotationSpeed = 180f; // 회전 속도 (도/초)
            transform.Rotate(Vector3.up, horizontal * rotationSpeed * Time.deltaTime);

            // 2. 이동 처리 (전/후진)
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
            // 여기서 보내면 될듯 싶다. 
            Vector3 direction = transform.forward;
            float dirY = transform.eulerAngles.y;

            if (Vector3.Distance(transform.position, _lastSentPos) > 0.01f ||
                Mathf.Abs(dirY - _lastSentDirY) > 0.5f) // 0.5도 이상 회전하면 전송
            {
                NetworkManager.Instance.Send_Move(transform.position, dirY, _rawSpeed);
                _lastSentPos = transform.position;
                _lastSentDirY = dirY; // float 값으로 저장
            }
        }
        private void UpdateSpeed()
        {
            Vector3 currentPosition = transform.position;
            float distance = Vector3.Distance(currentPosition, _previousPosition);
            _rawSpeed = distance / Time.deltaTime;  // 실제 속도 측정
            _previousPosition = currentPosition;

            // 보간 처리 (부드러운 감속/가속)
            _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, _rawSpeed, smoothingFactor * Time.deltaTime);


            // _animator.SetFloat("speed", normalizedSpeed, 0.1f, Time.deltaTime);
        }
        private void NotLocalPlayerMove()
        {
            Vector3 currentPos = transform.position;

            // 1. 이동 처리
            transform.position = Vector3.Lerp(transform.position, _networkTargetPos, Time.deltaTime * _networkSpeed);

            // 2. 회전 처리
            Quaternion targetRot = Quaternion.Euler(0f, _networkDirY, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);

            // 3. 속도 계산용 (다음 프레임에서 사용됨)
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