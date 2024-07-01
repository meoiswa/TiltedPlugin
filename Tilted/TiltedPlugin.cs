﻿using System.IO;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tilted
{
    public sealed class TiltedPlugin : IDalamudPlugin
  {
    public string Name => "Tilted";

    private const string commandName = "/tilted";

    public IDalamudPluginInterface PluginInterface { get; init; }
    public ICommandManager CommandManager { get; init; }
    public ConfigurationMKII Configuration { get; init; }
    public WindowSystem WindowSystem { get; init; }
    public CameraTilter CameraTilter { get; init; }
    public TiltedUI Window { get; init; }

    public TiltedPlugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager)
    {
      pluginInterface.Create<Service>();

      PluginInterface = pluginInterface;
      CommandManager = commandManager;

      WindowSystem = new("TiltedPlugin");

      Configuration = LoadConfiguration();
      Configuration.Initialize(SaveConfiguration);

      CameraTilter = new CameraTilter(Configuration);

      Window = new TiltedUI(Configuration)
      {
        IsOpen = Configuration.IsVisible
      };

      WindowSystem.AddWindow(Window);

      CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
      {
        HelpMessage = "opens the configuration window"
      });

      Service.Framework.Update += CameraTilter.OnUpdate;
      PluginInterface.UiBuilder.Draw += DrawUI;
      PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
      PluginInterface.UiBuilder.OpenMainUi += DrawConfigUI;
    }

    public void Dispose()
    {
      Service.Framework.Update -= CameraTilter.OnUpdate;
      PluginInterface.UiBuilder.Draw -= DrawUI;
      PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
      PluginInterface.UiBuilder.OpenMainUi -= DrawConfigUI;

      CommandManager.RemoveHandler(commandName);

      WindowSystem.RemoveAllWindows();
    }

    private ConfigurationMKII LoadConfiguration()
    {
      JObject? baseConfig = null;
      if (File.Exists(PluginInterface.ConfigFile.FullName))
      {
        var configJson = File.ReadAllText(PluginInterface.ConfigFile.FullName);
        baseConfig = JObject.Parse(configJson);
      }

      if (baseConfig != null)
      {
        if ((int?)baseConfig["Version"] == 0)
        {
          var configmki = baseConfig.ToObject<ConfigurationMKI>();
          if (configmki != null)
          {
            return ConfigurationMKII.FromConfigurationMKI(configmki);
          }
        }
        else if ((int?)baseConfig["Version"] == 1)
        {
          return baseConfig.ToObject<ConfigurationMKII>() ?? new ConfigurationMKII();
        }
      }

      return new ConfigurationMKII();
    }

    public void SaveConfiguration()
    {
      var configJson = JsonConvert.SerializeObject(Configuration, Formatting.Indented);
      File.WriteAllText(PluginInterface.ConfigFile.FullName, configJson);
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
