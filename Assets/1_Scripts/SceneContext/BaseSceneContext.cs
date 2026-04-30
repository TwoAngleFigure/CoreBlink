using UnityEngine;

/// <summary>
/// 각 씬에 하나씩 배치되는 씬 컨텍스트의 추상 베이스.
/// GameManager가 씬 로드 후 자동으로 탐색하여 초기화를 위임합니다.
/// </summary>
public abstract class BaseSceneContext : MonoBehaviour
{
    [SerializeField] private SceneConfigData _config;

    public SceneConfigData Config => _config;

    /// <summary>
    /// GameManager.OnSceneLoaded에서 호출됩니다.
    /// 기본 구현: Config 기반 공통 초기화 (커서, MouseCursorTracker 등)
    /// </summary>
    public virtual void OnSceneEnter()
    {
        ApplyConfig();
    }

    /// <summary>
    /// 다른 씬으로 전환되기 직전에 GameManager가 호출합니다.
    /// 리소스 정리, 이벤트 해제 등에 사용합니다.
    /// </summary>
    public virtual void OnSceneExit()
    {
    }

    /// <summary>
    /// Config 데이터를 기반으로 공통 설정을 적용합니다.
    /// ShowCursor == true  → 시스템 커서 표시, MouseCursorTracker 비활성화
    /// ShowCursor == false → 시스템 커서 숨김, MouseCursorTracker 활성화
    /// </summary>
    protected void ApplyConfig()
    {
        if (_config == null) return;

        // 커서 & MouseCursorTracker 설정
        Cursor.visible = _config.ShowCursor;

        if (MouseCursorTracker.Instance != null)
        {
            MouseCursorTracker.Instance.gameObject.SetActive(_config.ShowCursor == false);
        }
    }
}
