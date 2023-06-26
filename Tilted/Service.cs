using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Config;
using Dalamud.Game.Gui;
using Dalamud.IoC;

namespace Tilted
{
  public class Service
  {
#pragma warning disable CS8618
    [PluginService] public static ChatGui ChatGui { get; private set; }
    [PluginService] public static Condition Condition { get; private set; }
    [PluginService] public static Framework Framework { get; private set; }
    [PluginService] public static GameConfig GameConfig { get; private set; }
#pragma warning restore CS8618
  }
}
