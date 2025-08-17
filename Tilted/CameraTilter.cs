using System;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using Dalamud.Plugin.Services;

namespace Tilted
{
  public class CameraTilter
  {
    private readonly ConfigurationMKII configuration;

    private bool IsFlying => Service.Condition[ConditionFlag.InFlight];
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
      var (shouldBeEnabled, trigger) = GetEnabledStateAndTrigger();
      var didChange = IsEnabled != shouldBeEnabled;

      if (didChange)
      {
        IsEnabled = shouldBeEnabled;
        Service.PluginLog.Verbose($"State changed => {(IsEnabled ? "Enabled" : "Disabled")}. Trigger: {trigger}");
      }

      return didChange;
    }

    private (bool isEnabled, string triggerName) GetEnabledStateAndTrigger()
    {
      // Check for each enabling condition and return the corresponding trigger name.
      if (configuration.DebugForceEnabled)
      {
        return (true, "Force Enabled");
      }
      if (configuration.EnableInDuty && BoundByDuty)
      {
        return (true, "In Duty");
      }
      if (configuration.EnableUnsheathed && TiltedHelper.GetIsUnsheathed())
      {
        return (true, "Unsheathed");
      }
      if (configuration.EnableFlying && IsFlying)
      {
        return (true, "Is Flying");
      }
      if (configuration.EnableMounted && IsMounted)
      {
        return (true, "Is Mounted");
      }

      if (configuration.EnableInCombat)
      {
        // If currently in combat, the feature should be enabled.
        if (InCombat)
        {
          return (true, "In Combat");
        }
        // If not in combat but the timeout timer is still running, stay enabled.
        if (TimeoutTime > 0)
        {
          return (true, "Timeout Active");
        }
      }

      if (configuration.EnableZoomed && ZoomedIn)
      {
        return (true, "Zoomed In");
      }

      // If none of the enabling conditions are met, it should be disabled.
      return (false, "None");
    }
  }
}
