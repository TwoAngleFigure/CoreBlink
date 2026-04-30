using UnityEngine;

/// <summary>
/// Lobby(레벨 선택) 씬의 Context.
/// SceneConfigData 기반 설정(커서 표시, MouseCursorTracker 비활성화 등)만 적용합니다.
/// 레벨 선택 로직은 기존 LevelSelectManager가 그대로 담당합니다.
/// </summary>
public class LobbySceneContext : BaseSceneContext
{
    public override void OnSceneEnter()
    {
        base.OnSceneEnter();
    }
}
