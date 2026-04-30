using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [Header("Input")]
    PlayerAction _action;
    InputAction _movementAction;
    InputAction _jumpAction;
    InputAction _abilityAction;
    InputAction _mousePosAction;
    InputAction _interactionAction;

    public BaseMovementState _moveState;

    // ── 상호작용 ──
    private IInteractable _currentInteractable;

    [Header("Move Direction")]
    [SerializeField] InputData inputData;
    [SerializeField] InputDirection _currentDirection;
    [SerializeField] MoveMod _moveMod;

    [Header("Jump")]
    [SerializeField] int _maxJumpFrames = 18;  // FixedUpdate 기준 프레임 수

    int  _jumpFrameCount = 0;
    bool _jumpFrameCountedThisFixed = false;

    [Header("Getter")]
    public InputDirection CurrentDirection => _currentDirection;
    public MoveMod CurrentMod => _moveMod;
    public InputData InputData => inputData;

    #region Unity Lifecycle
    public void Initialize()
    {
        //input
        if (inputData == null) inputData = new();
        if (_action == null) _action = new();
        _movementAction = _action.PlayerActionMap.Movement;
        _jumpAction = _action.PlayerActionMap.Jump;
        _abilityAction = _action.PlayerActionMap.Ability;
        _mousePosAction = _action.PlayerActionMap.MousePos;
        _interactionAction = _action.PlayerActionMap.Interaction;

        //data
        inputData.rigi = GetComponent<Rigidbody>();
        inputData.camera = Camera.main;

        if (_moveState == null) SetMovementState(new MovementState_Normal());

        SetInputLock(false);
        OnEnable();
    }

    public void OnEnable()
    {
        if (_action == null) return;

        _movementAction.Enable();
        _jumpAction.Enable();
        _abilityAction.Enable();
        _mousePosAction.Enable();
        if (_interactionAction != null) _interactionAction.Enable();

        _jumpAction.started  += OnJumpStarted;
        _jumpAction.canceled += OnJumpCanceled;
        _abilityAction.performed += OnAbility;
        if (_interactionAction != null) _interactionAction.performed += OnInteraction;
    }

    public void OnDisable()
    {
        if (_action == null) return;

        _jumpAction.started  -= OnJumpStarted;
        _jumpAction.canceled -= OnJumpCanceled;
        _abilityAction.performed -= OnAbility;
        if (_interactionAction != null) _interactionAction.performed -= OnInteraction;

        _movementAction.Disable();
        _jumpAction.Disable();
        _abilityAction.Disable();
        _mousePosAction.Disable();
        if (_interactionAction != null) _interactionAction.Disable();
    }

    void Update()
    {
        if (_action == null) return;

        inputData.inputVector    = _movementAction.ReadValue<Vector2>();
        inputData.mouseScreenPos = _mousePosAction.ReadValue<Vector2>();
        UpdateDirection();
        UpdateJumpFrameCount();
    }
    #endregion

    // FixedUpdate마다 PlayerManager가 호출 → 카운팅 허용 플래그 리셋
    public void OnFixedTick()
    {
        _jumpFrameCountedThisFixed = false;
    }

    // FixedUpdate 틱당 1회만 카운팅 → FPS와 무관한 점프 높이 보장
    private void UpdateJumpFrameCount()
    {
        if (inputData.isJumping == false) return;
        if (_jumpFrameCountedThisFixed) return;  // 이번 Fixed 틱에서 이미 카운팅됨 → 무시

        _jumpFrameCountedThisFixed = true;
        _jumpFrameCount++;

        if (_jumpFrameCount >= _maxJumpFrames)
            ReleaseJump();
    }

    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        _jumpFrameCount = 0;
        _jumpFrameCountedThisFixed = false;
        inputData.isJumping = true;
        _moveState.JumpStart();
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        ReleaseJump();
    }

    private void ReleaseJump()
    {
        if (inputData.isJumping == false) return;

        inputData.isJumping = false;
        float jumpRatio = Mathf.Clamp01((float)_jumpFrameCount / _maxJumpFrames);
        inputData.jumpRatio = jumpRatio;
        _moveState.JumpRelease(jumpRatio);
    }

    private void OnAbility(InputAction.CallbackContext context)
    {
        _moveState.Ability();
    }

    // ── 상호작용 ──
    public void SetInteractable(IInteractable interactable)
    {
        _currentInteractable = interactable;
        _currentInteractable.OnEnterRange();
    }

    public void ClearInteractable(IInteractable interactable)
    {
        // 현재 대상과 동일한 경우에만 해제 (다른 영역에서 새 대상이 이미 설정됐을 수 있음)
        if (_currentInteractable == interactable)
        {
            _currentInteractable.OnExitRange();
            _currentInteractable = null;
        }
    }

    private void OnInteraction(InputAction.CallbackContext context)
    {
        if (_currentInteractable != null)
        {
            _currentInteractable.Interact();
        }
    }

    private void UpdateDirection()
    {
        const float deadzoneValue = 0.1f;

        if (inputData.inputVector.sqrMagnitude < 0.01f) return;

        float x = inputData.inputVector.x;
        float y = inputData.inputVector.y;

        if (x > deadzoneValue)
        {
            if (y > deadzoneValue) _currentDirection = InputDirection.RightUp;
            else if (y < -1 * deadzoneValue) _currentDirection = InputDirection.RightDown;
            else _currentDirection = InputDirection.Right;
        }
        else if (x < -1 * deadzoneValue)
        {
            if (y > deadzoneValue) _currentDirection = InputDirection.LeftUp;
            else if (y < -1 * deadzoneValue) _currentDirection = InputDirection.LeftDown;
            else _currentDirection = InputDirection.Left;
        }
        else
        {
            if (y > deadzoneValue) _currentDirection = InputDirection.Up;
            else if (y < -1 * deadzoneValue) _currentDirection = InputDirection.Down;
        }
    }

    public void SetMovementState(BaseMovementState state)
    {
        _moveState = state;
        state.SetInputData(inputData);
    }

    public void ChangePlayerState(bool isGround)
    {
        if (isGround)
        {
            inputData.playerState = PlayerState.Idle;
            inputData.jumpRatio   = 0f;
        }
        else
        {
            inputData.playerState = PlayerState.Air;
        }
    }

    public void SetInputLock(bool isLocked)
    {
        this.enabled = isLocked == false;
        if (isLocked)
        {
            _moveState.FreezeMovement();
        }
        else
        {
            _moveState.UnfreezeMovement();
        }
    }
}

public enum InputDirection
{
    Left, LeftUp, LeftDown,
    Right, RightUp, RightDown,
    Up, Down, None
}

[System.Serializable]
public class InputData
{
    public Vector2 inputVector;
    public Vector2 mouseScreenPos;
    public Rigidbody rigi;
    public Camera camera;
    public PlayerState playerState = PlayerState.Air;

    public float jumpRatio  = 0f;    // 마지막 점프의 높이 비율 (0~1)
    public bool  isJumping  = false; // 현재 점프 입력 유지 중 여부
}

public enum PlayerState
{
    Idle,
    Air,
}