using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class LevelSelectManager : MonoBehaviour, IInteractable
{
    public static LevelSelectManager Instance { get; private set; }

    public List<LevelData> levels = new List<LevelData>();
    public int levelIndex = -1;

    [SerializeField] private CanvasGroup _levelSelectUI;
    [SerializeField] private RectTransform _contentViewUI;

    [SerializeField] private LevelInfoUI _levelInfoUI;
    [SerializeField] private RectTransform _SelectEffectUI;
    private CanvasGroup _selectUICanvasGroup;

    [SerializeField] private LevelSelectPageUI levelSelectPageUI;

    private bool _isUIActive = false;

    [Header("Movement Settings")]
    [SerializeField] private float _smoothTime = 0.2f;
    private Vector2 _targetPosition;
    private Vector2 _currentVelocity = Vector2.zero;

    [Header("Portal Settings")]
    [SerializeField] private LobbyPortalLook portalLook;

    public bool IsUIActive => _isUIActive;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _selectUICanvasGroup = _SelectEffectUI.GetComponentInChildren<CanvasGroup>();

        _targetPosition = _contentViewUI.anchoredPosition;
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
            CloseLevelInfoUI();
            DisableSelectEffectUI();
            return;
        }
    }

    public void OpenLevelInfoUI(LevelSelectObject selectedObject)
    {
        if (_levelInfoUI == null) return;

        _isUIActive = true;

        _levelInfoUI.ToggleUI(true, selectedObject.Rect);
        _levelInfoUI.UpdateUI(selectedObject.LevelData);
        _levelInfoUI.icon = selectedObject._icon;
        levelIndex = selectedObject.LevelIndex;

        _targetPosition = new Vector2(levelIndex * -500 + -100, 0);
    }

    public void CloseLevelInfoUI()
    {
        if (_levelInfoUI == null) return;

        _isUIActive = false;

        _levelInfoUI.ToggleUI(false);
    }

    public void EnableSelectEffectUI(RectTransform targetRect)
    {
        _SelectEffectUI.anchoredPosition = targetRect.anchoredPosition + new Vector2(-30, 0);
        _selectUICanvasGroup.alpha = 1f;
        _selectUICanvasGroup.interactable = false;
        _selectUICanvasGroup.blocksRaycasts = false;
    }

    public void DisableSelectEffectUI()
    {
        _selectUICanvasGroup.alpha = 0f;
        _selectUICanvasGroup.interactable = false;
        _selectUICanvasGroup.blocksRaycasts = false;
    }

    private string _selectedSceneName;

    /// <summary>
    /// 버튼에 연결하여 레벨 정보를 저장합니다.
    /// </summary>
    public void OnSelectButtonClicked(LevelData levelData, Sprite icon)
    {
        if (levelData == null)
        {
            portalLook.DeactivePortalLook();
            return; 
        }

        levelSelectPageUI.UpdateUI(true, levelData.LevelName, icon);

        _selectedSceneName = levelData.SceneName;
        portalLook.ActivePortalLook();
        DisableSelectEffectUI();
        CloseLevelInfoUI();

        _levelSelectUI.alpha = 0f;
        _levelSelectUI.interactable = false;
        _levelSelectUI.blocksRaycasts = false;
    }

    public void OnReturnToSelectClicked()
    {
        _levelSelectUI.alpha = 1f;
        _levelSelectUI.interactable = true;
        _levelSelectUI.blocksRaycasts = true;

        levelSelectPageUI.UpdateUI(false);
        portalLook.DeactivePortalLook();
    }

    /// <summary>
    /// 플레이어가 상호작용 영역 안에서 키 입력 시 호출됩니다.
    /// 저장된 씬으로 전환합니다.
    /// </summary>
    public void OnEnterKeyInput()
    {
        if (string.IsNullOrEmpty(_selectedSceneName) == true) return;

        GameManager.Instance.LoadSceneWithFade(_selectedSceneName);
    }

    #region IInteractable

    public void Interact()
    {
        OnEnterKeyInput();
    }

    public void OnEnterRange()
    {
        // 힌트 UI는 InteractionTrigger의 World Canvas에서 관리됩니다.
    }

    public void OnExitRange()
    {
        // 힌트 UI는 InteractionTrigger의 World Canvas에서 관리됩니다.
    }

    #endregion
}
