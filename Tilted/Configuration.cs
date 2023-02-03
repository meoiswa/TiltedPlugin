using Dalamud.Configuration;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;

namespace Tilted
{
  [Serializable]
  public unsafe class Configuration : IPluginConfiguration
  {
    public int Version { get; set; } = 0;

    public bool IsVisible { get; set; } = true;

    public bool Enabled { get; set; } = true;
    public bool EnabledInDuty { get; set; } = false;
    public bool EnabledInCombat { get; set; } = false;
    public bool EnabledUnsheathed { get; set; } = false;
    public bool EnabledWhileMounted { get; set; } = false;

    public bool SmoothingTilt { get; set; } = true;

    public bool TweakCameraTilt { get; set; } = false;

    public int EnabledCameraTilt { get; set; } = ConfigModule.Instance()->GetIntValue(ConfigOption.TiltOffset);
    public int DisabledCameraTilt { get; set; } = ConfigModule.Instance()->GetIntValue(ConfigOption.TiltOffset);

    public bool TweakCameraDistance { get; set; } = false;

    public float EnabledCameraDistance { get; set; } = CameraManager.Instance->GetActiveCamera()->Distance;
    public float DisabledCameraDistance { get; set; } = CameraManager.Instance->GetActiveCamera()->Distance;

    public float CombatTimeoutSeconds { get; set; } = ConfigModule.Instance()->GetIntValue(ConfigOption.WeaponAutoPutAwayTime);

    public bool DebugForceEnabled = false;
    public bool DebugMessages = false;

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private DalamudPluginInterface? pluginInterface;
    public void Initialize(DalamudPluginInterface pluginInterface) => this.pluginInterface = pluginInterface;
    public void Save()
    {
      pluginInterface!.SavePluginConfig(this);
    }
  }
}
