using UnityEngine;

/// <summary>
/// 씬별 선언적 설정을 데이터로 관리하는 ScriptableObject.
/// 에디터에서 씬마다 SO 에셋을 생성하여 SceneContext에 연결합니다.
/// </summary>
[CreateAssetMenu(menuName = "SO/SceneConfigData", fileName = "New SceneConfig")]
public class SceneConfigData : ScriptableObject
{
    [Header("커서")]
    [Tooltip("true: 시스템 커서 표시 + MouseCursorTracker 비활성화\n" +
             "false: 시스템 커서 숨김 + MouseCursorTracker 활성화")]
    [SerializeField] private bool _showCursor = false;

    public bool ShowCursor => _showCursor;
}
