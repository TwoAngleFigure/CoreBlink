using System;
using System.Collections;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TutorialManager : MonoBehaviour
{
    public LevelData levelData;

    public PlayerManager playerManager;
    public PlayerLook playerLook;
    public PlayerInput playerInput;
    public Volume volume;
    Vignette _vignette;

    public float vignetteDuraction = 5f;

    public float coreAwakeDuration = 5f;
    public float coreAwakeStartRate = 0f;
    public float coreAwakeTargetRate = 7.29f;

    public void Start()
    {
        playerManager = FindFirstObjectByType<PlayerManager>();
        playerLook = FindFirstObjectByType<PlayerLook>();
        playerInput = FindFirstObjectByType<PlayerInput>();

        if (volume.profile.TryGet<Vignette>(out var vignette))
            _vignette = vignette;

        if (levelData.IsClear)
        {
            _vignette.intensity.value = 0f;
            _vignette.active = false;

            playerLook.ParticleCoreAwake(coreAwakeTargetRate);

            playerInput.SetInputLock(false);
            return;
        }
        playerInput.SetInputLock(true);
        playerLook.ParticleCoreAwake(0);
        _vignette.intensity.value = 0.5f;
        _vignette.active = true;

        TitleTutorialStageStartCutScene();
    }

    public void TitleTutorialStageStartCutScene()
    {
        ChangeVignetteIntensity(0.5f, 0f, vignetteDuraction, () =>
        {
            playerLook.ParticleCoreAwake(coreAwakeDuration, coreAwakeStartRate, coreAwakeTargetRate, () =>
            {
                playerInput.SetInputLock(false);
            });
        });
    }

    private void ChangeVignetteIntensity(float start, float end, float duration, Action onComplete)
    {
        if (_vignette == null)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(VignetteIntensityRoutine(_vignette, start, end, duration, onComplete));
    }

    private IEnumerator VignetteIntensityRoutine(Vignette vignette, float start, float end, float duration, Action onComplete)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            // Lambda to calculate lerp
            vignette.intensity.value = new Func<float, float, float, float>((s, e, t) => Mathf.Lerp(s, e, t))(start, end, elapsedTime / duration);
            yield return null;
        }

        vignette.intensity.value = end;
        onComplete?.Invoke();
    }
}
