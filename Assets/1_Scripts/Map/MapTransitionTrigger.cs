using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MapTransitionTrigger : MonoBehaviour
{
    [SerializeField] MapData _targetMap;
    
    MapData _ownerMap;
    IGameplaySceneHandler _gameplayHandler;

    public void SetOwnerMap(MapData ownerMap)
    {
        _ownerMap = ownerMap;
    }

    private void Start()
    {
        // SceneContext 캐싱 — IGameplaySceneHandler를 통해 LevelSceneContext와
        // TutorialSceneContext 모두 호환
        _gameplayHandler = GameManager.Instance.CurrentContext as IGameplaySceneHandler;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7) // Player Layer
        {
            if (_gameplayHandler == null) return;

            // 현재 플레이어가 있는 맵이 아니면 발동 무시
            if (_gameplayHandler.CurrentMap != null && _gameplayHandler.CurrentMap != _ownerMap)
            {
                return;
            }

            _gameplayHandler.TransitionToMap(_targetMap, other.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider boxCol = GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0f, 1f, 1f, 0.5f); // Cyan 색상, 반투명
            Gizmos.DrawWireCube(boxCol.center, boxCol.size);
            
            Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
            Gizmos.DrawCube(boxCol.center, boxCol.size);
        }
        else
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
        }
    }
}
