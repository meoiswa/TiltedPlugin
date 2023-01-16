using System;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace Tilted
{
  public unsafe class CameraTilter
  {
    public Configuration configuration;

    public GameMain* gameMain;

    public ConfigModule* configModule;

    private bool BoundByDuty = false;
    private bool BoundByDutyPending = false;
    private bool IsEnabled = false;

    private float CurrentTilt = 0;
    private float TimeoutTime = 0;

    public static float InOutSine(float t) => (float)(Math.Cos(t * Math.PI) - 1) / -2;

    public CameraTilter(Configuration configuration)
    {
      this.configuration = configuration;
      this.gameMain = GameMain.Instance();
      this.configModule = ConfigModule.Instance();

      CurrentTilt = configModule->GetIntValue(ConfigOption.TiltOffset);
    }

    public void OnUpdate(Framework framework)
    {
      if (configuration.Enabled)
      {
        var TargetTilt = CurrentTilt;
        if (IsEnabled)
        {
          TargetTilt = configuration.EnabledCameraTilt;
        }
        else
        {
          TargetTilt = configuration.DisabledCameraTilt;
        }

        if (configuration.SmoothingInCombat || (configuration.SmoothingInInstance && BoundByDuty))
        {
          if (TimeoutTime > 0) {
            TimeoutTime -= framework.UpdateDelta.Milliseconds / 1000f;
            return;
          }

          if (CurrentTilt > TargetTilt)
          {
            CurrentTilt = Math.Clamp(CurrentTilt - 0.05f * framework.UpdateDelta.Milliseconds, TargetTilt, 100);
          }
          else if (CurrentTilt < TargetTilt)
          {
            CurrentTilt = Math.Clamp(CurrentTilt + 0.05f * framework.UpdateDelta.Milliseconds, 0, TargetTilt);
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
      if (!configuration.Enabled)
      {
        return;
      }

      if (configuration.EnabledInInstance)
      {
        if (flag == ConditionFlag.BoundByDuty)
        {
          BoundByDuty = value;
          BoundByDutyPending = true;
        }
        if (flag == ConditionFlag.BetweenAreas && !value)
        {
          if (BoundByDutyPending && !BoundByDuty)
          {
            DisableAdjust();
            BoundByDutyPending = false;
          }
        }
        if (flag == ConditionFlag.OccupiedInCutSceneEvent && !value)
        {
          if (BoundByDutyPending && BoundByDuty)
          {
            EnableAdjust();
            BoundByDutyPending = false;
          }
        }
      }

      if (configuration.EnabledInCombat)
      {
        if (flag == ConditionFlag.InCombat)
        {
          if (BoundByDuty && configuration.EnabledInInstance)
          {
            return;
          }

          if (value)
          {
            TimeoutTime = 0;
            EnableAdjust();
          }
          else
          {
            TimeoutTime = configuration.CombatTimeoutSeconds;
            DisableAdjust();
          }
        }
      }
    }

    public void EnableAdjust()
    {
      IsEnabled = true;
    }

    public void DisableAdjust()
    {
      IsEnabled = false;
    }
  }
}
