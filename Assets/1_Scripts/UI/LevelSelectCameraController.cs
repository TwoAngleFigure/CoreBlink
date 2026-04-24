using UnityEngine;
using UnityEngine.InputSystem;

public class LevelSelectCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _smoothTime = 0.3f;

    private Vector3 _targetPosition;
    private Vector3 _targetRotation;

    private Vector3 _originPosition;
    private Vector3 _originRotation;

    private Vector3 _currentVelocity = Vector3.zero;
    private Vector3 _currentRotationVelocity = Vector3.zero;

    private bool _isActive = false;

    private void Awake()
    {
        // 시작 시점의 위치와 회전을 '원래 위치'로 기억합니다.
        _originPosition = transform.position;
        _originRotation = transform.eulerAngles;
    }

    public void SetTarget(Vector3 position, Vector3 rotation)
    {
        _targetPosition = position;
        _targetRotation = rotation;
        _isActive = true;
    }

    public void StopManualControl()
    {
        // 제어를 끄는 대신, 목표 지점을 원래 위치로 변경하여 부드럽게 돌아가게 합니다.
        _targetPosition = _originPosition;
        _targetRotation = _originRotation;
    }

    private void LateUpdate()
    {
        if (_isActive == false) return;

        // Smooth Position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            _targetPosition,
            ref _currentVelocity,
            _smoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );

        // Smooth Rotation
        Vector3 currentEuler = transform.eulerAngles;
        
        float x = Mathf.SmoothDampAngle(currentEuler.x, _targetRotation.x, ref _currentRotationVelocity.x, _smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        float y = Mathf.SmoothDampAngle(currentEuler.y, _targetRotation.y, ref _currentRotationVelocity.y, _smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        float z = Mathf.SmoothDampAngle(currentEuler.z, _targetRotation.z, ref _currentRotationVelocity.z, _smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

        transform.eulerAngles = new Vector3(x, y, z);
    }
}
