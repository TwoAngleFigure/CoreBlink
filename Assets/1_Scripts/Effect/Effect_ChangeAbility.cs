using UnityEngine;

public class Effect_ChangeAbility : BaseEffect
{
    public AbillityType _abillityType;

    public Effect_ChangeAbility(AbillityType abillityType)
    {
        _abillityType = abillityType;
    }

    public override void Effect(GameObject target)
    {
        if (target.TryGetComponent<PlayerManager>(out PlayerManager player))
        {
            BaseMovementState state = Mapper.AbillityTypeMapper(_abillityType);
            player.PlayerChangeAbillity(state);
        }
    }
}