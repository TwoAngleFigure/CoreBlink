using UnityEngine;
using UnityEngine.UI;

public class LevelSelectObject : MonoBehaviour
{
    [SerializeField] private LevelData _levelData;
    [SerializeField] private int _levelIndex;

    [SerializeField] private Button _button;

    [SerializeField] RectTransform _rect;

    public Sprite _icon;

    public RectTransform Rect => _rect;
    public LevelData LevelData => _levelData;
    public int LevelIndex => _levelIndex;

    public void Awake()
    {
        if (_button == null) _button = GetComponentInChildren<Button>();
        if (_rect == null) _rect = GetComponent<RectTransform>();

        _button.onClick.AddListener(() => LevelSelectManager.Instance.OpenLevelInfoUI(this));
    }
}
