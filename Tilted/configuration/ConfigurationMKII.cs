using System;

namespace Tilted
{
    [Serializable]
  public class ConfigurationMKII : ConfigurationBase
  {
    public override int Version { get; set; } = 1;

    public bool IsVisible { get; set; } = true;

    public bool MasterEnable { get; set; } = true;
    public bool EnableInDuty { get; set; } = false;
    public bool EnableInCombat { get; set; } = false;
    public bool EnableUnsheathed { get; set; } = false;
    public bool EnableMounted { get; set; } = false;

    public bool EnableZoomed { get; set; } = false;

    public float CombatTimeoutSeconds { get; set; } = TiltedHelper.GetWeaponAutoPutAwayTime();

    public bool EnableTweakingCameraTilt { get; set; } = false;
    public bool EnableCameraTiltSmoothing { get; set; } = true;

    public bool EnableDistanceToTiltMapping { get; set; } = true;
    
    public float CameraTiltWhenEnabled { get; set; } = TiltedHelper.GetTiltOffset();
    public float CameraTiltWhenDisabled { get; set; } = TiltedHelper.GetTiltOffset();

    public bool EnableCameraDistanceTweaking { get; set; } = false;
    public float CameraDistanceWhenEnabled { get; set; } = TiltedHelper.GetActiveCameraDistance();
    public float CameraDistanceWhenDisabled { get; set; } = TiltedHelper.GetActiveCameraDistance();

    public float ZoomedTriggerDistance { get; set; } = TiltedHelper.GetActiveCameraDistance();

    public bool DebugForceEnabled = false;
    public bool DebugMessages = false;
    
    public static ConfigurationMKII FromConfigurationMKI(ConfigurationMKI legacy)
    {
      return new ConfigurationMKII()
      {
        IsVisible = legacy.IsVisible,

        MasterEnable = legacy.Enabled,
        EnableInDuty = legacy.EnabledInDuty,
        EnableInCombat = legacy.EnabledInCombat,
        EnableUnsheathed = legacy.EnabledUnsheathed,
        EnableMounted = legacy.EnabledWhileMounted,

        CombatTimeoutSeconds = legacy.CombatTimeoutSeconds,

        EnableTweakingCameraTilt = legacy.TweakCameraTilt,
        EnableCameraTiltSmoothing = legacy.SmoothingTilt,
        CameraTiltWhenEnabled = legacy.EnabledCameraTilt,
        CameraTiltWhenDisabled = legacy.DisabledCameraTilt,

        EnableCameraDistanceTweaking = legacy.TweakCameraDistance,
        CameraDistanceWhenEnabled = legacy.EnabledCameraDistance,
        CameraDistanceWhenDisabled = legacy.DisabledCameraDistance,

        DebugForceEnabled = legacy.DebugForceEnabled,
        DebugMessages = legacy.DebugMessages,
      };
    }
  }
}
