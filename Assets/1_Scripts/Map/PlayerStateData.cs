using System;
using UnityEngine;

public class PlayerStateData
{
    public Type SavedMovementType { get; private set; }
    public Color SavedCoreColor { get; private set; }

    public PlayerStateData(Type movementType, Color coreColor)
    {
        SavedMovementType = movementType;
        SavedCoreColor = coreColor;
    }
}
