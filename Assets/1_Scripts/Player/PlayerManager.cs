using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private PlayerInput _input;
    private PlayerLook _look;
    private PlayerObjectDetection _detection;

    bool _isDead = false;
    bool _isInitialized = false;

    // ── Getter (SceneContext에서 접근용) ──
    public PlayerInput Input => _input;
    public PlayerLook Look => _look;
    public PlayerObjectDetection Detection => _detection;

    public void Awake()
    {
        // GameManager보다 Awake가 먼저 호출될 수 있으므로 null 체크
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnInitialize += Initialize;
        }
    }

    private void Start()
    {
        // Awake 시점에 GameManager가 없었다면 Start에서 직접 초기화
        if (_isInitialized == false && GameManager.Instance != null)
        {
            Initialize();
        }
    }

    public void Initialize()
    {
        if (_isInitialized == true) return;
        _isInitialized = true;

        DontDestroyOnLoad(gameObject);

        if (_input == null) _input = GetComponent<PlayerInput>();
        _input.Initialize();

        if (_look == null) _look = GetComponent<PlayerLook>();
        _look.Initialize();

        if (_detection == null) _detection = GetComponent<PlayerObjectDetection>();

        _detection.GroundAction += _input.ChangePlayerState;
        _detection.obstacleDetected += OnObstacleDetected;
        _detection.CollisionDetected += OnCollisionDetected;
        _detection.InteractionEnter += _input.SetInteractable;
        _detection.InteractionExit += _input.ClearInteractable;

        // 씬 전환 시 기본 상태 리셋 (명명 메서드로 해제 가능)
        GameManager.Instance.OnSceneChange += ResetPlayerState;
        GameManager.Instance.OnSceneChange += ResetDefaultMovementState;
    }

    private void FixedUpdate()
    {
        if (_isDead) return;

        _input.OnFixedTick();
        _detection.CheckGroundState();
        _input._moveState.Movement();

        if (_input.InputData.isJumping)
            _input._moveState.JumpHold();

        _input._moveState.FallUpdate();
        _look.UpdateRotation();

        // 어빌리티 사용 가능 여부를 커서 파티클에 반영
        if (MouseCursorTracker.Instance != null)
        {
            MouseCursorTracker.Instance.UpdateAbilityColorState(
                _input._moveState.IsAirAbilityUsed);
        }
    }

    public void PlayerChangeAbillity(BaseMovementState state)
    {
        _input.SetMovementState(state);

        state.OnAfterimageStart = _look.StartAfterimage;
        state.OnAfterimageStop = _look.StopAfterimage;

        // 어빌리티 색상을 MouseCursorTracker 파티클로 전달
        if (MouseCursorTracker.Instance != null)
        {
            MouseCursorTracker.Instance.SetAbilityColor(state.AbillityType);
        }
    }

    private void OnObstacleDetected(bool tri)
    {
        if (tri && _isDead == false)
        {
            _isDead = true;
            _input.SetInputLock(true);
            _look.PlayerDeadEffect(true, () =>
            {
                // 현재 씬이 게임플레이 씬이면 사망 이벤트 전달
                IGameplaySceneHandler handler =
                    GameManager.Instance.CurrentContext as IGameplaySceneHandler;
                if (handler != null)
                {
                    handler.OnPlayerDeath?.Invoke(this);
                }
            });
        }
    }

    private void OnCollisionDetected()
    {
        if (_input != null && _input._moveState != null)
        {
            _input._moveState.CancelAbility();
        }
    }

    /// <summary>
    /// 씬 전환 시 PlayerInput의 BaseMovementState를 기본(None)으로 리셋합니다.
    /// </summary>
    public void ResetDefaultMovementState()
    {
        BaseMovementState defaultState = Mapper.AbillityTypeMapper(AbillityType.None);
        PlayerChangeAbillity(defaultState);
    }

    public void ResetPlayerState()
    {
        if (_input != null && _input.InputData != null)
        {
            _input.InputData.camera = Camera.main;
        }

        _input.SetInputLock(false);
        _look.ResetLook();
        _isDead = false;
    }

    private void OnDestroy()
    {
        if (_detection != null)
        {
            _detection.obstacleDetected -= OnObstacleDetected;
            _detection.GroundAction -= _input.ChangePlayerState;
            _detection.CollisionDetected -= OnCollisionDetected;
            _detection.InteractionEnter -= _input.SetInteractable;
            _detection.InteractionExit -= _input.ClearInteractable;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSceneChange -= ResetPlayerState;
            GameManager.Instance.OnSceneChange -= ResetDefaultMovementState;
        }
    }
}