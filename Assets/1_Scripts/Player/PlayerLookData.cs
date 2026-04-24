using UnityEngine;

[CreateAssetMenu(fileName = "New Player Look Data", menuName = "SO/Player Look Data")]
public class PlayerLookData : ScriptableObject
{
    public Mesh mesh;
    public int shapeAngle;
}
