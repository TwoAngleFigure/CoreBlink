using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// _Title 씬의 Context. 메뉴 버튼을 바인딩합니다.
/// 기존 TitleManager의 기능을 흡수합니다.
/// </summary>
public class TitleSceneContext : BaseSceneContext
{
    [Header("UI")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _endButton;

    [Header("Data")]
    [SerializeField] private SaveData _saveData;

    public override void OnSceneEnter()
    {
        base.OnSceneEnter();

        _startButton.onClick.AddListener(OnStartClicked);
        _endButton.onClick.AddListener(OnEndClicked);
    }

    public override void OnSceneExit()
    {
        _startButton.onClick.RemoveListener(OnStartClicked);
        _endButton.onClick.RemoveListener(OnEndClicked);
    }

    /// <summary>
    /// 튜토리얼 클리어 여부에 따라 Lobby 또는 Level_Tutorial로 분기합니다.
    /// </summary>
    private void OnStartClicked()
    {
        if (_saveData != null && _saveData.isTutorialClear)
        {
            GameManager.Instance.LoadSceneWithFade("Lobby");
        }
        else
        {
            GameManager.Instance.LoadSceneWithFade("Level_Tutorial");
        }
    }

    private void OnEndClicked()
    {
        Application.Quit();
    }
}
