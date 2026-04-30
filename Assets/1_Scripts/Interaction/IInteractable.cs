/// <summary>
/// 플레이어가 상호작용할 수 있는 오브젝트의 인터페이스.
/// InteractionTrigger의 _target에 연결됩니다.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// 플레이어가 상호작용 키를 눌렀을 때 호출됩니다.
    /// </summary>
    void Interact();

    /// <summary>
    /// 플레이어가 상호작용 영역에 진입했을 때 호출됩니다. (UI 힌트 표시 등)
    /// </summary>
    void OnEnterRange();

    /// <summary>
    /// 플레이어가 상호작용 영역에서 이탈했을 때 호출됩니다. (UI 힌트 숨김 등)
    /// </summary>
    void OnExitRange();
}
