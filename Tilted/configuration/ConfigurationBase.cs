using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace Tilted
{
  [Serializable]
  public class ConfigurationBase : IPluginConfiguration
  {
    public virtual int Version { get; set; } = 0;

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private TiltedPlugin? plugin;
    public void Initialize(TiltedPlugin plugin) => this.plugin = plugin;
    public void Save()
    {
      plugin!.SaveConfiguration();
    }
  }
}
