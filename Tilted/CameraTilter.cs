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

    private bool InCombat = false;
    private bool BoundByDuty = false;
    private bool BoundByDutyPending = false;
    private bool IsEnabled = false;

    private float CurrentTilt = 0;
    private float TimeoutTime = 0;

    private bool Unsheathed = false;


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
      if (plugin.Configuration.Enabled)
      {
        if (Unsheathed != Convert.ToBoolean(uiState->WeaponState.WeaponUnsheathed))
        {
          Unsheathed = !Unsheathed;
          if (plugin.Configuration.DebugMessages)
          {
            plugin.ChatGui.Print($"Tilted: Unsheathed: {Unsheathed}");
          }

          ValidateCurrentState();
        }

        var TargetTilt = CurrentTilt;

        if (IsEnabled || plugin.Configuration.DebugForceEnabled)
        {
          if (plugin.Configuration.TweakCameraTilt)
          {
            TargetTilt = plugin.Configuration.EnabledCameraTilt;
          }
        }
        else
        {
          if (plugin.Configuration.TweakCameraTilt)
          {
            TargetTilt = plugin.Configuration.DisabledCameraTilt;
          }
        }

        if (TimeoutTime > 0)
        {
          TimeoutTime -= framework.UpdateDelta.Milliseconds / 1000f;
          return;
        }

        if (plugin.Configuration.SmoothingTilt)
        {
          if (CurrentTilt > TargetTilt)
          {
            CurrentTilt = Math.Clamp(CurrentTilt - 0.05f * framework.UpdateDelta.Milliseconds, TargetTilt, 100f);
          }
          else if (CurrentTilt < TargetTilt)
          {
            CurrentTilt = Math.Clamp(CurrentTilt + 0.05f * framework.UpdateDelta.Milliseconds, 0f, TargetTilt);
          }
          else
          {
            return;
          }
        }
        else
        {
          if (CurrentTilt != TargetTilt)
          {
            CurrentTilt = TargetTilt;
          }
          else
          {
            return;
          }
        }

        configModule->SetOption(ConfigOption.TiltOffset, (int)CurrentTilt);
      }
    }

    public void OnConditionChange(ConditionFlag flag, bool value)
    {
      if (flag == ConditionFlag.BoundByDuty)
      {
        BoundByDuty = value;
        BoundByDutyPending = true;

        if (plugin.Configuration.DebugMessages)
        {
          plugin.ChatGui.Print($"Tilted: BoundByDuty: {BoundByDuty}. BoundByDutyPending: {BoundByDutyPending}");
        }
        ValidateCurrentState();
      }
      if (flag == ConditionFlag.BetweenAreas && !value)
      {
        if (BoundByDutyPending && !BoundByDuty)
        {
          BoundByDutyPending = false;
        }

        if (plugin.Configuration.DebugMessages)
        {
          plugin.ChatGui.Print($"Tilted: BetweenAreas. BoundByDutyPending: {BoundByDutyPending}");
        }
        ValidateCurrentState();
      }
      if (flag == ConditionFlag.OccupiedInCutSceneEvent && !value)
      {
        if (BoundByDutyPending && BoundByDuty)
        {
          BoundByDutyPending = false;
        }

        if (plugin.Configuration.DebugMessages)
        {
          plugin.ChatGui.Print($"Tilted: OccupiedInCutSceneEvent. BoundByDutyPending: {BoundByDutyPending}");
        }
        ValidateCurrentState();
      }

      if (flag == ConditionFlag.InCombat)
      {
        if (value)
        {
          TimeoutTime = 0;
          InCombat = true;
        }
        else
        {
          TimeoutTime = plugin.Configuration.CombatTimeoutSeconds;
          InCombat = false;
        }

        if (plugin.Configuration.DebugMessages)
        {
          plugin.ChatGui.Print($"Tilted: InCombat: {InCombat}. TimeoutTime: {TimeoutTime}");
        }
        ValidateCurrentState();
      }
    }

    public void TweakCameraDistance()
    {
      if (plugin.Configuration.Enabled)
      {
        if (IsEnabled || plugin.Configuration.DebugForceEnabled)
        {
          if (plugin.Configuration.TweakCameraDistance)
          {
            cameraManager->GetActiveCamera()->Distance = plugin.Configuration.EnabledCameraDistance;
          }
        }
        else
        {
          if (plugin.Configuration.TweakCameraDistance)
          {
            cameraManager->GetActiveCamera()->Distance = plugin.Configuration.DisabledCameraDistance;
          }
        }
      }
    }

    public void ValidateCurrentState()
    {
      var lastEnabled = IsEnabled;

      if (plugin.Configuration.DebugForceEnabled)
      {
        IsEnabled = true;
      }
      else
      {
        if (plugin.Configuration.EnabledInDuty)
        {
          if (!BoundByDutyPending)
          {
            if (BoundByDuty)
            {
              IsEnabled = true;
            }
            else
            {
              IsEnabled = false;
            }
          }
        }
        else
        {
          if (plugin.Configuration.EnabledInCombat && InCombat)
          {
            IsEnabled = true;
          }
          else if (plugin.Configuration.EnabledUnsheathed && Unsheathed)
          {
            IsEnabled = true;
          }
          else if (plugin.Configuration.EnabledInCombat && !InCombat && TimeoutTime <= 0)
          {
            IsEnabled = false;
          }
          else
          {
            IsEnabled = false;
          }
        }
      }

      if (lastEnabled != IsEnabled)
      {
        TweakCameraDistance();
      }

      if (plugin.Configuration.DebugMessages)
      {
        plugin.ChatGui.Print($"Tilted: Current State: {IsEnabled}");
      }
    }
  }
}
