using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace Tilted
{
  public sealed class TiltedPlugin : IDalamudPlugin
  {
    public string Name => "Tilted";

    private const string commandName = "/tilted";

    public DalamudPluginInterface PluginInterface { get; init; }
    public CommandManager CommandManager { get; init; }
    public ChatGui ChatGui { get; init; }
    public Configuration Configuration { get; init; }
    public WindowSystem WindowSystem { get; init; }
    public CameraTilter CameraTilter { get; init; }
    public TiltedUI Window { get; init; }

    public TiltedPlugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager,
        [RequiredVersion("1.0")] ChatGui chatGui)
    {
      pluginInterface.Create<Service>();

      PluginInterface = pluginInterface;
      CommandManager = commandManager;
      ChatGui = chatGui;
      WindowSystem = new("TiltedPlugin");

      Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
      Configuration.Initialize(PluginInterface);

      CameraTilter = new CameraTilter(this);
      Service.Condition.ConditionChange += CameraTilter.OnConditionChange;
      Service.Framework.Update += CameraTilter.OnUpdate;

      Window = new TiltedUI(this)
      {
        IsOpen = Configuration.IsVisible
      };

      WindowSystem.AddWindow(Window);

      CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
      {
        HelpMessage = "opens the configuration window"
      });

      PluginInterface.UiBuilder.Draw += DrawUI;
      PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
    }

    public void Dispose()
    {
      PluginInterface.UiBuilder.Draw -= DrawUI;
      PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;

      Service.Condition.ConditionChange -= CameraTilter.OnConditionChange;
      Service.Framework.Update -= CameraTilter.OnUpdate;

      WindowSystem.RemoveAllWindows();

      CommandManager.RemoveHandler(commandName);
    }

    public void PrintDebug(string message)
    {
      if (Configuration.DebugMessages)
      {
        ChatGui.Print(message);
      }
    }

    private void SetVisible(bool isVisible)
    {
      Configuration.IsVisible = isVisible;
      Configuration.Save();

      Window.IsOpen = Configuration.IsVisible;
    }

    private void OnCommand(string command, string args)
    {
      SetVisible(!Configuration.IsVisible);
    }

    private void DrawUI()
    {
      WindowSystem.Draw();
    }

    private void DrawConfigUI()
    {
      SetVisible(!Configuration.IsVisible);
    }
  }
}
