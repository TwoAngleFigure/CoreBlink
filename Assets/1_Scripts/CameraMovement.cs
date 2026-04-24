using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Target Setting")]
    [SerializeField] Transform _target;

    [Header("Camera Options")]
    [SerializeField] Vector3 _offset = new Vector3(0f, 5f, -10f);
    [SerializeField] float _smoothTime = 0.2f;

    [Header("Boundary Detection")]
    [SerializeField] bool _useBoundaryDetection = false;
    [SerializeField] float _horizontalRayDistance = 1f;
    [SerializeField] float _verticalRayDistance = 1f;
    [SerializeField] Vector3 _raycastOffset = Vector3.zero;
    [SerializeField] LayerMask _boundaryLayer;

    Vector3 _currentVelocity = Vector3.zero;

    bool _blockedRight;
    bool _blockedLeft;
    bool _blockedUp;
    bool _blockedDown;

    public Vector3 CurrentVelocity => _currentVelocity;

    bool _ignoreOffset;

    private void Awake()
    {
        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.InputData.camera = GetComponent<Camera>();
            SetTarget(playerInput.transform);
        }
    }

    public void SetTarget(Transform newTarget, bool ignoreOffset = false)
    {
        _target = newTarget;
        _ignoreOffset = ignoreOffset;
    }

    public void ResetPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
        _currentVelocity = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 targetPosition;
        if (_ignoreOffset == true)
        {
            targetPosition = new Vector3(_target.position.x, _target.position.y, transform.position.z);
        }
        else
        {
            // X, Y축은 대상을 추적하고, Z축은 카메라의 원래 위치를 그대로 유지
            targetPosition = new Vector3(_target.position.x + _offset.x, _target.position.y + _offset.y, transform.position.z);
        }

        // 맵 전환 시 등, 대상 좌표가 명시적으로 지정(ignoreOffset == true)되었을 경우에는 
        // 기존 맵의 벽/경계선이 카메라의 부드러운 이동을 가로막지 않도록 경계 고정을 무시합니다.
        if (_useBoundaryDetection == true && _ignoreOffset == false)
        {
            DetectBoundaries();
            targetPosition = ClampByBoundary(targetPosition);
        }

        // 맵 전환(_ignoreOffset == true) 시에만 SmoothDamp 적용
        if (_ignoreOffset == true)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                targetPosition, 
                ref _currentVelocity, 
                _smoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );
        }
        else
        {
            // 일반적인 대상 추적 시에는 즉시 이동
            transform.position = targetPosition;
            _currentVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// 카메라의 상하좌우 4방향으로 Raycast를 쏴서 경계 충돌 여부를 갱신한다.
    /// </summary>
    private void DetectBoundaries()
    {
        Vector3 origin = transform.position + _raycastOffset;

        _blockedRight = Physics.Raycast(origin, Vector3.right, _horizontalRayDistance, _boundaryLayer);
        _blockedLeft  = Physics.Raycast(origin, Vector3.left,  _horizontalRayDistance, _boundaryLayer);
        _blockedUp    = Physics.Raycast(origin, Vector3.up,    _verticalRayDistance,   _boundaryLayer);
        _blockedDown  = Physics.Raycast(origin, Vector3.down,  _verticalRayDistance,   _boundaryLayer);
    }

    /// <summary>
    /// 충돌이 감지된 방향으로는 카메라가 현재 위치 이상 이동하지 못하도록 목표 위치를 제한한다.
    /// </summary>
    private Vector3 ClampByBoundary(Vector3 targetPosition)
    {
        Vector3 currentPosition = transform.position;

        // 오른쪽 차단: 목표가 현재보다 오른쪽이면 현재 X로 고정
        if (_blockedRight == true && targetPosition.x > currentPosition.x)
        {
            targetPosition.x = currentPosition.x;
        }

        // 왼쪽 차단: 목표가 현재보다 왼쪽이면 현재 X로 고정
        if (_blockedLeft == true && targetPosition.x < currentPosition.x)
        {
            targetPosition.x = currentPosition.x;
        }

        // 위쪽 차단: 목표가 현재보다 위면 현재 Y로 고정
        if (_blockedUp == true && targetPosition.y > currentPosition.y)
        {
            targetPosition.y = currentPosition.y;
        }

        // 아래쪽 차단: 목표가 현재보다 아래면 현재 Y로 고정
        if (_blockedDown == true && targetPosition.y < currentPosition.y)
        {
            targetPosition.y = currentPosition.y;
        }

        return targetPosition;
    }

    private void OnDrawGizmosSelected()
    {
        if (_useBoundaryDetection == false) return;

        Vector3 origin = transform.position + _raycastOffset;

        Gizmos.color = _blockedRight == true ? Color.red : Color.green;
        Gizmos.DrawRay(origin, Vector3.right * _horizontalRayDistance);

        Gizmos.color = _blockedLeft == true ? Color.red : Color.green;
        Gizmos.DrawRay(origin, Vector3.left * _horizontalRayDistance);

        Gizmos.color = _blockedUp == true ? Color.red : Color.green;
        Gizmos.DrawRay(origin, Vector3.up * _verticalRayDistance);

        Gizmos.color = _blockedDown == true ? Color.red : Color.green;
        Gizmos.DrawRay(origin, Vector3.down * _verticalRayDistance);
    }
}
