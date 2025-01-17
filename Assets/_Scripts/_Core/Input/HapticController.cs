using Lofelt.NiceVibrations;
using StarWriter.Core;
using UnityEngine;

public enum HapticType
{
    None = 0,
    ButtonPress = 1,
    BlockCollision = 2,
    ShipCollision = 3,
    CystalCollision = 4,
    DecoyCollision = 5,
}

public class HapticController : MonoBehaviour
{
    static HapticPatterns.PresetType ButtonPattern = HapticPatterns.PresetType.LightImpact;
    static HapticPatterns.PresetType CrystalCollisionPattern = HapticPatterns.PresetType.MediumImpact;
    static HapticPatterns.PresetType BlockCollisionPattern = HapticPatterns.PresetType.Success;
    static HapticPatterns.PresetType ShipCollisionPattern = HapticPatterns.PresetType.HeavyImpact;
    static HapticPatterns.PresetType FakeCrystalCollisionPattern = HapticPatterns.PresetType.HeavyImpact;

    public static void PlayPreset(int option)
    {
        if (!GameSetting.Instance.HapticsEnabled)
            return;

        HapticPatterns.PlayPreset((HapticPatterns.PresetType) option);
    }

    public static void PlayHaptic(HapticType type)
    {
        if (!GameSetting.Instance.HapticsEnabled)
            return;

        switch (type)
        {
            case HapticType.ButtonPress:
                PlayButtonPressHaptics();
                break;
            case HapticType.BlockCollision:
                PlayBlockCollisionHaptics();
                break;
            case HapticType.ShipCollision:
                PlayShipCollisionHaptics();
                break;
            case HapticType.CystalCollision:
                PlayCrystalImpactHaptics();
                break;
            case HapticType.DecoyCollision:
                PlayFakeCrystalImpactHaptics();
                break;
        }
    }

    public static void PlayButtonPressHaptics()
    {
        PlayPreset((int)ButtonPattern);
    }
    public static void PlayCrystalImpactHaptics()
    {
        PlayPreset((int) CrystalCollisionPattern);
    }
    public static void PlayBlockCollisionHaptics()
    {
        PlayPreset((int) BlockCollisionPattern);
    }
    public static void PlayShipCollisionHaptics()
    {
        PlayPreset((int) ShipCollisionPattern);
    }
    public static void PlayFakeCrystalImpactHaptics()
    {
        PlayPreset((int) FakeCrystalCollisionPattern);
    }
}
/*
haptic preset notes:

0, 1, 4, 8  = would good for UI use - feedback for correct input on tutorial
2, 5, 7 - might be good for running through stuff (positive)
3 - Not in use (negative) crash? odd pattern - I wouldn't use it unless its going to match an animation cause it might seem out of place otherwise

5 - Crystal
4 - UI
6 - crash into blocks - intense (negative feedback) 
*/
