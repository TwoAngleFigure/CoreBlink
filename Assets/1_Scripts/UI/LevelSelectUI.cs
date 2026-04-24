using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectUI : MonoBehaviour
{
    [SerializeField] private RectTransform _myRect;
    [SerializeField] private Vector3 _rectOffset;
    [SerializeField] private TextMeshProUGUI _levelNameText;
    [SerializeField] private TextMeshProUGUI _deathCountText;
    [SerializeField] private Button _enterButton;
    [SerializeField] private CanvasGroup _canvasGroup;


    private string _sceneName;

    public void ToggleUI(bool isOpen, RectTransform rect = null)
    {
        if (_canvasGroup == null) return;

        if (_myRect != null && rect != null)
        {

        }

        _canvasGroup.alpha = isOpen == true ? 1f : 0f;
        _canvasGroup.interactable = isOpen;
        _canvasGroup.blocksRaycasts = isOpen;
    }

    private void Awake()
    {
        if (_enterButton != null)
        {
            _enterButton.onClick.AddListener(() => LevelSelectManager.Instance.OnEnterButtonClicked(_sceneName));
        }
    }

    public void UpdateUI(LevelData levelData)
    {
        if (levelData == null) return;

        _sceneName = levelData.SceneName;

        if (_levelNameText != null)
        {
            _levelNameText.text = levelData.LevelName;
        }

        if (_deathCountText != null)
        {
            _deathCountText.text = levelData.TotalDeathCount.ToString();
        }
    }
}
