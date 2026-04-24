using UnityEngine;

public class Effect_RecoveryJump : BaseEffect
{
    public override void Effect(GameObject target)
    {
        if(target.TryGetComponent<PlayerInput>(out PlayerInput state))
        {
            state._moveState.SetCanAirJump(true);
        }
    }
}
