using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelResultUI : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Button continueButton;
    public TMP_Text _deathCount;

    public void Awake()
    {
        if (_deathCount == null) _deathCount = GetComponentInChildren<TMP_Text>();
        if (continueButton == null) continueButton = GetComponentInChildren<Button>();
        continueButton.onClick.AddListener(() => GameManager.Instance.LoadSceneWithFade("Lobby"));
        GameManager.Instance.OnStageClear += UpdateUI;
        CloseUI();
    }

    public void OpenUI()
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
    }

    public void CloseUI()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
    }

    public void UpdateUI(LevelData data)
    {
        OpenUI();
        _deathCount.text = data.TotalDeathCount.ToString();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStageClear -= UpdateUI;
        }
    }
}
