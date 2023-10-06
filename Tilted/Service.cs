using Dalamud.IoC;
using Dalamud.Plugin.Services;

namespace Tilted
{
  public class Service
  {
#pragma warning disable CS8618
    [PluginService] public static IChatGui ChatGui { get; private set; }
    [PluginService] public static ICondition Condition { get; private set; }
    [PluginService] public static IFramework Framework { get; private set; }
    [PluginService] public static IGameConfig GameConfig { get; private set; }
    [PluginService] public static IPluginLog PluginLog { get; private set; }
#pragma warning restore CS8618
  }
}
