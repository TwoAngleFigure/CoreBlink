using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MouseCursorTracker : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _zDepth = 0f;

    [Header("Detection")]
    [SerializeField] private float _rayDistance = 100f;
    [SerializeField] private LayerMask _detectLayer;
    [Header("Effects")]
    [SerializeField] private TrailRenderer _trailRenderer;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _detectColor = Color.cyan;

    private bool _isDetecting = false;

    public void EnableTrail(bool isEnable)
    {
        if (_trailRenderer != null)
        {
            _trailRenderer.emitting = isEnable;
            if (isEnable == false)
            {
                _trailRenderer.Clear();
            }
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
        FindMainCamera();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindMainCamera();
    }

    private void FindMainCamera()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        FollowMouseCursor();
        CheckObjectDetection();
    }

    private void FollowMouseCursor()
    {
        bool hasCamera = _mainCamera != null;
        if (hasCamera == false) return;

        bool hasMouse = Mouse.current != null;
        if (hasMouse == false) return;

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Ray cameraRay = _mainCamera.ScreenPointToRay(mouseScreenPosition);
        
        Plane targetPlane = new Plane(Vector3.back, new Vector3(0f, 0f, _zDepth));

        bool isHit = targetPlane.Raycast(cameraRay, out float distance);
        if (isHit == true)
        {
            transform.position = cameraRay.GetPoint(distance);
        }
    }

    private void CheckObjectDetection()
    {
        Ray ray = new Ray(transform.position, Vector3.forward);
        bool isHit = Physics.Raycast(ray, out RaycastHit hitInfo, _rayDistance, _detectLayer);

        _isDetecting = isHit;

        // 마우스 클릭 시 로그가 나오지 않는 문제 디버깅
        if (Mouse.current.leftButton.wasPressedThisFrame == true)
        {
            if (isHit == true)
            {
                // 콜라이더가 있는 자식 오브젝트가 맞았을 경우를 대비해 부모까지 확인
                LevelSelectObject selection = hitInfo.collider.GetComponentInParent<LevelSelectObject>();

                if (selection != null)
                {
                    Debug.Log($"LevelSelectObject detected on: {selection.name}");
                    if (LevelSelectManager.Instance != null)
                    {
                        LevelSelectManager.Instance.OpenLevelUI(selection);
                    }
                }
                else
                {
                    Debug.Log($"Hit object: {hitInfo.collider.name}, but no LevelSelectObject found.");
                }
            }
            else
            {
                Debug.Log("Left click detected, but nothing was hit by raycast.");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _isDetecting == true ? _detectColor : _normalColor;
        Gizmos.DrawRay(transform.position, Vector3.forward * _rayDistance);
    }
}
