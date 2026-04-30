using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 튜토리얼 씬의 Context.
/// GameplaySceneContext의 게임플레이 기능 + 컷씬(비네트, 코어 파티클 각성) 로직을 추가합니다.
/// 기존 TutorialManager의 기능을 흡수합니다.
/// </summary>
public class TutorialSceneContext : GameplaySceneContext
{
    [Header("PostProcessing")]
    [SerializeField] private Volume _volume;
    private Vignette _vignette;

    [Header("Cutscene Settings")]
    [SerializeField] private float _vignetteDuration = 5f;
    [SerializeField] private float _coreAwakeDuration = 5f;
    [SerializeField] private float _coreAwakeStartRate = 0f;
    [SerializeField] private float _coreAwakeTargetRate = 7.29f;

    public override void OnSceneEnter()
    {
        base.OnSceneEnter(); // GameplaySceneContext 초기화 (리스폰, 이벤트 등)

        // PostProcessing 초기화
        if (_volume != null && _volume.profile.TryGet<Vignette>(out var vignette))
        {
            _vignette = vignette;
        }

        // 레벨 클리어 여부에 따른 분기
        if (LevelData != null && LevelData.IsClear)
        {
            SkipCutscene();
        }
        else
        {
            PlayCutscene();
        }
    }

    private void SkipCutscene()
    {
        if (_vignette != null)
        {
            _vignette.intensity.value = 0f;
            _vignette.active = false;
        }

        if (CachedPlayer != null)
        {
            CachedPlayer.Look?.ParticleCoreAwake(_coreAwakeTargetRate);
            CachedPlayer.Input?.SetInputLock(false);
            CachedPlayer.Detection?.SetDetectionEnabled(true);
        }
    }

    private void PlayCutscene()
    {
        if (CachedPlayer != null)
        {
            CachedPlayer.Input?.SetInputLock(true);
            CachedPlayer.Look?.ParticleCoreAwake(0);
            CachedPlayer.Detection?.SetDetectionEnabled(false);
        }

        if (_vignette != null)
        {
            _vignette.intensity.value = 0.5f;
            _vignette.active = true;
        }

        StartCoroutine(CutsceneRoutine());
    }

    private IEnumerator CutsceneRoutine()
    {
        // 비네트 페이드
        yield return VignetteIntensityRoutine(0.5f, 0f, _vignetteDuration);

        // 코어 파티클 각성
        if (CachedPlayer != null && CachedPlayer.Look != null)
        {
            bool isComplete = false;
            CachedPlayer.Look.ParticleCoreAwake(
                _coreAwakeDuration, _coreAwakeStartRate, _coreAwakeTargetRate,
                () => isComplete = true
            );
            yield return new WaitUntil(() => isComplete == true);
        }

        // 입력 잠금 해제 및 감지 콜라이더 활성화
        if (CachedPlayer != null)
        {
            CachedPlayer.Input?.SetInputLock(false);
            CachedPlayer.Detection?.SetDetectionEnabled(true);
        }
    }

    private IEnumerator VignetteIntensityRoutine(float start, float end, float duration)
    {
        if (_vignette == null) yield break;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            _vignette.intensity.value = Mathf.Lerp(start, end, elapsedTime / duration);
            yield return null;
        }
        _vignette.intensity.value = end;
    }
}
