using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    PlayerInput input;
    PlayerLook look;
    PlayerObjectDetection detection;

    bool _isDead = false;

    public void Awake()
    {
        GameManager.Instance.OnInitialize += Initialize;
    }

    public void Initialize()
    {
        DontDestroyOnLoad(gameObject);

        if (input == null) input = GetComponent<PlayerInput>();
        input.Initialize();

        if (look == null) look = GetComponent<PlayerLook>();
        look.Initialize();

        if (detection == null) detection = GetComponent<PlayerObjectDetection>();

        detection.GroundAction += input.ChangePlayerState;
        detection.obstacleDetected += OnObstacleDetected;
        detection.CollisionDetected += OnCollisionDetected;

        GameManager.Instance.OnStageClear += (LevelData data) => input.SetInputLock(true);

        GameManager.Instance.OnSceneChange += () => GameManager.Instance.PlayerRespawn(this);
        GameManager.Instance.OnSceneChange += ResetPlayerState;
        BaseMovementState state = Mapper.AbillityTypeMapper(AbillityType.None);
        GameManager.Instance.OnSceneChange += () => PlayerChangeAbillity(state);
    }

    private void FixedUpdate()
    {
        if (_isDead) return;

        input.OnFixedTick();
        detection.CheckGroundState();
        input._moveState.Movement();

        if (input.InputData.isJumping)
            input._moveState.JumpHold();

        input._moveState.FallUpdate();
        look.UpdateRotation();
    }

    public void PlayerChangeAbillity(BaseMovementState state)
    {
        input.SetMovementState(state);

        if (state.AbillityType == AbillityType.None)
            look.SetCoreColor("#FFFFFF");
        else if (state.AbillityType == AbillityType.Dash)
            look.SetCoreColor("#00D3FF");
    }

    private void OnObstacleDetected(bool tri)
    {
        if (tri && _isDead == false)
        {
            _isDead = true;
            input.SetInputLock(true);
            look.PlayerDeadEffect(true, () =>
            {
                GameManager.Instance.OnPlayerDeath?.Invoke(this);
            });
        }
    }

    private void OnCollisionDetected()
    {
        if (input != null && input._moveState != null)
        {
            input._moveState.CancelAbility();
        }
    }

    public void ResetPlayerState()
    {
        if (input != null && input.InputData != null)
        {
            input.InputData.camera = Camera.main;
        }

        input.SetInputLock(false);
        look.ResetLook();
        _isDead = false;
    }

    private void OnDestroy()
    {
        detection.obstacleDetected -= OnObstacleDetected;
        detection.GroundAction -= input.ChangePlayerState;
        detection.CollisionDetected -= OnCollisionDetected;
    }
}