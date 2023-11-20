using System;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Logging;
using Dalamud.Plugin.Services;

namespace Tilted
{
  public class CameraTilter
  {
    private readonly ConfigurationMKII configuration;

    private bool IsMounted => Service.Condition[ConditionFlag.Mounted];
    private bool InCombat => Service.Condition[ConditionFlag.InCombat];
    private bool BoundByDuty => Service.Condition[ConditionFlag.BoundByDuty] && !Service.Condition[ConditionFlag.BetweenAreas] && !Service.Condition[ConditionFlag.OccupiedInCutSceneEvent];

    public bool ZoomedIn = false;

    private bool IsEnabled = false;

    private float CurrentTilt = 0;
    private float TimeoutTime = 0;

    public static float InOutSine(float t) => (float)(Math.Cos(t * Math.PI) - 1) / -2;

    public CameraTilter(ConfigurationMKII configuration)
    {
      this.configuration = configuration;

      CurrentTilt = TiltedHelper.GetTiltOffset();
    }

    public void OnUpdate(IFramework framework)
    {
      if (!configuration.MasterEnable)
      {
        return;
      }

      if (FFXIVClientStructs.FFXIV.Client.Game.GameMain.IsInGPose() && !configuration.EnableInGpose)
      {
        return;
      }

      UpdateIsZoomed();

      if (EvaluateTriggersAndSetIsEnabled())
      {
        if (configuration.EnableCameraDistanceTweaking && !configuration.EnableZoomed)
        {
          TweakCameraDistance();
        }
      }

      UpdateCombatTimeoutTimer(framework);

      if (configuration.EnableTweakingCameraTilt)
      {
        TweakCameraTilt(framework);
      }
    }

    private void UpdateIsZoomed()
    {
      var dist = TiltedHelper.GetActiveCameraDistance();

      if (dist < configuration.ZoomedTriggerDistance)
      {
        ZoomedIn = true;
      }
      else
      {
        ZoomedIn = false;
      }
    }

    private void UpdateCombatTimeoutTimer(IFramework framework)
    {
      if (InCombat)
      {
        TimeoutTime = configuration.CombatTimeoutSeconds;
      }
      else if (TimeoutTime > 0)
      {
        TimeoutTime -= framework.UpdateDelta.Milliseconds / 1000f;
      }
    }

    private void TweakCameraTilt(IFramework framework)
    {
      float targetTilt;

      if (configuration.EnableDistanceToTiltMapping)
      {
        float interpolatedDistance = TiltedHelper.GetActiveCameraDistanceInterpolated();
        float distanceRatio = Math.Clamp((interpolatedDistance - configuration.MinimumCameraDistance) / (configuration.MaximumCameraDistance - configuration.MinimumCameraDistance), 0f, 1f);
        
        targetTilt = Math.Clamp(TiltedHelper.Lerp(configuration.CameraTiltWhenDisabled, configuration.CameraTiltWhenEnabled, distanceRatio), 0f, 100f);
      }
      else
      {
        if (IsEnabled)
        {
          targetTilt = configuration.CameraTiltWhenEnabled;
        }
        else
        {
          targetTilt = configuration.CameraTiltWhenDisabled;
        }
      }

      if (configuration.EnableCameraTiltSmoothing)
      {
        if (CurrentTilt > targetTilt)
        {

          CurrentTilt = Math.Clamp(CurrentTilt - 0.05f * framework.UpdateDelta.Milliseconds, targetTilt, 100f);
          TiltedHelper.SetTiltOffset(CurrentTilt);
        }
        else if (CurrentTilt < targetTilt)
        {
          CurrentTilt = Math.Clamp(CurrentTilt + 0.05f * framework.UpdateDelta.Milliseconds, 0f, targetTilt);
          TiltedHelper.SetTiltOffset(CurrentTilt);
        }
      }
      else
      {
        if (CurrentTilt != targetTilt)
        {
          CurrentTilt = targetTilt;
          TiltedHelper.SetTiltOffset(CurrentTilt);
        }
      }
    }

    private void TweakCameraDistance()
    {
      if (IsEnabled)
      {
        Service.PluginLog.Verbose($"Tweaking Camera Distance => Enabled Distance: {configuration.CameraDistanceWhenEnabled}");
        TiltedHelper.SetActiveCameraDistance(configuration.CameraDistanceWhenEnabled);
      }
      else
      {
        Service.PluginLog.Verbose($"Tweaking Camera Distance => Disabled Distance: {configuration.CameraDistanceWhenDisabled}");
        TiltedHelper.SetActiveCameraDistance(configuration.CameraDistanceWhenDisabled);
      }
    }

    private bool EvaluateTriggersAndSetIsEnabled()
    {
      var didChange = false;

      if (IsEnabled)
      {
        if (
          !configuration.DebugForceEnabled
          && (!configuration.EnableInDuty || !(configuration.EnableInDuty && BoundByDuty))
          && (!configuration.EnableUnsheathed || !(configuration.EnableUnsheathed && TiltedHelper.GetIsUnsheathed()))
          && (!configuration.EnableMounted || !(configuration.EnableMounted && IsMounted))
          && (!configuration.EnableInCombat || !(configuration.EnableInCombat && InCombat))
          && (!configuration.EnableInCombat || TimeoutTime <= 0)
          && (!configuration.EnableZoomed || !(configuration.EnableZoomed && ZoomedIn))
        )
        {
          Service.PluginLog.Verbose($"Trigger: None => Disabled");
          IsEnabled = false;
        }

        if (!IsEnabled)
        {
          Service.PluginLog.Verbose($"State changed => Disabled");
          didChange = true;
        }
      }
      else
      {
        if (configuration.DebugForceEnabled)
        {
          Service.PluginLog.Verbose($"Trigger: Force Enabled => Enabled");
          IsEnabled = true;
        }
        else if (configuration.EnableInDuty && BoundByDuty)
        {
          Service.PluginLog.Verbose($"Trigger: In Duty => Enabled");
          IsEnabled = true;
        }
        else if (configuration.EnableUnsheathed && TiltedHelper.GetIsUnsheathed())
        {
          Service.PluginLog.Verbose($"Trigger: Unsheathed => Enabled");
          IsEnabled = true;
        }
        else if (configuration.EnableMounted && IsMounted)
        {
          Service.PluginLog.Verbose($"Trigger: Is Mounted => Enabled");
          IsEnabled = true;
        }
        else if (configuration.EnableInCombat && InCombat)
        {
          Service.PluginLog.Verbose($"Trigger: In Combat => Enabled");
          IsEnabled = true;
        }
        else if (configuration.EnableZoomed && ZoomedIn)
        {
          Service.PluginLog.Verbose($"Trigger: Zoomed In => Enabled");
          IsEnabled = true;
        }

        if (IsEnabled)
        {
          Service.PluginLog.Verbose($"State changed => Enabled");
          didChange = true;
        }
      }

      return didChange;
    }
  }
}
