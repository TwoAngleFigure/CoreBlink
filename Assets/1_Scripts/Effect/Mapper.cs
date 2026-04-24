using System.Collections.Generic;

public static class Mapper
{
    private static readonly Dictionary<AbillityType, BaseMovementState> s_abilityPool = new Dictionary<AbillityType, BaseMovementState>();
    private static readonly Dictionary<EffectType, BaseEffect> s_effectPool = new Dictionary<EffectType, BaseEffect>();

    public static BaseMovementState AbillityTypeMapper(AbillityType type)
    {
        if (s_abilityPool.TryGetValue(type, out BaseMovementState cachedState))
        {
            return cachedState;
        }

        BaseMovementState newState = null;
        switch (type)
        {
            case AbillityType.None:
                newState = new MovementState_Normal();
                break;
            case AbillityType.Dash:
                newState = new DashState();
                break;
            case AbillityType.Wire:
                return null;
            case AbillityType.Teleport:
                return null;
        }

        if (newState != null)
        {
            s_abilityPool.Add(type, newState);
        }

        return newState;
    }

    public static BaseEffect EffectTypeMapper(EffectType type)
    {
        if (s_effectPool.TryGetValue(type, out BaseEffect cachedEffect))
        {
            return cachedEffect;
        }

        BaseEffect newEffect = null;
        switch (type)
        {
            case EffectType.ChangeAbillity_Dash:
                newEffect = new Effect_ChangeAbility(AbillityType.Dash);
                break;
            case EffectType.RecoveryJump:
                newEffect = new Effect_RecoveryJump();
                break;
            case EffectType.RecoveryAbility:
                newEffect = new Effect_RecoveryAbility();
                break;
        }

        if (newEffect != null)
        {
            s_effectPool.Add(type, newEffect);
        }

        return newEffect;
    }
}

public enum EffectType
{
    //ChangeAbillity
    ChangeAbillity_Dash,

    //Jump
    RecoveryJump,

    //Ability
    RecoveryAbility,
}

public enum AbillityType
{
    None,
    Dash,
    Wire,
    Teleport,
}