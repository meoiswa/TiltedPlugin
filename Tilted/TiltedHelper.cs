using Dalamud.Game.Config;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace Tilted
{
  public static class TiltedHelper
  {
    public static float GetTiltOffset()
    {
      Service.GameConfig.TryGet(UiControlOption.TiltOffset, out float tiltOffset);
      var converted = (tiltOffset - (-0.08f)) / (0.21f - (-0.08f)) * 100f;
      return converted;
    }

    public static void SetTiltOffset(float value)
    {
      var converted = value / 100f * (0.21f - (-0.08f)) + (-0.08f);
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

    /**
     * Returns the current camera distance as interpolated by the game's built-in camera motion easing function.
     * Using this value can make camera motions set by the plugin feel more integrated to the camera motion built in to the game.
     */
    public static float GetActiveCameraDistanceInterpolated()
    {
      unsafe
      {
        return CameraManager.Instance()->GetActiveCamera()->InterpDistance;
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

    /**
     * Linearly interpolate two values by value t.
     */
    public static float Lerp(float start, float end, float t)
    {
      return (start * (1 - t)) + (end * t);
    }
  }
}
