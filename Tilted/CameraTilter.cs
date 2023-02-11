using System;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace Tilted
{
  public unsafe class CameraTilter
  {
    private readonly TiltedPlugin plugin;
    private readonly GameMain* gameMain;
    private readonly CameraManager* cameraManager;

    private readonly ConfigModule* configModule;
    private readonly UIState* uiState;

    private bool IsMounted => plugin.Condition[ConditionFlag.Mounted];
    private bool InCombat => plugin.Condition[ConditionFlag.InCombat];
    private bool BoundByDuty => plugin.Condition[ConditionFlag.BoundByDuty] && !plugin.Condition[ConditionFlag.BetweenAreas] && !plugin.Condition[ConditionFlag.OccupiedInCutSceneEvent];
    private bool Unsheathed => uiState->WeaponState.IsUnsheathed;
    private bool IsEnabled = false;

    private float CurrentTilt = 0;
    private float TimeoutTime = 0;

    public static float InOutSine(float t) => (float)(Math.Cos(t * Math.PI) - 1) / -2;

    public CameraTilter(TiltedPlugin plugin)
    {
      this.plugin = plugin;
      gameMain = GameMain.Instance();
      cameraManager = CameraManager.Instance;
      configModule = ConfigModule.Instance();
      uiState = UIState.Instance();

      CurrentTilt = configModule->GetIntValue(ConfigOption.TiltOffset);
    }

    public void OnUpdate(Framework framework)
    {
      if (plugin.Configuration.MasterEnable)
      {
        if (EvaluateTriggersAndSetIsEnabled())
        {
          if (plugin.Configuration.EnableCameraDistanceTweaking)
          {
            TweakCameraDistance();
          }
        }

        UpdateCombatTimeoutTimer(framework);

        if (plugin.Configuration.EnableTweakingCameraTilt)
        {
          TweakCameraTilt(framework);
        }
      }
    }

    private void UpdateCombatTimeoutTimer(Framework framework)
    {
      if (InCombat)
      {
        TimeoutTime = plugin.Configuration.CombatTimeoutSeconds;
      }
      else if (TimeoutTime > 0)
      {
        TimeoutTime -= framework.UpdateDelta.Milliseconds / 1000f;
      }
    }

    private void TweakCameraTilt(Framework framework)
    {
      var TargetTilt = CurrentTilt;
      if (IsEnabled)
      {
        TargetTilt = plugin.Configuration.CameraTiltWhenEnabled;
      }
      else
      {
        TargetTilt = plugin.Configuration.CameraTiltWhenDisabled;
      }

      if (plugin.Configuration.EnableCameraTiltSmoothing)
      {
        if (CurrentTilt > TargetTilt)
        {
          CurrentTilt = Math.Clamp(CurrentTilt - 0.05f * framework.UpdateDelta.Milliseconds, TargetTilt, 100f);
          configModule->SetOption(ConfigOption.TiltOffset, (int)CurrentTilt);
        }
        else if (CurrentTilt < TargetTilt)
        {
          CurrentTilt = Math.Clamp(CurrentTilt + 0.05f * framework.UpdateDelta.Milliseconds, 0f, TargetTilt);
          configModule->SetOption(ConfigOption.TiltOffset, (int)CurrentTilt);
        }
      }
      else
      {
        if (CurrentTilt != TargetTilt)
        {
          CurrentTilt = TargetTilt;
          configModule->SetOption(ConfigOption.TiltOffset, (int)CurrentTilt);
        }
      }
    }

    private void TweakCameraDistance()
    {
      if (IsEnabled)
      {
        plugin.PrintDebug($"Tweaking Camera Distance => Enabled Distance: {plugin.Configuration.CameraDistanceWhenEnabled}");
        cameraManager->GetActiveCamera()->Distance = plugin.Configuration.CameraDistanceWhenEnabled;
      }
      else
      {
        plugin.PrintDebug($"Tweaking Camera Distance => Disabled Distance: {plugin.Configuration.CameraDistanceWhenDisabled}");
        cameraManager->GetActiveCamera()->Distance = plugin.Configuration.CameraDistanceWhenDisabled;
      }
    }

    private bool EvaluateTriggersAndSetIsEnabled()
    {
      var didChange = false;

      if (IsEnabled)
      {
        if (
          !plugin.Configuration.DebugForceEnabled
          && (!plugin.Configuration.EnableInDuty || !(plugin.Configuration.EnableInDuty && BoundByDuty))
          && (!plugin.Configuration.EnableUnsheathed || !(plugin.Configuration.EnableUnsheathed && Unsheathed))
          && (!plugin.Configuration.EnableMounted || !(plugin.Configuration.EnableMounted && IsMounted))
          && (!plugin.Configuration.EnableInCombat || !(plugin.Configuration.EnableInCombat && InCombat))
          && (!plugin.Configuration.EnableInCombat || TimeoutTime <= 0)
        )
        {
          plugin.PrintDebug($"Trigger: None => Disabled");
          IsEnabled = false;
        }

        if (!IsEnabled)
        {
          plugin.PrintDebug($"State changed => Disabled");
          didChange = true;
        }
      }
      else
      {
        if (plugin.Configuration.DebugForceEnabled)
        {
          plugin.PrintDebug($"Trigger: Force Enabled => Enabled");
          IsEnabled = true;
        }
        else if (plugin.Configuration.EnableInDuty && BoundByDuty)
        {
          plugin.PrintDebug($"Trigger: In Duty => Enabled");
          IsEnabled = true;
        }
        else if (plugin.Configuration.EnableUnsheathed && Unsheathed)
        {
          plugin.PrintDebug($"Trigger: Unsheathed => Enabled");
          IsEnabled = true;
        }
        else if (plugin.Configuration.EnableMounted && IsMounted)
        {
          plugin.PrintDebug($"Trigger: Is Mounted => Enabled");
          IsEnabled = true;
        }
        else if (plugin.Configuration.EnableInCombat && InCombat)
        {
          plugin.PrintDebug($"Trigger: In Combat => Enabled");
          IsEnabled = true;
        }

        if (IsEnabled)
        {
          plugin.PrintDebug($"State changed => Enabled");
          didChange = true;
        }
      }

      return didChange;
    }
  }
}
