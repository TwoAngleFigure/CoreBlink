using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킨 선택 시스템의 중심 클래스.
/// IInteractable을 구현하여 플레이어 상호작용 시 스킨 선택 UI를 열고,
/// 선택된 PlayerLookData를 PlayerLook에 적용합니다.
/// </summary>
public class SkinManager : MonoBehaviour, IInteractable
{
    [Header("Skin Collection")]
    [Tooltip("선택 가능한 스킨 목록. 에디터에서 PlayerLookData 에셋을 등록하세요.")]
    [SerializeField] private List<PlayerLookData> _skinList = new List<PlayerLookData>();

    [Header("UI")]
    [Tooltip("스킨 선택 UI 전체를 감싸는 CanvasGroup")]
    [SerializeField] private CanvasGroup _skinSelectUI;

    [Header("Button Container")]
    [Tooltip("SkinSelectButton 프리팹이 생성될 부모 Transform (Grid Layout 등)")]
    [SerializeField] private Transform _buttonContainer;

    [Tooltip("SkinSelectButton 프리팹")]
    [SerializeField] private SkinSelectButton _buttonPrefab;

    private bool _isUIOpen = false;

    private void Start()
    {
        SetUIVisibility(false);
        GenerateButtons();
    }

    /// <summary>
    /// _skinList를 기반으로 버튼을 동적 생성합니다.
    /// </summary>
    private void GenerateButtons()
    {
        if (_buttonPrefab == null || _buttonContainer == null) return;

        for (int i = 0; i < _skinList.Count; i++)
        {
            SkinSelectButton button = Instantiate(_buttonPrefab, _buttonContainer);
            button.Initialize(_skinList[i], this);
        }
    }

    #region IInteractable

    /// <summary>
    /// 플레이어가 상호작용 키를 눌렀을 때 호출됩니다.
    /// 스킨 선택 UI를 토글합니다.
    /// </summary>
    public void Interact()
    {
        if (_isUIOpen)
        {
            CloseUI();
        }
        else
        {
            OpenUI();
        }
    }

    /// <summary>
    /// 플레이어가 상호작용 영역에 진입했을 때 호출됩니다.
    /// </summary>
    public void OnEnterRange()
    {
        // 힌트 UI는 InteractionTrigger의 World Canvas에서 관리됩니다.
    }

    /// <summary>
    /// 플레이어가 상호작용 영역에서 이탈했을 때 호출됩니다.
    /// 스킨 선택 UI가 열려 있다면 닫습니다.
    /// </summary>
    public void OnExitRange()
    {
        if (_isUIOpen)
        {
            CloseUI();
        }
    }

    #endregion

    /// <summary>
    /// SkinSelectButton에서 호출. 선택된 스킨을 PlayerLook에 적용합니다.
    /// </summary>
    public void SelectSkin(PlayerLookData lookData)
    {
        if (lookData == null) return;

        // PlayerManager → PlayerLook 접근
        PlayerManager playerManager = FindFirstObjectByType<PlayerManager>();
        if (playerManager == null || playerManager.Look == null) return;

        playerManager.Look.SetNewLook(lookData);

        CloseUI();
    }

    private void OpenUI()
    {
        _isUIOpen = true;
        SetUIVisibility(true);
    }

    private void CloseUI()
    {
        _isUIOpen = false;
        SetUIVisibility(false);
    }

    private void SetUIVisibility(bool visible)
    {
        if (_skinSelectUI == null) return;

        _skinSelectUI.alpha = visible ? 1f : 0f;
        _skinSelectUI.interactable = visible;
        _skinSelectUI.blocksRaycasts = visible;
    }
}
