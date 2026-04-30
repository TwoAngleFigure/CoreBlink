using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectPageUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup _currentSelectCanvasGroup;
    [SerializeField] private TMP_Text _currentLevelName;
    [SerializeField] private Image _currentLevelIcon;

    [SerializeField] private Button returnButton;

    public void Start()
    {
        returnButton.onClick.AddListener(LevelSelectManager.Instance.OnReturnToSelectClicked);
    }

    public void UpdateUI(bool tri, string levelName = null, Sprite icon = null)
    {
        if (tri)
        {
            _currentSelectCanvasGroup.alpha = 1f;
            _currentSelectCanvasGroup.interactable = true;
            _currentSelectCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            _currentSelectCanvasGroup.alpha = 0f;
            _currentSelectCanvasGroup.interactable = false;
            _currentSelectCanvasGroup.blocksRaycasts = false;
        }
        if(levelName != null)
        {
            _currentLevelName.text = levelName;
        }
        if (icon != null)
        {
            _currentLevelIcon.sprite = icon;
        }
    }
}
