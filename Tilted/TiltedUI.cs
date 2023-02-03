using Dalamud.Logging;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ImGuiNET;
using System;
using System.Numerics;

namespace Tilted
{
  // It is good to have this be disposable in general, in case you ever need it
  // to do any cleanup
  unsafe class TiltedUI : Window, IDisposable
  {
    private readonly TiltedPlugin plugin;

    public TiltedUI(TiltedPlugin plugin)
      : base(
        "Tilted##ConfigWindow",
        ImGuiWindowFlags.AlwaysAutoResize
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoCollapse
      )
    {
      this.plugin = plugin;

      SizeConstraints = new WindowSizeConstraints()
      {
        MinimumSize = new Vector2(468, 0),
        MaximumSize = new Vector2(468, 1000)
      };
    }

    public void Dispose()
    {
    }

    public override void OnClose()
    {
      base.OnClose();
      plugin.Configuration.IsVisible = false;
      plugin.Configuration.Save();
    }

    public override void Draw()
    {
      // can't ref a property, so use a local copy
      var enabled = plugin.Configuration.Enabled;
      if (ImGui.Checkbox("Master Enable", ref enabled))
      {
        plugin.Configuration.Enabled = enabled;
        plugin.Configuration.Save();
      }

      ImGui.Separator();

      if (ImGui.CollapsingHeader("Triggers"))
      {
        ImGui.Indent();

        if (ImGui.CollapsingHeader("Duties"))
        {
          ImGui.Indent();
          ImGui.TextWrapped("Enables when entering a Duty such as Dungeons and Trials.\nDisables after leaving the Duty.");
          var enabledInInstance = plugin.Configuration.EnabledInDuty;
          if (ImGui.Checkbox("Trigger##EnabledInDuties", ref enabledInInstance))
          {
            plugin.Configuration.EnabledInDuty = enabledInInstance;
            plugin.Configuration.Save();
            plugin.CameraTilter.ValidateCurrentState();
          }
          ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("Combat"))
        {
          ImGui.Indent();
          ImGui.TextWrapped("Enables when entering combat.\nWaits \"Timeout\" seconds after leaving combat before disabling.\nClick \"Set\" to copy Auto-sheathe timer.");
          var enabledInCombat = plugin.Configuration.EnabledInCombat;
          if (ImGui.Checkbox("Trigger##EnabledInCombat", ref enabledInCombat))
          {
            plugin.Configuration.EnabledInCombat = enabledInCombat;
            plugin.Configuration.Save();
            plugin.CameraTilter.ValidateCurrentState();
          }

          var combatTimeout = plugin.Configuration.CombatTimeoutSeconds;
          if (ImGui.Button("Set##SetCombatTimeout"))
          {
            combatTimeout = ConfigModule.Instance()->GetIntValue(ConfigOption.WeaponAutoPutAwayTime);
          }
          ImGui.SameLine();
          if (ImGui.InputFloat("Timeout##CombatTmeout", ref combatTimeout, 0.1f, 1.0f))
          {
            combatTimeout = Math.Clamp(combatTimeout, 0f, 10f);
            plugin.Configuration.CombatTimeoutSeconds = combatTimeout;
            plugin.Configuration.Save();
          }
          ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("Weapon Sheathed"))
        {
          ImGui.Indent();
          ImGui.TextWrapped("Enables when un-sheathing weapons.\nDisables when sheathing weapons.");
          var enabledUnsheathed = plugin.Configuration.EnabledUnsheathed;
          if (ImGui.Checkbox("Trigger##EnabledUnsheathed", ref enabledUnsheathed))
          {
            plugin.Configuration.EnabledUnsheathed = enabledUnsheathed;
            plugin.Configuration.Save();
            plugin.CameraTilter.ValidateCurrentState();
          }
          ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("Mounted"))
        {
          ImGui.Indent();
          ImGui.TextWrapped("Enables when entering riding a Mount.\nDisables when dismounting.");
          var enabledWhileMounted = plugin.Configuration.EnabledWhileMounted;
          if (ImGui.Checkbox("Trigger##EnabledWhileMounted", ref enabledWhileMounted))
          {
            plugin.Configuration.EnabledWhileMounted = enabledWhileMounted;
            plugin.Configuration.Save();
            plugin.CameraTilter.ValidateCurrentState();
          }
          ImGui.Unindent();
        }

        ImGui.Unindent();
      }

      ImGui.Separator();

      if (ImGui.CollapsingHeader("Tweaks"))
      {
        ImGui.Indent();

        if (ImGui.CollapsingHeader("Tilt Angle"))
        {
          ImGui.Indent();

          ImGui.TextWrapped("These values alter the Character Configuration value:\n  \"3rd Person Camera Angle\".\nClick \"Set\" to copy the current camera tilt angle");

          var tiltEnabled = plugin.Configuration.TweakCameraTilt;
          if (ImGui.Checkbox("Enabled##TweakCameraTilt", ref tiltEnabled))
          {
            plugin.Configuration.TweakCameraTilt = tiltEnabled;
            plugin.Configuration.Save();
          }


          var inTilt = plugin.Configuration.EnabledCameraTilt;
          if (ImGui.Button("Set##InTilt"))
          {
            inTilt = ConfigModule.Instance()->GetIntValue(ConfigOption.TiltOffset);
          }
          ImGui.SameLine();
          if (ImGui.InputInt("Enabled##EnabledTilt", ref inTilt))
          {
            inTilt = Math.Clamp(inTilt, 0, 100);
            plugin.Configuration.EnabledCameraTilt = inTilt;
            plugin.Configuration.Save();
          }

          if (inTilt != plugin.Configuration.EnabledCameraTilt)
          {
            plugin.Configuration.EnabledCameraTilt = inTilt;
            plugin.Configuration.Save();
          }

          var outTilt = plugin.Configuration.DisabledCameraTilt;
          if (ImGui.Button("Set##OutTilt"))
          {
            outTilt = ConfigModule.Instance()->GetIntValue(ConfigOption.TiltOffset);
          }
          ImGui.SameLine();
          if (ImGui.InputInt("Disabled##DisabledTilt", ref outTilt))
          {
            outTilt = Math.Clamp(outTilt, 0, 100);
          }

          if (outTilt != plugin.Configuration.DisabledCameraTilt)
          {
            plugin.Configuration.DisabledCameraTilt = outTilt;
            plugin.Configuration.Save();
          }

          var smoothing = plugin.Configuration.SmoothingTilt;
          if (ImGui.Checkbox("Smoothing##SmoothingTilt", ref smoothing))
          {
            plugin.Configuration.SmoothingTilt = smoothing;
            plugin.Configuration.Save();
          }

          ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("Camera Distance"))
        {
          ImGui.Indent();

          ImGui.TextWrapped("Tweaks the camera distance (Zoom).\nOnly happens when transitioning between states\nSmoothing is always applied.\nClick \"Set\" to copy the current camera distance.");

          var distanceEnabled = plugin.Configuration.TweakCameraDistance;
          if (ImGui.Checkbox("Enabled##TweakCameraDistance", ref distanceEnabled))
          {
            plugin.Configuration.TweakCameraDistance = distanceEnabled;
            plugin.Configuration.Save();
          }

          var outDistance = plugin.Configuration.DisabledCameraDistance;

          var inDistance = plugin.Configuration.EnabledCameraDistance;
          if (ImGui.Button("Set##InDistance"))
          {
            inDistance = CameraManager.Instance->GetActiveCamera()->Distance;
          }
          ImGui.SameLine();
          if (ImGui.InputFloat("Enabled##EnabledDistance", ref inDistance, 0.1f, 1.0f))
          {
            inDistance = Math.Clamp(inDistance, 1.5f, 20f);
          }

          if (inDistance != plugin.Configuration.EnabledCameraDistance)
          {
            plugin.Configuration.EnabledCameraDistance = inDistance;
            plugin.Configuration.Save();
          }

          if (ImGui.Button("Set##OutDistance"))
          {
            outDistance = CameraManager.Instance->GetActiveCamera()->Distance;
          }
          ImGui.SameLine();
          if (ImGui.InputFloat("Disabled##DisabledDistance", ref outDistance, 0.1f, 1.0f))
          {
            outDistance = Math.Clamp(outDistance, 1.5f, 20f);
          }

          if (outDistance != plugin.Configuration.DisabledCameraDistance)
          {
            plugin.Configuration.DisabledCameraDistance = outDistance;
            plugin.Configuration.Save();
          }
          ImGui.Unindent();
        }

        ImGui.Unindent();
      }

      ImGui.Separator();

      if (ImGui.CollapsingHeader("Debug Options"))
      {
        ImGui.Indent();

        ImGui.TextWrapped("Debug Options\nUse these to test your settings.");
        var isForceEnabled = plugin.Configuration.DebugForceEnabled;
        if (ImGui.Button("Force Enabled state"))
        {
          plugin.Configuration.DebugForceEnabled = true;
          plugin.CameraTilter.ValidateCurrentState();
        }
        if (ImGui.Button("Reset state"))
        {
          plugin.Configuration.DebugForceEnabled = false;
          plugin.CameraTilter.ValidateCurrentState();
        }

        var debugMessages = plugin.Configuration.DebugMessages;
        if (ImGui.Checkbox("Debug Messages", ref debugMessages))
        {
          plugin.Configuration.DebugMessages = debugMessages;
          plugin.Configuration.Save();
        }

        ImGui.Unindent();
      }
    }
  }
}
