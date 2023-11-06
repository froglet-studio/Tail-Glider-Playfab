using StarWriter.Core;
using System;
using _Scripts._Core.Input;

[Serializable]
public struct ImpactProperties
{
    public float fuelBonus;
    public float fuelPenalty;
    public int scoreBonus;
    public int scorePenalty;
    public float tailLengthBonus;
    public float tailLengthPenalty;
    public float speedBonus;
    public float speedPenalty;
    public HapticType hapticType;
    public TrailBlock trailBlock;
}