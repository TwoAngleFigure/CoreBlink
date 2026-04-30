using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 전역 게임 매니저. 씬 전환과 BaseSceneContext 라이프사이클 관리만 담당합니다.
/// 게임플레이 로직(사망, 리스폰, 맵 전환, 클리어)은 GameplaySceneContext에 위임합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ── 이벤트 ──
    public Action OnInitialize;
    public Action OnSceneChange;

    // ── 영구 참조 ──
    [SerializeField] private FadeUI _fadeUI;

    // ── 현재 SceneContext ──
    private BaseSceneContext _currentContext;
    public BaseSceneContext CurrentContext => _currentContext;

    // ── Getter ──
    public FadeUI FadeUI => _fadeUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        OnInitialize?.Invoke();
        LoadTitleScene();
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

        // 전역 이벤트 먼저 호출 (PlayerManager 상태 리셋 등)
        OnSceneChange?.Invoke();

        // 새 Context 탐색 및 초기화 (씬별 설정 적용)
        _currentContext = FindFirstObjectByType<BaseSceneContext>();
        if (_currentContext != null)
        {
            _currentContext.OnSceneEnter();
        }

        _fadeUI.FadeOut();
    }

    // ── 씬 전환 ──
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(LoadSceneWithFadeRoutine(sceneName));
    }

    public void LoadTitleScene()
    {
        SceneManager.LoadScene("_Title");
    }

    private IEnumerator LoadSceneWithFadeRoutine(string sceneName)
    {
        // 현재 Context 정리 (씬 전환 전)
        if (_currentContext != null)
        {
            _currentContext.OnSceneExit();
            _currentContext = null;
        }

        if (_fadeUI != null)
        {
            bool isFadeInComplete = false;
            _fadeUI.FadeIn(() => isFadeInComplete = true);
            yield return new WaitUntil(() => isFadeInComplete == true);
        }

        SceneManager.LoadScene(sceneName);
    }
}
