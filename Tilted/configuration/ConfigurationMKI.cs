using System;

namespace Tilted
{
    [Serializable]
  public class ConfigurationMKI : ConfigurationBase
  {
    public override int Version { get; set; } = 0;

    public bool IsVisible { get; set; } = true;

    public bool Enabled { get; set; } = true;
    public bool EnabledInDuty { get; set; } = false;
    public bool EnabledInCombat { get; set; } = false;
    public bool EnabledUnsheathed { get; set; } = false;
    public bool EnabledWhileMounted { get; set; } = false;

    public bool SmoothingTilt { get; set; } = true;

    public bool TweakCameraTilt { get; set; } = false;

    public float EnabledCameraTilt { get; set; } = TiltedHelper.GetTiltOffset();
    public float DisabledCameraTilt { get; set; } = TiltedHelper.GetTiltOffset();

    public bool TweakCameraDistance { get; set; } = false;

    public float EnabledCameraDistance { get; set; } = TiltedHelper.GetActiveCameraDistance();
    public float DisabledCameraDistance { get; set; } = TiltedHelper.GetActiveCameraDistance();

    public float CombatTimeoutSeconds { get; set; } = TiltedHelper.GetWeaponAutoPutAwayTime();

    public bool DebugForceEnabled = false;
    public bool DebugMessages = false;
  }
}
