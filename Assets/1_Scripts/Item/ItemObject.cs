using UnityEngine;

public class ItemObject : MonoBehaviour
{
    [SerializeField] ItemData data;

    [SerializeField] ItemLook look;

    [SerializeField] LayerMask targetLayer;

    [SerializeField] float _coolDown = 3f;
    bool _isCoolingDown = false;
    Coroutine _cooldownCoroutine;

    public void Awake()
    {
        if (look == null) look = GetComponent<ItemLook>();
        look.Initialize(data);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (_isCoolingDown == true) return;

        if (((1 << other.gameObject.layer) & targetLayer.value) != 0)
        {
            foreach (EffectType effectType in data.effects)
            {
                BaseEffect effect = Mapper.EffectTypeMapper(effectType);
                if (effect != null)
                {
                    effect.Effect(other.gameObject);
                }
            }
            
            _cooldownCoroutine = StartCoroutine(CooldownRoutine());
        }
    }

    public void ResetCooldown()
    {
        if (_cooldownCoroutine != null)
        {
            StopCoroutine(_cooldownCoroutine);
            _cooldownCoroutine = null;
        }

        _isCoolingDown = false;
        look.SetEnable();
    }

    private System.Collections.IEnumerator CooldownRoutine()
    {
        _isCoolingDown = true;
        look.SetDisable();

        yield return new WaitForSeconds(_coolDown);

        look.SetEnable();
        _isCoolingDown = false;
        _cooldownCoroutine = null;
    }
}
