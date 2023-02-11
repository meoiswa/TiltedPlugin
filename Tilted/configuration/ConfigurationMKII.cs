﻿using Dalamud.Configuration;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;

namespace Tilted
{
  [Serializable]
  public unsafe class ConfigurationMKII : ConfigurationBase
  {
    public override int Version { get; set; } = 1;

    public bool IsVisible { get; set; } = true;

    public bool MasterEnable { get; set; } = true;
    public bool EnableInDuty { get; set; } = false;
    public bool EnableInCombat { get; set; } = false;
    public bool EnableUnsheathed { get; set; } = false;
    public bool EnableMounted { get; set; } = false;

    public float CombatTimeoutSeconds { get; set; } = ConfigModule.Instance()->GetIntValue(ConfigOption.WeaponAutoPutAwayTime);

    public bool EnableTweakingCameraTilt { get; set; } = false;
    public bool EnableCameraTiltSmoothing { get; set; } = true;
    public int CameraTiltWhenEnabled { get; set; } = ConfigModule.Instance()->GetIntValue(ConfigOption.TiltOffset);
    public int CameraTiltWhenDisabled { get; set; } = ConfigModule.Instance()->GetIntValue(ConfigOption.TiltOffset);

    public bool EnableCameraDistanceTweaking { get; set; } = false;
    public float CameraDistanceWhenEnabled { get; set; } = CameraManager.Instance->GetActiveCamera()->Distance;
    public float CameraDistanceWhenDisabled { get; set; } = CameraManager.Instance->GetActiveCamera()->Distance;

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
