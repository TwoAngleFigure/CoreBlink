using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelResultUI : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Button continueButton;
    public TMP_Text _deathCount;
    public TMP_Text _clearTime;

    private IGameplaySceneHandler _gameplayHandler;

    public void Awake()
    {
        if (_deathCount == null) _deathCount = GetComponentInChildren<TMP_Text>();
        if (continueButton == null) continueButton = GetComponentInChildren<Button>();
        continueButton.onClick.AddListener(() => GameManager.Instance.LoadSceneWithFade("Lobby"));

        _gameplayHandler = FindFirstObjectByType<BaseSceneContext>() as IGameplaySceneHandler;
        if (_gameplayHandler != null)
        {
            _gameplayHandler.OnStageClear += UpdateUI;
        }

        CloseUI();
    }

    public void OpenUI()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void CloseUI()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void UpdateUI(LevelData data, float clearTime)
    {
        OpenUI();
        _deathCount.text = data.TotalDeathCount.ToString();

        if (_clearTime != null)
        {
            int minutes = Mathf.FloorToInt(clearTime / 60f);
            int seconds = Mathf.FloorToInt(clearTime % 60f);
            int milliseconds = Mathf.FloorToInt((clearTime % 1f) * 100f);
            _clearTime.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
        }
    }

    private void OnDestroy()
    {
        if (_gameplayHandler != null)
        {
            _gameplayHandler.OnStageClear -= UpdateUI;
        }
    }
}
