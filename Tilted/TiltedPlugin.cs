using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;

namespace Tilted
{
    public sealed class TiltedPlugin : IDalamudPlugin
    {
        public string Name => "Tilted";

        private const string commandName = "/tilted";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        private CameraTilter CameraTilter { get; init; }

        public TiltedPlugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            pluginInterface.Create<Service>();

            PluginInterface = pluginInterface;
            CommandManager = commandManager;

            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            PluginUi = new PluginUI(Configuration);

            CameraTilter = new CameraTilter(Configuration);
            Service.Condition.ConditionChange += CameraTilter.OnConditionChange;

            Service.Framework.Update += CameraTilter.OnUpdate;

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

            PluginUi.Dispose();

            CommandManager.RemoveHandler(commandName);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            PluginUi.Visible = true;
        }

        private void DrawUI()
        {
            PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            PluginUi.Visible = true;
        }
    }
}
