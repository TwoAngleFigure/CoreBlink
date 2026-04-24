using UnityEngine;
using System.Collections;

public class ItemLook : MonoBehaviour
{
    [Header("Physics & Motion Option")]
    [SerializeField] private Rigidbody rigi;
    [SerializeField] private Vector3 torqueVector;
    [SerializeField] private float torquePower;

    [Header("Object State Option")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody deactiveCircle;

    [Header("Particle Option")]
    [SerializeField] private ParticleSystem[] _particles;

    [Header("Fade Option")]
    [SerializeField] private Renderer[] _targetRenderers;
    [SerializeField] private float _fadeDuration = 1f;

    private int _animHash;
    private Coroutine _fadeCoroutine;

    public void Initialize(ItemData data)
    {
        if (animator != null) _animHash = Animator.StringToHash("Tri_GetItem");

        SetLook(data.color);       
        ApplyTorqueEffect(true);
        SetDeactiveCircleState(true);
        SetParticleState(true);
    }

    public void SetLook(Color newColor)
    {
        if (_particles == null || _particles.Length == 0) return;

        for (int i = 0; i < _particles.Length; i++)
        {
            if (_particles[i] == null) continue;

            var maint = _particles[i].main;
            maint.startColor = new Color(newColor.r, newColor.g, newColor.b);
        }
    }

    public void SetEnable()
    {
        ApplyTorqueEffect(true);
        PlayAnimation();
        ApplyFadeEffect(false);
        SetDeactiveCircleState(true);
        SetParticleState(true);
    }

    public void SetDisable()
    {
        ApplyTorqueEffect(false);
        PlayAnimation();
        ApplyFadeEffect(true);
        SetDeactiveCircleState(false);
        SetParticleState(false);
    }

    private void ApplyTorqueEffect(bool isEnable)
    {
        if (rigi == null) return;

        if (isEnable == true)
        {
            rigi.AddTorque(torqueVector * torquePower);
        }
        else
        {
            rigi.angularVelocity = Vector3.zero;
        }
    }

    private void PlayAnimation()
    {
        if (animator == null) return;

        animator.SetTrigger(_animHash);
    }

    private void ApplyFadeEffect(bool isFadeOut)
    {
        if (_targetRenderers == null || _targetRenderers.Length == 0) return;
        
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StartCoroutine(FadeMaterialAlphaRoutine(isFadeOut));
    }

    private void SetDeactiveCircleState(bool isEnable)
    {
        if (deactiveCircle == null) return;

        deactiveCircle.gameObject.SetActive(isEnable == false);
    }

    private void SetParticleState(bool isEnable)
    {
        if (_particles == null || _particles.Length == 0) return;

        for (int i = 0; i < _particles.Length; i++)
        {
            if (_particles[i] == null) continue;

            _particles[i].gameObject.SetActive(isEnable);
            if (isEnable == true)
            {
                _particles[i].Play();
            }
        }
    }

    private IEnumerator FadeMaterialAlphaRoutine(bool isFadeOut)
    {
        float currentTime = 0f;
        
        Material[] materials = new Material[_targetRenderers.Length];
        Color[] startColors = new Color[_targetRenderers.Length];
        bool[] hasBaseColors = new bool[_targetRenderers.Length];
        
        for (int i = 0; i < _targetRenderers.Length; i++)
        {
            if (_targetRenderers[i] != null)
            {
                materials[i] = _targetRenderers[i].material;
                hasBaseColors[i] = materials[i].HasProperty("_BaseColor");
                startColors[i] = hasBaseColors[i] == true ? materials[i].GetColor("_BaseColor") : materials[i].color;
            }
        }

        while (currentTime < _fadeDuration)
        {
            currentTime += Time.deltaTime;
            
            for (int i = 0; i < materials.Length; i++)
            {
                Material currentMaterial = materials[i];
                if (currentMaterial == null) continue;

                Color initialColor = startColors[i];
                float targetAlpha = isFadeOut == true ? 0f : 1f;
                float newAlpha = Mathf.Lerp(initialColor.a, targetAlpha, currentTime / _fadeDuration);
                Color newColor = new Color(initialColor.r, initialColor.g, initialColor.b, newAlpha);
                
                if (hasBaseColors[i] == true)
                {
                    currentMaterial.SetColor("_BaseColor", newColor);
                }
                else
                {
                    currentMaterial.color = newColor;
                }
            }
            
            yield return null;
        }
    }
}
