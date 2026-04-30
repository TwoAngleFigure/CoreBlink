using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    [SerializeField] LayerMask _targetLayer;

    private void OnTriggerEnter(Collider other)
    {
        // 레이어 마스크를 이용한 비트 연산으로 판정
        if ((_targetLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            // Start() 캐싱 대신 트리거 시점에 조회 — sceneLoaded 타이밍 문제 방지
            var handler = GameManager.Instance.CurrentContext as IGameplaySceneHandler;
            if (handler != null)
            {
                handler.StageClear();
            }
        }
    }
}

