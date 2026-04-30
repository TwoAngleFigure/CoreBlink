using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelInfoUI : MonoBehaviour
{
    [SerializeField] private RectTransform _myRect;
    [SerializeField] private Vector3 _rectOffset;
    [SerializeField] private TextMeshProUGUI _levelNameText;
    [SerializeField] private TextMeshProUGUI _deathCountText;
    [SerializeField] private TextMeshProUGUI _clearTimeText;
    [SerializeField] private Button _enterButton;
    [SerializeField] private CanvasGroup _canvasGroup;

    private LevelData _levelData;
    public Sprite icon;

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
            _enterButton.onClick.AddListener(() => LevelSelectManager.Instance.OnSelectButtonClicked(_levelData, icon));
        }
    }

    public void UpdateUI(LevelData levelData)
    {
        if (levelData == null) return;

        _levelData = levelData;

        if (_levelNameText != null)
        {
            _levelNameText.text = levelData.LevelName;
        }

        if (_deathCountText != null)
        {
            _deathCountText.text = levelData.TotalDeathCount.ToString();
        }

        if (_clearTimeText != null)
        {
            if (levelData.IsClear)
            {
                float time = levelData.TimeToClear;
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                int milliseconds = Mathf.FloorToInt((time % 1f) * 100f);
                _clearTimeText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
            }
            else
            {
                _clearTimeText.text = "--:--.--";
            }
        }
    }
}
