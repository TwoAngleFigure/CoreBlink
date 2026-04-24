using System;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] MeshFilter filter;
    [SerializeField] ParticleSystem coreParticle;

    [SerializeField] Rigidbody playerRigi;

    [SerializeField] int shapeAngle;

    [Header("Rotation")]
    [SerializeField] float _rotationSpeedMultiplier = 180f;
    [SerializeField] float _snapSpeed = 360f;
    [SerializeField] float _snapThreshold = 0.05f;

    float _displayAngle;
    Vector3 _lastPosition;

    [SerializeField] GameObject deadEffect;

    [Header("Dead Effect Settings")]
    [SerializeField] float _shrinkDuration = 0.5f;
    [SerializeField] float _effectTriggerScale = 0.2f;
    [SerializeField] float _effectWaitDuration = 1f;

    public void Initialize()
    {
        if (filter == null) filter = GetComponentInChildren<MeshFilter>();
        if (coreParticle == null) coreParticle = GetComponentInChildren<ParticleSystem>();
        playerRigi = GetComponent<Rigidbody>();
        _lastPosition = transform.position;

        // 디버깅/구조 확인용: 만약 Mesh가 최상위 객체에 있다면 Rigidbody Constraints 때문에 회전이 무시됨.
        if (filter != null && filter.gameObject == this.gameObject)
        {
            Debug.LogWarning("[PlayerLook] 시각적 모델(MeshFilter)이 Rigidbody와 동일한 최상위 오브젝트에 있습니다. Rigidbody의 Freeze Rotation 제약에 의해 회전 로직이 덮어씌워질 수 있으니 자식 오브젝트로 분리하세요.");
        }
    }

    public void ParticleCoreAwake(float time, float startRate, float targetRate, Action onComplete = null)
    {
        if (coreParticle == null) return;
        StartCoroutine(CoreAwakeRoutine(time, startRate, targetRate, onComplete));
    }

    public void ParticleCoreAwake(float targetRate)
    {
        if (coreParticle == null) return;
        coreParticle.Clear();
        var emission = coreParticle.emission;
        emission.rateOverTime = targetRate;
    }

    private System.Collections.IEnumerator CoreAwakeRoutine(float duration, float startRate, float targetRate, Action onComplete)
    {
        var emission = coreParticle.emission;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentRate = Mathf.Lerp(startRate, targetRate, elapsedTime / duration);
            emission.rateOverTime = currentRate;
            yield return null;
        }

        emission.rateOverTime = targetRate;
        onComplete?.Invoke();
    }

    public void SetNewLook(PlayerLookData data)
    {
        filter.mesh = data.mesh;
        shapeAngle = data.shapeAngle;
    }

    public Color CoreColor => coreParticle.main.startColor.color;

    public void SetCoreColor(Color newColor)
    { 
        var maint = coreParticle.main;
        maint.startColor = new Color(newColor.r, newColor.g, newColor.b);
    }

    public void SetCoreColor(string colorCode)
    {
        var maint = coreParticle.main;

        Color newColor;
        if (ColorUtility.TryParseHtmlString(colorCode, out newColor))
        {
            maint.startColor = newColor;
        }
    }

    public void UpdateRotation()
    {
        if (playerRigi == null || filter == null) return;

        float velX = (transform.position.x - _lastPosition.x) / Time.fixedDeltaTime;
        _lastPosition = transform.position;

        if (Mathf.Abs(velX) >= _snapThreshold)
        {
            // 이동 중: 속도에 비례하여 누적 회전 (오른쪽 이동 시 시계 방향)
            _displayAngle += -velX * _rotationSpeedMultiplier * Time.fixedDeltaTime;
        }
        else
        {
            // 정지 중: 가장 가까운 면으로 스냅
            if (shapeAngle > 0)
            {
                float snappedAngle = Mathf.Round(_displayAngle / shapeAngle) * shapeAngle;
                _displayAngle = Mathf.MoveTowardsAngle(
                    _displayAngle, snappedAngle, _snapSpeed * Time.fixedDeltaTime
                );
            }
        }

        filter.transform.localRotation = Quaternion.Euler(0f, 0f, _displayAngle);
    }

    public void PlayerDeadEffect(bool tri, Action onComplete = null)
    {
        if (tri)
        {
            StartCoroutine(ShrinkAndPlayEffectRoutine(onComplete));
        }
    }

    private System.Collections.IEnumerator ShrinkAndPlayEffectRoutine(Action onComplete)
    {
        // filter가 null인 경우를 대비한 안전망 (Initialize 전 호출 등)
        if (filter == null) yield break;

        Vector3 initialScale = filter.transform.localScale;
        float elapsedTime = 0f;
        bool hasPlayedEffect = false;

        while (elapsedTime < _shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            filter.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, elapsedTime / _shrinkDuration);

            if (hasPlayedEffect == false && filter.transform.localScale.x <= _effectTriggerScale)
            {
                if (deadEffect != null)
                {
                    deadEffect.SetActive(true);
                }
                hasPlayedEffect = true;
            }

            yield return null;
        }

        filter.transform.localScale = Vector3.zero;
        filter.gameObject.SetActive(false);
        if (coreParticle != null) coreParticle.gameObject.SetActive(false);

        if (hasPlayedEffect == false && deadEffect != null)
        {
            deadEffect.SetActive(true);
        }

        yield return new WaitForSeconds(_effectWaitDuration);

        onComplete?.Invoke();
    }

    public void ResetLook()
    {
        if (filter != null)
        {
            filter.gameObject.SetActive(true);
            filter.transform.localScale = Vector3.one;
            filter.transform.localRotation = Quaternion.identity;
        }
        if (coreParticle != null)
        {
            coreParticle.gameObject.SetActive(true);
        }
        if (deadEffect != null)
        {
            deadEffect.SetActive(false);
        }
    }
}