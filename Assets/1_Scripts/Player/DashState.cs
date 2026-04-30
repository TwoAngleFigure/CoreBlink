using UnityEngine;

public class DashState : BaseMovementState
{
    [Header("Dash Option")]
    [SerializeField]
    float _dashSpeed = 30f;
    [SerializeField]
    float _dashDistance = 5f;

    Vector3 _dashDirection;
    Vector3 _dashStartPosition;
    public override AbillityType AbillityType => AbillityType.Dash;

    public override void Ability()
    {
        if (IsAbilityActive == true) return;
        if (PlayerState == PlayerState.Air && HasAirAbilityUsed == true) return;

        Vector2 mouseScreenPos = InputMouseScreenPos;
        Camera cam = InputCamera;

        if (cam == null) return;

        // 마우스 스크린 좌표를 월드 좌표로 변환 (카메라 Z축 고정이므로 Z는 카메라-플레이어 간 거리로 설정)
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x, mouseScreenPos.y, cam.transform.position.z - Rigi.position.z)
        );

        // XY 평면에서 방향 계산 (Z 고정)
        Vector3 direction = mouseWorldPos - Rigi.position;
        direction.z = 0f;

        if (direction.sqrMagnitude < 0.01f) return;

        if (PlayerState == PlayerState.Air)
            HasAirAbilityUsed = true;

        _dashDirection = direction.normalized;
        _dashStartPosition = Rigi.position;
        IsAbilityActive = true;

        // 대시 시작 시 중력 및 기존 속도 제거
        Rigi.useGravity = false;
        Rigi.linearDamping = 0f;
        Rigi.linearVelocity = Vector3.zero;

        // 잔상 효과 시작
        OnAfterimageStart?.Invoke();
    }

    public override void Movement()
    {
        base.Movement();

        if (IsAbilityActive == false)
        {
            return;
        }

        // 대시 중에는 일정 속도로 이동
        Rigi.linearVelocity = _dashDirection * _dashSpeed;

        // 이동 거리 체크
        float distanceTraveled = Vector3.Distance(_dashStartPosition, Rigi.position);

        if (distanceTraveled >= _dashDistance)
        {
            EndDash();
        }
    }

    void EndDash()
    {
        IsAbilityActive = false;
        Rigi.useGravity = true;
        Rigi.linearDamping = 1f; // 원래 값으로 복원 (필요 시 조정)

        // 잔상 효과 종료
        OnAfterimageStop?.Invoke();
    }

    public override void FreezeMovement()
    {
        base.FreezeMovement();

        // 강제로 움직임이 제한될 때 켜져있는 대쉬 능력을 깔끔하게 종료 (리스폰/씬 이동 대응)
        if (IsAbilityActive)
        {
            EndDash();
        }
    }

    public override void CancelAbility()
    {
        base.CancelAbility();
        
        if (IsAbilityActive)
        {
            EndDash();
        }
    }
}


