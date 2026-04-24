using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public Button _startButton;
    public Button _endButton;

    public void Awake()
    {
        _startButton.onClick.AddListener(SceneChange);
        _endButton.onClick.AddListener(EndGame);
    }

    public void SceneChange()
    {
        GameManager.Instance.LoadSceneWithFade("Lobby");
    }

    public void EndGame()
    {

    }
}
