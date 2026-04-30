using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] MeshFilter filter;
    [SerializeField] MeshRenderer _meshRenderer;
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

    // ── Afterimage (잔상) ──
    [Header("Afterimage Settings")]
    [SerializeField] GameObject _echoPrefab;
    [SerializeField] Material _echoMaterial;
    [SerializeField] float _echoDuration = 0.4f;
    [SerializeField] float _echoInterval = 0.25f;
    [SerializeField] int _echoPoolSize = 20;
    [SerializeField] Color _echoColor = new Color(0f, 0.83f, 1f, 0.6f);
    [SerializeField] Color _echoEndColor = new Color(0.5f, 0f, 1f, 0f);

    private Coroutine _afterimageCoroutine;
    private List<GameObject> _echoPool;
    private int _echoPoolIndex;
    private static readonly int s_BaseColor = Shader.PropertyToID("_BaseColor");

    public void Initialize()
    {
        if (filter == null) filter = GetComponentInChildren<MeshFilter>();
        if (_meshRenderer == null)
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (coreParticle == null) coreParticle = GetComponentInChildren<ParticleSystem>();
        playerRigi = GetComponent<Rigidbody>();
        _lastPosition = transform.position;

        // 디버깅/구조 확인용: 만약 Mesh가 최상위 객체에 있다면 Rigidbody Constraints 때문에 회전이 무시됨.
        if (filter != null && filter.gameObject == this.gameObject)
        {
            Debug.LogWarning("[PlayerLook] 시각적 모델(MeshFilter)이 Rigidbody와 동일한 최상위 오브젝트에 있습니다. Rigidbody의 Freeze Rotation 제약에 의해 회전 로직이 덮어씌워질 수 있으니 자식 오브젝트로 분리하세요.");
        }

        InitializeEchoPool();
    }

    /// <summary>
    /// echoPrefab을 미리 생성하여 풀에 보관합니다.
    /// </summary>
    private void InitializeEchoPool()
    {
        _echoPool = new List<GameObject>(_echoPoolSize);
        _echoPoolIndex = 0;

        if (_echoPrefab == null) return;

        for (int i = 0; i < _echoPoolSize; i++)
        {
            GameObject echo = Instantiate(_echoPrefab);
            echo.SetActive(false);
            DontDestroyOnLoad(echo);
            _echoPool.Add(echo);
        }
    }

    #region Afterimage (잔상)

    /// <summary>
    /// 잔상 효과를 시작합니다. _echoInterval 간격으로 잔상을 반복 생성합니다.
    /// </summary>
    public void StartAfterimage()
    {
        if (_afterimageCoroutine != null) return;
        _afterimageCoroutine = StartCoroutine(AfterimageRoutine());
    }

    /// <summary>
    /// 잔상 효과를 종료합니다.
    /// </summary>
    public void StopAfterimage()
    {
        if (_afterimageCoroutine == null) return;
        StopCoroutine(_afterimageCoroutine);
        _afterimageCoroutine = null;
    }

    /// <summary>
    /// _echoInterval 간격으로 잔상을 반복 생성하는 코루틴입니다.
    /// </summary>
    private IEnumerator AfterimageRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(_echoInterval);

        while (true)
        {
            SpawnEcho();
            yield return wait;
        }
    }

    /// <summary>
    /// MeshFilter의 현재 메시를 스냅샷하여 잔상 1개를 생성합니다.
    /// </summary>
    private void SpawnEcho()
    {
        if (filter == null || _meshRenderer == null) return;
        if (_echoPool == null || _echoPool.Count == 0) return;

        // 현재 메시를 참조
        Mesh currentMesh = filter.sharedMesh;
        if (currentMesh == null) return;

        // 풀에서 오브젝트 가져오기 (라운드 로빈)
        GameObject echo = _echoPool[_echoPoolIndex];
        _echoPoolIndex = (_echoPoolIndex + 1) % _echoPool.Count;

        // 이전 페이드 코루틴이 남아있을 수 있으므로 비활성화 후 재활성화
        echo.SetActive(false);

        echo.transform.position = filter.transform.position;
        echo.transform.rotation = filter.transform.rotation;
        echo.transform.localScale = filter.transform.lossyScale;

        // 메시와 머티리얼 세팅
        var mf = echo.GetComponent<MeshFilter>();
        var mr = echo.GetComponent<MeshRenderer>();

        mf.sharedMesh = currentMesh;

        int subMeshCount = currentMesh.subMeshCount;
        // 모든 SubMesh에 동일한 머티리얼 적용
        var materials = Enumerable.Repeat(_echoMaterial, subMeshCount).ToArray();
        mr.sharedMaterials = materials;

        // 초기 색상 적용
        var mpb = new MaterialPropertyBlock();
        mpb.SetColor(s_BaseColor, _echoColor);
        for (int i = 0; i < subMeshCount; ++i)
            mr.SetPropertyBlock(mpb, i);

        echo.SetActive(true);

        // 색상 전환 + 페이드아웃 코루틴 시작
        StartCoroutine(FadeEchoRoutine(echo, mr, subMeshCount));
    }

    /// <summary>
    /// 잔상의 색상을 _echoColor → _echoEndColor로 전환하면서 알파를 서서히 줄입니다.
    /// </summary>
    private IEnumerator FadeEchoRoutine(GameObject echo, MeshRenderer mr, int subMeshCount)
    {
        var mpb = new MaterialPropertyBlock();
        float elapsedTime = 0f;

        while (elapsedTime < _echoDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / _echoDuration;

            // 색상 보간 (RGB)
            Color lerpedColor = Color.Lerp(_echoColor, _echoEndColor, t);
            // 알파 보간 (시작 알파 → 0)
            lerpedColor.a = Mathf.Lerp(_echoColor.a, 0f, t);

            mpb.SetColor(s_BaseColor, lerpedColor);
            for (int i = 0; i < subMeshCount; ++i)
                mr.SetPropertyBlock(mpb, i);

            yield return null;
        }

        echo.SetActive(false);
    }

    #endregion

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

    private IEnumerator CoreAwakeRoutine(float duration, float startRate, float targetRate, Action onComplete)
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
        filter.sharedMesh = data.mesh;
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

    private IEnumerator ShrinkAndPlayEffectRoutine(Action onComplete)
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