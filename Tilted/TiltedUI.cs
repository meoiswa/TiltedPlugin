using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace Tilted
{
  public class TiltedUI : Window, IDisposable
  {
    private readonly ConfigurationMKII configuration;

    public TiltedUI(ConfigurationMKII configuration)
      : base(
        "Tilted##ConfigWindow",
        ImGuiWindowFlags.AlwaysAutoResize
        | ImGuiWindowFlags.NoResize
        | ImGuiWindowFlags.NoCollapse
      )
    {
      this.configuration = configuration;

      SizeConstraints = new WindowSizeConstraints()
      {
        MinimumSize = new Vector2(468, 0),
        MaximumSize = new Vector2(468, 1000)
      };
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
    }

    public override void OnClose()
    {
      base.OnClose();
      configuration.IsVisible = false;
      configuration.Save();
    }

    private void DrawSectionMasterEnable()
    {
      // can't ref a property, so use a local copy
      var enabled = configuration.MasterEnable;
      if (ImGui.Checkbox("Master Enable", ref enabled))
      {
        configuration.MasterEnable = enabled;
        configuration.Save();
      }
      var enabledInGpose = configuration.EnableInGpose;
      if (ImGui.Checkbox("Enable in Gpose", ref enabledInGpose))
      {
        configuration.EnableInGpose = enabledInGpose;
        configuration.Save();
      }
    }

    private void DrawCheckbox(string label, string key, Func<bool> getter, Action<bool> setter)
    {
      ImGui.TextWrapped(label);
      var value = getter();
      if (ImGui.Checkbox(key, ref value))
      {
        setter(value);
      }
    }

    private void DrawTriggersSection()
    {
      if (ImGui.CollapsingHeader("Triggers"))
      {
        ImGui.Indent();

        if (ImGui.CollapsingHeader("Duties"))
        {
          ImGui.Indent();
          DrawCheckbox(
            "Enables when entering a Duty such as Dungeons and Trials.\nDisables after leaving the Duty.",
            "Trigger##EnabledInDuties",
            () => configuration.EnableInDuty,
            (value) =>
            {
              configuration.EnableInDuty = value;
              configuration.Save();
            }
          );
          ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("Combat"))
        {
          ImGui.Indent();
          DrawCheckbox(
            "Enables when entering combat.\nWaits \"Timeout\" seconds after leaving combat before disabling.\nClick \"Set\" to copy Auto-sheathe timer.",
            "Trigger##EnabledInCombat",
            () => configuration.EnableInCombat,
            (value) =>
            {
              configuration.EnableInCombat = value;
              configuration.Save();
            }
          );

          var combatTimeout = configuration.CombatTimeoutSeconds;
          if (ImGui.Button("Set##SetCombatTimeout"))
          {
            combatTimeout = TiltedHelper.GetWeaponAutoPutAwayTime();
          }
          ImGui.SameLine();
          if (ImGui.InputFloat("Timeout##CombatTmeout", ref combatTimeout, 0.1f, 1.0f))
          {
            combatTimeout = Math.Clamp(combatTimeout, 0f, 10f);
            configuration.CombatTimeoutSeconds = combatTimeout;
            configuration.Save();
          }
          ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("Weapon Sheathed"))
        {
          ImGui.Indent();
          DrawCheckbox(
            "Enables when un-sheathing weapons.\nDisables when sheathing weapons.",
            "Trigger##EnabledUnsheathed",
            () => configuration.EnableUnsheathed,
            (value) =>
            {
              configuration.EnableUnsheathed = value;
              configuration.Save();
            }
          );
          ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("Mounted"))
        {
          ImGui.Indent();
          DrawCheckbox(
            "Enables when riding a Mount.\nDisables when dismounting.",
            "Trigger##EnabledWhileMounted",
            () => configuration.EnableMounted,
            (value) =>
            {
              configuration.EnableMounted = value;
              configuration.Save();
            }
          );
          ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("Flying"))
        {
          ImGui.Indent();
          DrawCheckbox(
            "Enables when flying on a Mount.\nDisables when landed.",
            "Trigger##EnabledWhileFlying",
            () => configuration.EnableFlying,
            (value) =>
            {
              configuration.EnableFlying = value;
              configuration.Save();
            }
          );
          ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("Zoomed"))
        {
          ImGui.Indent();
          DrawCheckbox(
            "Enables when zooming in past certain amount.\nDisables when zooming out.",
            "Trigger##EnabledZoomed",
            () => configuration.EnableZoomed,
            (value) =>
            {
              configuration.EnableZoomed = value;
              configuration.Save();
            }
          );

          var triggerDistance = configuration.ZoomedTriggerDistance;
          if (ImGui.Button("Set##TriggerDistance"))
          {
            triggerDistance = TiltedHelper.GetActiveCameraDistance();
          }
          ImGui.SameLine();
          if (ImGui.InputFloat("Enabled##TriggerDistance", ref triggerDistance, 0.1f, 1.0f))
          {
            triggerDistance = Math.Clamp(triggerDistance, 1.5f, 20f);
          }

          if (triggerDistance != configuration.ZoomedTriggerDistance)
          {
            configuration.ZoomedTriggerDistance = triggerDistance;
            configuration.Save();
          }

          ImGui.Unindent();
        }

        ImGui.Unindent();
      }
    }

    private void DrawTweaksSection()
    {
      if (ImGui.CollapsingHeader("Tweaks"))
      {
        ImGui.Indent();

        DrawTiltAngleSection();
        DrawCameraDistanceSection();

        ImGui.Unindent();
      }
    }

    private void DrawTiltAngleSection()
    {
      if (ImGui.CollapsingHeader("Tilt Angle"))
      {
        ImGui.Indent();

        ImGui.TextWrapped("These values alter the Character Configuration value:\n  \"3rd Person Camera Angle\".\nClick \"Set\" to copy the current camera tilt angle");

        var tiltEnabled = configuration.EnableTweakingCameraTilt;
        if (ImGui.Checkbox("Enabled##TweakCameraTilt", ref tiltEnabled))
        {
          configuration.EnableTweakingCameraTilt = tiltEnabled;
          configuration.Save();
        }

        int inTilt = (int)configuration.CameraTiltWhenEnabled;
        if (ImGui.Button("Set##InTilt"))
        {
          inTilt = (int)TiltedHelper.GetTiltOffset();
        }
        ImGui.SameLine();
        if (ImGui.InputInt("Enabled##EnabledTilt", ref inTilt))
        {
          inTilt = Math.Clamp(inTilt, 0, 100);
        }

        if (inTilt != configuration.CameraTiltWhenEnabled)
        {
          configuration.CameraTiltWhenEnabled = (uint)inTilt;
          configuration.Save();
        }

        int outTilt = (int)configuration.CameraTiltWhenDisabled;
        if (ImGui.Button("Set##OutTilt"))
        {
          outTilt = (int)TiltedHelper.GetTiltOffset();
        }
        ImGui.SameLine();
        if (ImGui.InputInt("Disabled##DisabledTilt", ref outTilt))
        {
          outTilt = Math.Clamp(outTilt, 0, 100);
        }

        if (outTilt != configuration.CameraTiltWhenDisabled)
        {
          configuration.CameraTiltWhenDisabled = (uint)outTilt;
          configuration.Save();
        }

        var smoothing = configuration.EnableCameraTiltSmoothing;
        if (ImGui.Checkbox("Smoothing##SmoothingTilt", ref smoothing))
        {
          configuration.EnableCameraTiltSmoothing = smoothing;
          configuration.Save();
        }

        var mapping = configuration.EnableDistanceToTiltMapping;
        if (ImGui.Checkbox("Interpolate by Distance##MappingTilt", ref mapping))
        {
          configuration.EnableDistanceToTiltMapping = mapping;
          configuration.Save();
        }

        float maximumDistance = configuration.MaximumCameraDistance;
        if (ImGui.Button("Set##MaximumDistance"))
        {
          maximumDistance = TiltedHelper.GetActiveCameraDistance();
          configuration.MaximumCameraDistance = maximumDistance;
          configuration.Save();
        }
        ImGui.SameLine();
        if (ImGui.InputFloat("Maximum##MaximumDistance", ref maximumDistance))
        {
          maximumDistance = Math.Clamp(maximumDistance, 1.5f, 20.0f);
          configuration.MaximumCameraDistance = maximumDistance;
          configuration.Save();
        }

        float minimumDistance = configuration.MinimumCameraDistance;
        if (ImGui.Button("Set##MinimumDistance"))
        {
          minimumDistance = TiltedHelper.GetActiveCameraDistance();
          configuration.MinimumCameraDistance = minimumDistance;
          configuration.Save();
        }
        ImGui.SameLine();
        if (ImGui.InputFloat("Minimum##MinimumDistance", ref minimumDistance))
        {
          minimumDistance = Math.Clamp(minimumDistance, 1.5f, 20.0f);
          configuration.MinimumCameraDistance = minimumDistance;
          configuration.Save();
        }

        ImGui.Indent();
        ImGui.TextWrapped("When this setting is enabled the Camera Tilt will be set to a value in-between the \"Enabled\" and \"Disabled\" values based on the camera's distance from your character."
          + "\nTriggers and Smoothing will have no effect while this setting is enabled."
          );
        ImGui.Unindent();

        ImGui.Unindent();
      }
    }

    private void DrawCameraDistanceSection()
    {
      if (ImGui.CollapsingHeader("Camera Distance"))
      {
        ImGui.Indent();

        ImGui.TextWrapped("Tweaks the camera distance (Zoom)."
          + "\nOnly happens when transitioning between states"
          + "\nSmoothing is always applied."
          + "\nClick \"Set\" to copy the current camera distance."
          + "\n(Disabled when using Zoomed trigger.");

        var distanceEnabled = configuration.EnableCameraDistanceTweaking;
        if (ImGui.Checkbox("Enabled##TweakCameraDistance", ref distanceEnabled))
        {
          configuration.EnableCameraDistanceTweaking = distanceEnabled;
          configuration.Save();
        }

        var inDistance = configuration.CameraDistanceWhenEnabled;
        if (ImGui.Button("Set##InDistance"))
        {
          inDistance = TiltedHelper.GetActiveCameraDistance();
        }
        ImGui.SameLine();
        if (ImGui.InputFloat("Enabled##EnabledDistance", ref inDistance, 0.1f, 1.0f))
        {
          inDistance = Math.Clamp(inDistance, 1.5f, 20f);
        }

        if (inDistance != configuration.CameraDistanceWhenEnabled)
        {
          configuration.CameraDistanceWhenEnabled = inDistance;
          configuration.Save();
        }

        var outDistance = configuration.CameraDistanceWhenDisabled;
        if (ImGui.Button("Set##OutDistance"))
        {
          outDistance = TiltedHelper.GetActiveCameraDistance();
        }
        ImGui.SameLine();
        if (ImGui.InputFloat("Disabled##DisabledDistance", ref outDistance, 0.1f, 1.0f))
        {
          outDistance = Math.Clamp(outDistance, 1.5f, 20f);
        }

        if (outDistance != configuration.CameraDistanceWhenDisabled)
        {
          configuration.CameraDistanceWhenDisabled = outDistance;
          configuration.Save();
        }
        ImGui.Unindent();
      }
    }

    public void DrawDebugSection()
    {
      if (ImGui.CollapsingHeader("Debug Options"))
      {
        ImGui.Indent();

        ImGui.TextWrapped("Debug Options\nUse these to test your settings.");
        if (ImGui.Button("Force Enabled state"))
        {
          configuration.DebugForceEnabled = true;
        }
        if (ImGui.Button("Reset state"))
        {
          configuration.DebugForceEnabled = false;
        }

        var debugMessages = configuration.DebugMessages;
        if (ImGui.Checkbox("Debug Messages", ref debugMessages))
        {
          configuration.DebugMessages = debugMessages;
          configuration.Save();
        }

        ImGui.Unindent();
      }
    }

    public override void Draw()
    {
      DrawSectionMasterEnable();

      ImGui.Separator();

      DrawTriggersSection();

      ImGui.Separator();

      DrawTweaksSection();

      ImGui.Separator();

      DrawDebugSection();
    }
  }
}
