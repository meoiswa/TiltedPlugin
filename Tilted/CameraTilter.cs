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

        private float CurrentTilt = 0;
        private int TargetTilt = 0;

        public static float InOutSine(float t) => (float)(Math.Cos(t * Math.PI) - 1) / -2;

        public CameraTilter(Configuration configuration)
        {
            this.configuration = configuration;
            this.gameMain = GameMain.Instance();
            this.configModule = ConfigModule.Instance();

            CurrentTilt = configModule->GetIntValue(ConfigOption.TiltOffset);
            TargetTilt = configModule->GetIntValue(ConfigOption.TiltOffset);
        }

        public void OnUpdate(Framework framework)
        {
            if (configuration.Enabled)
            {
                if (configuration.SmoothingInCombat || (configuration.SmoothingInInstance && BoundByDuty))
                {
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
                        AdjustPeace();
                        BoundByDutyPending = false;
                    }
                }
                if (flag == ConditionFlag.OccupiedInCutSceneEvent && !value)
                {
                    if (BoundByDutyPending && BoundByDuty)
                    {
                        AdjustCombat();
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
                        AdjustCombat();
                    }
                    else
                    {
                        AdjustPeace();
                    }
                }
            }
        }

        public void AdjustCombat()
        {
            Adjust(configuration.EnabledCameraTilt);
        }

        public void AdjustPeace()
        {
            Adjust(configuration.DisabledCameraTilt);
        }

        public void Adjust(int tilt)
        {
            TargetTilt = tilt;
        }
    }
}
