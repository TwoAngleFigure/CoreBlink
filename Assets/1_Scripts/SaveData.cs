using UnityEngine;

[CreateAssetMenu(menuName = "SO/Save Data", fileName = "New Save Data")]
public class SaveData : ScriptableObject
{
    public bool isTutorialClear = false;
    public bool isLevelOneClear = false;

    /// <summary>
    /// LevelData의 SceneName을 기반으로 해당 레벨의 클리어 플래그를 true로 설정합니다.
    /// 새 레벨 추가 시 case를 추가합니다.
    /// </summary>
    public void MarkLevelClear(LevelData levelData)
    {
        if (levelData == null) return;

        switch (levelData.SceneName)
        {
            case "Level_Tutorial":
                isTutorialClear = true;
                break;

            case "Level_1":
                isLevelOneClear = true;
                break;
        }
    }
}
