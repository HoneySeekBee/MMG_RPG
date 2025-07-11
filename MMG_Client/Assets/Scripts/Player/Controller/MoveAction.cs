
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using UnityEngine;

namespace MMG
{
    public class MoveAction : ActionBase<MoveData>
    {
        public float speed = 5f;
        private Vector3 _moveDir;
        Vector3 _lastSentPos;
        private float _lastSentDirY;
        private float _rawSpeed = 0f;
        private float walkSpeed = 30;
        private float runSpeed = 50;

        private float _smoothedSpeed = 0f;
        private float smoothingFactor = 10f; // 속도 보간 정도

        private Vector3 _networkTargetPos;
        private float _networkDirY;
        private float _networkSpeed;
        private float maxSpeed = 30f;
        private PlayerAnimator playerAnimator;

        private Vector3 _prevTargetPos;
        private float _interpolationT = 1f;
        public override void Initialize(bool isLocal, IInputBase input = null)
        {
            base.Initialize(isLocal, input);
            playerAnimator = GetComponent<PlayerAnimator>();
            maxSpeed = runSpeed;
            speed = walkSpeed;
        }
        #region Public
        private void FixedUpdate()
        {
            Update_Move();
        }
        private void Update_Move()
        {
            PlayerMove_FromServer();
        }
        public void UpdateAnimator()
        {
            float dir;

            // 실제 위치가 목표에 거의 도달했으면 speed = 0 처리
            float distToTarget = Vector3.Distance(transform.position, _networkTargetPos);
            float speed = (distToTarget < 0.01f) ? 0f : _networkSpeed / maxSpeed;

            Vector3 forward = transform.forward;
            Vector3 toTarget = (_networkTargetPos - transform.position).normalized;
            dir = Mathf.Sign(Vector3.Dot(forward, toTarget));

            playerAnimator.UpdateMoveAnimation(speed, dir);
        }
        #endregion
        protected override void Action(MoveData input)
        {
            float vertical = input.Direction.y;    // W/S
            float horizontal = input.Direction.x;  // A/D

            // 1. 회전 처리 (좌/우 회전)
            float rotationSpeed = 30;
            transform.Rotate(Vector3.up, horizontal * rotationSpeed * Time.fixedDeltaTime);

            // 2. 이동 방향 및 속도 계산
            _moveDir = transform.forward * vertical;
            speed = input.IsRunning ? runSpeed : walkSpeed;

            // 3. 시뮬레이션 위치 계산
            Vector3 simulatedPos = transform.position + transform.forward * vertical * speed * Time.fixedDeltaTime;

            // 4. 회전값과 속도 계산
            float dirY = transform.eulerAngles.y;
            _rawSpeed = speed;

            // 5. 서버에 전송 (변화가 감지되었을 때만)
            if (Vector3.Distance(simulatedPos, _lastSentPos) > 0.05f || Mathf.Abs(dirY - _lastSentDirY) > 0.5f)
            {
                NetworkManager.Instance.Send_Move(simulatedPos, dirY, _rawSpeed);
                _lastSentPos = simulatedPos;
                _lastSentDirY = dirY;
            }
        }
        private void PlayerMove_FromServer()
        { // 목적지 보간 진행
            _interpolationT += Time.fixedDeltaTime * smoothingFactor;
            _interpolationT = Mathf.Clamp01(_interpolationT);

            Vector3 smoothedPos = Vector3.Lerp(_prevTargetPos, _networkTargetPos, _interpolationT);
            transform.position = smoothedPos;

            // 회전 처리
            Quaternion targetRot = Quaternion.Euler(0f, _networkDirY, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 10f);

            if (playerAnimator != null)
                UpdateAnimator();
        }
        public override void SetMove(Vector3 goalPos, float dirY, float speed)
        {
            Debug.Log($"SetMove {goalPos}");
            Move_FromServer(goalPos, dirY, speed);
        }
        private void Move_FromServer(Vector3 targetPos, float dirY, float speed)
        {
            if (Vector3.Distance(_networkTargetPos, targetPos) < 0.01f &&
                Mathf.Abs(_networkDirY - dirY) < 0.5f &&
                Mathf.Abs(_networkSpeed - speed) < 0.1f)
            {
                // 거의 동일하니 무시 (덜덜방지)
                return;
            }

            _prevTargetPos = transform.position; // 현재 위치에서 시작 (더 부드러움)
            _networkTargetPos = targetPos;
            _networkDirY = dirY;
            _networkSpeed = speed;
            _interpolationT = 0f;
        }
    }


}