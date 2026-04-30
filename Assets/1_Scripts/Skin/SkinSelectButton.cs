using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개별 스킨 버튼 UI 컴포넌트.
/// 스킨 데이터를 표시하고 클릭 시 SkinManager에 선택을 전달합니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class SkinSelectButton : MonoBehaviour
{
    [SerializeField] private Image _iconImage;

    private PlayerLookData _lookData;
    private SkinManager _skinManager;

    /// <summary>
    /// SkinManager.GenerateButtons()에서 호출됩니다.
    /// 버튼에 스킨 데이터를 바인딩하고 클릭 이벤트를 연결합니다.
    /// </summary>
    public void Initialize(PlayerLookData lookData, SkinManager skinManager)
    {
        _lookData = lookData;
        _skinManager = skinManager;

        // 아이콘 표시 갱신
        if (_iconImage != null && lookData.icon != null)
        {
            _iconImage.sprite = lookData.icon;
        }

        // 버튼 클릭 이벤트 연결
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (_skinManager != null && _lookData != null)
        {
            _skinManager.SelectSkin(_lookData);
        }
    }

    private void OnDestroy()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
        }
    }
}
