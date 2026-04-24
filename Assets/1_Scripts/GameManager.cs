using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Action OnInitialize;
    public Action OnSceneChange;
    public Action<PlayerManager> OnPlayerDeath;

    [Header("LevelData")]
    public LevelData levelData;
    float currentTime;

    [field: SerializeField]
    public MapData CurrentMap { get; private set; }

    [Header("Respawn")]
    public GameObject respawnSpot;
    public GameObject myRespawnSpot;

    bool _isTransitioning;

    [Header("Stage Clear")]
    public Action<LevelData> OnStageClear;
    [SerializeField] string _levelSelectSceneName = "Lobby";
    public FadeUI _fadeUI;

    public void Awake()
    {
        if(Instance == null) 
        { 
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        OnPlayerDeath += HandlePlayerDeath;

        Cursor.visible = false;
    }

    public void Start()
    {
        OnInitialize?.Invoke();
        LoadSceneWithFade("_Title");
    }

    public void Update()
    {
        currentTime += Time.deltaTime;
    }

    void OnEnable()
    {
        // SceneManager 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 메모리 누수 방지를 위한 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        OnSceneChange?.Invoke();
        _fadeUI.FadeOut();
    }

    public void TransitionToMap(MapData targetMap, GameObject player)
    {
        if (_isTransitioning == true) return;
        _isTransitioning = true;

        CurrentMap = targetMap;

        targetMap.SavePlayerState(player);

        if (targetMap.RespawnSpot != null)
        {
            // 부활 위치만 뒤에서 갱신해두고 플레이어 트랜스폼 강제 이동(텔레포트) 삭제
            respawnSpot = targetMap.RespawnSpot.gameObject;
        }

        // 맵 전환 쿨다운 및 카메라 스무스 이동 시작
        StartCoroutine(SmoothCameraTransition(targetMap));
    }

    private IEnumerator SmoothCameraTransition(MapData targetMap)
    {
        Time.timeScale = 0f;

        MouseCursorTracker cursorTracker = FindFirstObjectByType<MouseCursorTracker>();
        if (cursorTracker != null)
        {
            cursorTracker.EnableTrail(false);
        }

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

        if (cursorTracker != null)
        {
            cursorTracker.EnableTrail(true);
        }

        Time.timeScale = 1f;
        _isTransitioning = false;
    }

    private void HandlePlayerDeath(PlayerManager player)
    {
        StartCoroutine(DeathSequenceRoutine(player));
    }

    private IEnumerator DeathSequenceRoutine(PlayerManager player)
    {
        levelData.AddDeathCount();

        bool isFadeInComplete = false;
        _fadeUI.FadeIn(() => isFadeInComplete = true);
        yield return new WaitUntil(() => isFadeInComplete == true);

        PlayerRespawn(player);
        player.ResetPlayerState();

        yield return new WaitForSeconds(0.2f);

        bool isFadeOutComplete = false;
        _fadeUI.FadeOut(() => isFadeOutComplete = true);
        yield return new WaitUntil(() => isFadeOutComplete == true);
    }

    public void PlayerRespawn(PlayerManager player)
    {
        if (respawnSpot == null)
        {
            respawnSpot = myRespawnSpot;
        }

        player.transform.position = respawnSpot.transform.position;

        if (CurrentMap != null)
        {
            CurrentMap.MapReset(player.gameObject);
        }
    }

    public void StageClear()
    {
        Time.timeScale = 0f;

        OnStageClear?.Invoke(levelData);

        levelData.LevelClear(currentTime);

        Cursor.visible = true;
    }

    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(LoadSceneWithFadeRoutine(sceneName));
    }

    private IEnumerator LoadSceneWithFadeRoutine(string sceneName)
    {
        if (_fadeUI != null)
        {
            bool isFadeInComplete = false;
            _fadeUI.FadeIn(() => isFadeInComplete = true);
            yield return new WaitUntil(() => isFadeInComplete == true);
        }

        SceneManager.LoadScene(sceneName);
    }
}
