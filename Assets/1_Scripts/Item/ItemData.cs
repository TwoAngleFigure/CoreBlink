using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "SO/Item Data")]
public class ItemData : ScriptableObject
{
    public Color color;

    public EffectType[] effects;
}
