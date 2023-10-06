using System;
using Dalamud.Game.Config;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace Tilted
{
  public static class TiltedHelper
  {
    public static float GetTiltOffset()
    {
      Service.GameConfig.TryGet(UiControlOption.TiltOffset, out float tiltOffset);
      var converted = (tiltOffset - (-0.08f)) / (0.21f - (0.08f)) * (100f - 1f) + 1f;
      return converted;
    }

    public static void SetTiltOffset(float value)
    {
      var converted = (value - 1f) / (100f - 1f) * (0.21f - (-0.08f)) + (-0.08f);
      Service.PluginLog.Verbose("Setting TiltOffset to {converted}", converted);
      Service.GameConfig.Set(UiControlOption.TiltOffset, converted);
    }

    public static float GetActiveCameraDistance()
    {
      unsafe
      {
        return CameraManager.Instance()->GetActiveCamera()->Distance;
      }
    }

    public static void SetActiveCameraDistance(float value)
    {
      unsafe
      {
        Service.PluginLog.Verbose("Setting ActiveCameraDistance to {value}", value);
        CameraManager.Instance()->GetActiveCamera()->Distance = value;
      }
    }

    public static float GetWeaponAutoPutAwayTime()
    {
      Service.GameConfig.TryGet(UiConfigOption.WeaponAutoPutAwayTime, out float putAwayTime);
      return putAwayTime;
    }

    public static bool GetIsUnsheathed()
    {
      unsafe
      {
        return UIState.Instance()->WeaponState.IsUnsheathed;
      }
    }
  }
}
