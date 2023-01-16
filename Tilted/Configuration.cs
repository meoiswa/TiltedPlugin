using Dalamud.Configuration;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;

namespace Tilted
{
  [Serializable]
  public unsafe class Configuration : IPluginConfiguration
  {
    public int Version { get; set; } = 0;

    public bool Enabled { get; set; } = true;
    public bool EnabledInInstance { get; set; } = true;
    public bool EnabledInCombat { get; set; } = true;

    public bool SmoothingInCombat { get; set; } = true;
    public bool SmoothingInInstance { get; set; } = true;

    public int EnabledCameraTilt { get; set; } = 0;
    public int DisabledCameraTilt { get; set; } = 0;

    public int CombatTimeoutSeconds { get; set; } = 0;

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private DalamudPluginInterface? pluginInterface;
    public void Initialize(DalamudPluginInterface pluginInterface) => this.pluginInterface = pluginInterface;
    public void Save() => this.pluginInterface!.SavePluginConfig(this);
  }
}
