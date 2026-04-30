using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 게임플레이 로직(맵 전환, 사망/리스폰, 스테이지 클리어, 시간 추적)을 포함하는
/// SceneContext의 중간 베이스 클래스입니다.
/// LevelSceneContext와 TutorialSceneContext가 이 클래스를 상속합니다.
/// </summary>
public abstract class GameplaySceneContext : BaseSceneContext, IGameplaySceneHandler
{
    [Header("Level")]
    [SerializeField] private LevelData _levelData;

    [Header("Save")]
    [SerializeField] private SaveData _saveData;

    [Header("Respawn")]
    [SerializeField] private Transform _defaultRespawnSpot;
    private Transform _currentRespawnSpot;

    // ── 맵 전환 ──
    [field: SerializeField]
    public MapData CurrentMap { get; private set; }
    private bool _isTransitioning;

    // ── 이벤트 ──
    public Action<PlayerManager> OnPlayerDeath { get; set; }
    public Action<LevelData, float> OnStageClear { get; set; }

    // ── 시간 ──
    private float _currentTime;

    // ── 캐싱 (하위 클래스에서 Player 접근용) ──
    protected PlayerManager CachedPlayer { get; private set; }

    // ── Getter ──
    public LevelData LevelData => _levelData;

    #region Lifecycle

    public override void OnSceneEnter()
    {
        base.OnSceneEnter();

        CachedPlayer = FindFirstObjectByType<PlayerManager>();
        _currentRespawnSpot = _defaultRespawnSpot;
        _currentTime = 0f;
        _isTransitioning = false;

        // 플레이어 리스폰 위치 설정
        if (CachedPlayer != null && _currentRespawnSpot != null)
        {
            CachedPlayer.transform.position = _currentRespawnSpot.position;
        }

        // 사망 이벤트 바인딩
        OnPlayerDeath += HandlePlayerDeath;
    }

    public override void OnSceneExit()
    {
        OnPlayerDeath -= HandlePlayerDeath;
    }

    protected virtual void Update()
    {
        _currentTime += Time.deltaTime;
    }

    #endregion

    #region Map Transition

    public void TransitionToMap(MapData targetMap, GameObject player)
    {
        if (_isTransitioning == true) return;
        _isTransitioning = true;

        CurrentMap = targetMap;
        targetMap.SavePlayerState(player);

        if (targetMap.RespawnSpot != null)
        {
            _currentRespawnSpot = targetMap.RespawnSpot;
        }

        StartCoroutine(SmoothCameraTransition(targetMap));
    }

    private IEnumerator SmoothCameraTransition(MapData targetMap)
    {
        Time.timeScale = 0f;

        MouseCursorTracker cursorTracker = MouseCursorTracker.Instance;
        if (cursorTracker != null) cursorTracker.EnableTrail(false);

        if (targetMap.CameraPosition != null)
        {
            CameraMovement camMove = Camera.main.GetComponent<CameraMovement>();
            if (camMove != null)
            {
                // 페이드 인 아웃 없이 타겟 오프셋 무시하고 _cameraPosition 으로 이동 지시
                camMove.SetTarget(targetMap.CameraPosition, true);

                // 자연스러운 카메라 랜딩을 위해 속도가 줄어들 때까지 대기
                yield return new WaitForSecondsRealtime(0.1f);
                while (camMove.CurrentVelocity.sqrMagnitude > 0.05f)
                {
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSecondsRealtime(1.0f);
            }
        }
        else
        {
            // 타겟 카메라 포지션이 없다면 기본 쿨다운
            yield return new WaitForSecondsRealtime(1.0f);
        }

        if (cursorTracker != null) cursorTracker.EnableTrail(true);

        Time.timeScale = 1f;
        _isTransitioning = false;
    }

    #endregion

    #region Death / Respawn

    private void HandlePlayerDeath(PlayerManager player)
    {
        StartCoroutine(DeathSequenceRoutine(player));
    }

    private IEnumerator DeathSequenceRoutine(PlayerManager player)
    {
        _levelData.AddDeathCount();

        FadeUI fadeUI = GameManager.Instance.FadeUI;

        bool isFadeInComplete = false;
        fadeUI.FadeIn(() => isFadeInComplete = true);
        yield return new WaitUntil(() => isFadeInComplete == true);

        PlayerRespawn(player);
        player.ResetPlayerState();

        yield return new WaitForSeconds(0.2f);

        bool isFadeOutComplete = false;
        fadeUI.FadeOut(() => isFadeOutComplete = true);
        yield return new WaitUntil(() => isFadeOutComplete == true);
    }

    public void PlayerRespawn(PlayerManager player)
    {
        if (_currentRespawnSpot == null)
        {
            _currentRespawnSpot = _defaultRespawnSpot;
        }

        player.transform.position = _currentRespawnSpot.position;

        if (CurrentMap != null)
        {
            CurrentMap.MapReset(player.gameObject);
        }
    }

    #endregion

    #region Stage Clear

    public void StageClear()
    {
        Time.timeScale = 0f;

        _levelData.LevelClear(_currentTime);

        OnStageClear?.Invoke(_levelData, _currentTime);

        // SaveData 클리어 플래그 업데이트
        if (_saveData != null)
        {
            _saveData.MarkLevelClear(_levelData);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_saveData);
#endif
        }
        else
        {
            Debug.LogWarning("[GameplaySceneContext] _saveData가 할당되지 않았습니다! Inspector에서 SaveData SO를 연결하세요.", this);
        }

        // 커서 표시 + MouseCursorTracker 비활성화
        Cursor.visible = true;
        if (MouseCursorTracker.Instance != null)
        {
            MouseCursorTracker.Instance.gameObject.SetActive(false);
        }

        // 플레이어 입력 잠금
        if (CachedPlayer != null)
        {
            CachedPlayer.Input.SetInputLock(true);
        }
    }

    #endregion
}
