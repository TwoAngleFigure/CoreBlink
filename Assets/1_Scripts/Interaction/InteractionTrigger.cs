using System.Collections;
using UnityEngine;

/// <summary>
/// 상호작용 영역을 정의하는 트리거 컴포넌트.
/// Collider(isTrigger)와 함께 배치하며, IInteractable 구현체를 참조합니다.
/// PlayerObjectDetection이 이 컴포넌트를 감지합니다.
/// 자식으로 World Space Canvas 힌트 UI를 배치하면 영역 진입 시 자동 표시됩니다.
/// </summary>
[RequireComponent(typeof(Collider))]
public class InteractionTrigger : MonoBehaviour
{
    [Tooltip("IInteractable을 구현한 MonoBehaviour를 드래그하세요.")]
    [SerializeField] private MonoBehaviour _target;

    [Header("Hint UI")]
    [Tooltip("상호작용 힌트 UI (World Space Canvas). 자식 오브젝트로 배치하세요.")]
    [SerializeField] private CanvasGroup _hintUI;

    [Tooltip("힌트 UI 표시 위치 오프셋 (트리거 기준)")]
    [SerializeField] private Vector3 _hintOffset = new Vector3(0f, 2f, 0f);

    private IInteractable _interactable;
    private Coroutine _fadeCoroutine;

    [Tooltip("힌트 페이드 인/아웃 소요 시간. 0이면 즉시 표시됩니다.")]
    [SerializeField] private float _fadeDuration = 0f;

    public IInteractable Interactable => _interactable;

    private void Awake()
    {
        // SerializeField는 인터페이스를 직접 지원하지 않으므로
        // MonoBehaviour로 받아서 캐스팅
        if (_target != null)
        {
            _interactable = _target as IInteractable;

            if (_interactable == null)
            {
                Debug.LogWarning($"[InteractionTrigger] {_target.name}은 IInteractable을 구현하지 않습니다.", this);
            }
        }

        // 힌트 UI 초기 숨김
        if (_hintUI != null)
        {
            _hintUI.transform.localPosition = _hintOffset;
            _hintUI.alpha = 0f;
            _hintUI.interactable = false;
            _hintUI.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// 플레이어가 영역에 진입했을 때 힌트 UI를 표시합니다.
    /// </summary>
    public void ShowHint()
    {
        if (_hintUI != null)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            if (gameObject.activeInHierarchy && _fadeDuration > 0f)
            {
                _fadeCoroutine = StartCoroutine(FadeAlpha(1f));
            }
            else
            {
                _hintUI.alpha = 1f;
                _hintUI.interactable = true;
                _hintUI.blocksRaycasts = true;
            }
        }
    }

    /// <summary>
    /// 플레이어가 영역에서 이탈했을 때 힌트 UI를 숨깁니다.
    /// </summary>
    public void HideHint()
    {
        if (_hintUI != null)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            if (gameObject.activeInHierarchy && _fadeDuration > 0f)
            {
                _fadeCoroutine = StartCoroutine(FadeAlpha(0f));
            }
            else
            {
                _hintUI.alpha = 0f;
                _hintUI.interactable = false;
                _hintUI.blocksRaycasts = false;
            }
        }
    }

    private IEnumerator FadeAlpha(float targetAlpha)
    {
        float startAlpha = _hintUI.alpha;
        float time = 0f;

        while (time < _fadeDuration)
        {
            time += Time.deltaTime;
            _hintUI.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / _fadeDuration);
            yield return null;
        }

        _hintUI.alpha = targetAlpha;
        _hintUI.interactable = targetAlpha > 0.5f;
        _hintUI.blocksRaycasts = targetAlpha > 0.5f;
    }
}
