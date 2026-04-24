using UnityEngine;

public abstract class BaseMovementState
{
    InputData _inputData;

    [Header("Option")]
    [SerializeField]
    float _moveSpeed = 150f;
    [SerializeField]
    float _maxSpeed = 10f;

    [Header("Jump")]
    [SerializeField] float _jumpVelocity      = 20f;   // 점프 초기 수직 속도 (임펄스)
    [SerializeField] float _minFallMultiplier = 10f;  // 완만한 낙하 (최대 높이 점프)
    [SerializeField] float _maxFallMultiplier = 30f;  // 가파른 낙하 (최소 높이 점프)

    bool _canAirJump        = false;
    bool _isAbilityActive   = false;
    bool _hasAirAbilityUsed = false;
    bool _isJumpRising      = false;  // 현재 상승 고정 중 여부

    public virtual AbillityType AbillityType => AbillityType.None;

    protected bool IsAbilityActive
    {
        get => _isAbilityActive;
        set => _isAbilityActive = value;
    }

    protected bool HasAirAbilityUsed
    {
        get => _hasAirAbilityUsed;
        set => _hasAirAbilityUsed = value;
    }

    protected Rigidbody     Rigi                => _inputData.rigi;
    protected Vector2       InputVector         => _inputData.inputVector;
    protected PlayerState   PlayerState         => _inputData.playerState;
    protected Vector2       InputMouseScreenPos => _inputData.mouseScreenPos;
    protected Camera        InputCamera         => _inputData.camera;
    protected float         JumpRatio           => _inputData.jumpRatio;

    public void SetInputData(InputData inputData) { _inputData = inputData; }

    public void SetCanAirJump(bool tri) { _canAirJump = tri; }

    public void SetCanAbility(bool tri) { _hasAirAbilityUsed = (tri == false); }

    public virtual void Movement()
    {
        if (PlayerState == PlayerState.Idle)
        {
            _hasAirAbilityUsed = false;
        }

        if(_inputData.inputVector.sqrMagnitude < 0.01f) return;

        Vector3 forceDirection = new Vector3(InputVector.normalized.x, 0f, InputVector.normalized.y);
        Vector3 force = forceDirection * _moveSpeed;

        Vector3 currentVelocityXZ = new Vector3(Rigi.linearVelocity.x, 0f, Rigi.linearVelocity.z);
        float currentSpeed = currentVelocityXZ.magnitude;

        // 최대속도 이상일 때 가속 방향 힘만 제거, 브레이크 방향은 허용
        if (currentSpeed >= _maxSpeed)
        {
            Vector3 velocityDir = currentVelocityXZ.normalized;
            float dot = Vector3.Dot(force, velocityDir);

            // 이동 방향과 같은 방향 성분(가속)만 제거
            if (dot > 0f)
            {
                force -= velocityDir * dot;
            }
        }

        Rigi.AddForce(force, ForceMode.Force);
    }

    // 키가 눌리는 순간 1회 호출 — 초기 임펄스만 부여
    public virtual void JumpStart()
    {
        if (_inputData.playerState == PlayerState.Air && _canAirJump == false)
            return;

        Rigi.linearVelocity = new Vector3(Rigi.linearVelocity.x, _jumpVelocity, Rigi.linearVelocity.z);
        _isJumpRising = true;
        _canAirJump   = false;
    }

    // 키 누름 유지 중 FixedUpdate마다 호출 — 중력 감속 허용, 상승 중 추락 방지만 수행
    public virtual void JumpHold()
    {
        if (_isJumpRising == false) return;

        // 중력으로 속도가 0 이하가 되면 상승 종료
        if (Rigi.linearVelocity.y <= 0f)
        {
            _isJumpRising = false;
        }
    }

    // 키를 떼거나 최대 프레임 도달 시 1회 호출
    public virtual void JumpRelease(float jumpRatio)
    {
        _isJumpRising = false;

        // 이미 낙하 중이면 추가 보정 불필요
        if (Rigi.linearVelocity.y <= 0f) return;

        // jumpRatio = 1 → velocity.y 유지 (최대 프레임까지 누름, 최대 높이)
        // jumpRatio = 0 → velocity.y = 0  (즉시 뗌, 최소 높이)
        float clampedY = Rigi.linearVelocity.y * jumpRatio;
        Rigi.linearVelocity = new Vector3(Rigi.linearVelocity.x, clampedY, Rigi.linearVelocity.z);
    }

    // 낙하 로직 — 점프와 독립, FixedUpdate마다 호출
    public virtual void FallUpdate()
    {
        if (_isJumpRising) return;
        if (PlayerState == PlayerState.Idle) return;
        if (Rigi.linearVelocity.y >= 0f) return;

        // jumpRatio == 0: 점프 없이 낙하 → 완만한 낙하 적용
        float ratio = (JumpRatio == 0f) ? 1f : JumpRatio;
        float fallMultiplier = Mathf.Lerp(_maxFallMultiplier, _minFallMultiplier, ratio);

        Rigi.AddForce(Vector3.down * fallMultiplier, ForceMode.Acceleration);
    }

    public abstract void Ability();

    public virtual void CancelAbility()
    {
    }

    public virtual void FreezeMovement()
    {
        Rigi.linearVelocity = Vector3.zero;
        Rigi.angularVelocity = Vector3.zero;
        Rigi.isKinematic = true;
    }

    public virtual void UnfreezeMovement()
    {
        Rigi.isKinematic = false;
    }
}

public enum MoveMod
{
    Normal,
    Wire,
}