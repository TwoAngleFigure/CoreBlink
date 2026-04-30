using UnityEngine;

[CreateAssetMenu(fileName = "New Player Look Data", menuName = "SO/Player Look Data")]
public class PlayerLookData : ScriptableObject
{
    public Mesh mesh;
    public int shapeAngle;

    [Header("Skin UI")]
    [Tooltip("스킨 선택 UI에 표시될 아이콘")]
    public Sprite icon;
}
