using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Image를 이용한 화면 페이드 인/아웃 컨트롤러.
/// 별도의 Canvas > Image 오브젝트에 부착하여 사용한다.
/// </summary>
public class FadeUI : MonoBehaviour
{
    [SerializeField] Image _fadeImage;

    [Header("Settings")]
    [SerializeField] float _fadeInDuration = 0.5f;
    [SerializeField] float _fadeOutDuration = 0.5f;
    [SerializeField] float _holdDuration = 0.5f;
    [SerializeField] Color _fadeColor = Color.black;

    Coroutine _currentFade;

    void Awake()
    {
        if (_fadeImage == null) _fadeImage = GetComponentInChildren<Image>();

        _fadeColor.a = 0f;
        _fadeImage.color = _fadeColor;
        _fadeImage.raycastTarget = false;

        _fadeImage.enabled = false;
    }

    /// <summary>
    /// 화면을 어둡게 만든다 (투명 → 불투명).
    /// </summary>
    public void FadeIn(Action onComplete = null)
    {
        StartFade(0f, 1f, _fadeInDuration, onComplete);
    }

    /// <summary>
    /// 화면을 밝게 만든다 (불투명 → 투명).
    /// </summary>
    public void FadeOut(Action onComplete = null)
    {
        StartFade(1f, 0f, _fadeOutDuration, onComplete);
    }

    /// <summary>
    /// FadeIn 후 일정 시간 대기, FadeOut까지 한 번에 수행한다.
    /// </summary>
    public void FadeInOut(Action onFadeInComplete = null, Action onAllComplete = null)
    {
        if (_currentFade != null) StopCoroutine(_currentFade);

        _currentFade = StartCoroutine(FadeInOutRoutine(onFadeInComplete, onAllComplete));
    }

    void StartFade(float startAlpha, float endAlpha, float duration, Action onComplete)
    {
        if (_currentFade != null) StopCoroutine(_currentFade);

        _currentFade = StartCoroutine(FadeRoutine(startAlpha, endAlpha, duration, onComplete));
    }

    IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration, Action onComplete)
    {
        _fadeImage.enabled = true;
        _fadeImage.raycastTarget = true;

        float elapsedTime = 0f;
        Color color = _fadeColor;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            _fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        _fadeImage.color = color;

        if (Mathf.Approximately(endAlpha, 0f))
        {
            _fadeImage.raycastTarget = false;
            _fadeImage.enabled = false;
        }

        _currentFade = null;
        onComplete?.Invoke();
    }

    IEnumerator FadeInOutRoutine(Action onFadeInComplete, Action onAllComplete)
    {
        // FadeIn (투명 → 불투명)
        yield return FadeRoutine(0f, 1f, _fadeInDuration, null);
        onFadeInComplete?.Invoke();

        // Hold (화면 가려진 상태 유지)
        if (_holdDuration > 0f)
        {
            yield return new WaitForSeconds(_holdDuration);
        }

        // FadeOut (불투명 → 투명)
        yield return FadeRoutine(1f, 0f, _fadeOutDuration, null);

        _currentFade = null;
        onAllComplete?.Invoke();
    }
}
