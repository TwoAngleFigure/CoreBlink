using UnityEngine;

public class Effect_RecoveryAbility : BaseEffect
{
    public override void Effect(GameObject target)
    {
        if (target.TryGetComponent<PlayerInput>(out PlayerInput state))
        {
            state._moveState.SetCanAbility(true);
        }
    }
}
