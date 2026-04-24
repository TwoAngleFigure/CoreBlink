using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "ScriptableObjects/LevelData")]
public class LevelData : ScriptableObject
{
    [SerializeField] private bool _isClear;
    [SerializeField] private float _clearTime = float.PositiveInfinity;
    [SerializeField] private int _totalDeathCount;

    [SerializeField] private string _levelName;
    [SerializeField] private string _sceneName;

    public string LevelName => _levelName;
    public int TotalDeathCount => _totalDeathCount;
    public string SceneName => _sceneName;
    public bool IsClear => _isClear;
    public float TimeToClear => _clearTime;

    public void LevelClear(float clearTime)
    {
        _isClear = true;
        if(clearTime < _clearTime)
            _clearTime = clearTime;
    }

    public void AddDeathCount()
    {
        _totalDeathCount++;
    }
}
