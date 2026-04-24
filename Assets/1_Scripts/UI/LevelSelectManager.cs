using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    public static LevelSelectManager Instance { get; private set; }

    public List<LevelData> levels = new List<LevelData>();
    public int levelIndex = -1;

    [SerializeField] private RectTransform _contentViewUI;

    [SerializeField] private LevelSelectUI _levelSelectUI;
    [SerializeField] private RectTransform _SelectUI;
    private CanvasGroup _selectUICanvasGroup;
    private bool _isUIActive = false;

    [Header("Movement Settings")]
    [SerializeField] private float _smoothTime = 0.2f;
    private Vector2 _targetPosition;
    private Vector2 _currentVelocity = Vector2.zero;

    public bool IsUIActive => _isUIActive;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _selectUICanvasGroup = _SelectUI.GetComponentInChildren<CanvasGroup>();

        _targetPosition = _contentViewUI.anchoredPosition;

        Cursor.visible = true;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (_isUIActive == false) return;

        // Smooth Movement
        _contentViewUI.anchoredPosition = Vector2.SmoothDamp(
            _contentViewUI.anchoredPosition,
            _targetPosition,
            ref _currentVelocity,
            _smoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );

        if (Mouse.current.rightButton.wasPressedThisFrame == true)
        {
            CloseLevelUI();
            DisableSelectUI();
            return;
        }
    }

    public void OpenLevelUI(LevelSelectObject selectedObject)
    {
        if (_levelSelectUI == null) return;

        _isUIActive = true;

        _levelSelectUI.ToggleUI(true, selectedObject.Rect);
        _levelSelectUI.UpdateUI(selectedObject.LevelData);

        levelIndex = selectedObject.LevelIndex;

        _targetPosition = new Vector2(levelIndex * -500 + -100, 0);
    }

    public void CloseLevelUI()
    {
        if (_levelSelectUI == null) return;

        _isUIActive = false;

        _levelSelectUI.ToggleUI(false);
    }

    public void EnableSelectUI(RectTransform targetRect)
    {
        _SelectUI.anchoredPosition = targetRect.anchoredPosition + new Vector2(-30, 0);
        _selectUICanvasGroup.alpha = 1f;
        _selectUICanvasGroup.interactable = false;
    }

    public void DisableSelectUI()
    {
        _selectUICanvasGroup.alpha = 0f;
        _selectUICanvasGroup.interactable = false;
    }

    public void OnEnterButtonClicked(string sceneName)
    {
        if (levelIndex == -1) return;

        if (string.IsNullOrEmpty(sceneName) == false)
        {
            GameManager.Instance.LoadSceneWithFade(sceneName);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(levels != null && levelIndex >= 0 && levelIndex < levels.Count)
        {
            GameManager.Instance.levelData = levels[levelIndex];
        }
    }
}
