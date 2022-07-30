using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ImGuiNET;
using System;
using System.Numerics;

namespace Tilted
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private readonly Configuration configuration;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        // passing in the image here just for simplicity
        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawSettingsWindow();
        }

        public void DrawSettingsWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(320, 260), ImGuiCond.Always);
            if (ImGui.Begin("Tilted Settings", ref visible,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                // can't ref a property, so use a local copy
                var enabled = configuration.Enabled;
                if (ImGui.Checkbox("Master Enable", ref enabled))
                {
                    configuration.Enabled = enabled;
                    configuration.Save();
                }

                ImGui.NewLine();

                var enabledInCombat = configuration.EnabledInCombat;
                if (ImGui.Checkbox("Enabled in Combat", ref enabledInCombat))
                {
                    configuration.EnabledInCombat = enabledInCombat;
                    configuration.Save();
                }

                var smoothingInCombat = configuration.SmoothingInCombat;
                if (ImGui.Checkbox("Smoothing in Combat", ref smoothingInCombat))
                {
                    configuration.SmoothingInCombat = smoothingInCombat;
                    configuration.Save();
                }

                ImGui.NewLine();

                var enabledInInstance = configuration.EnabledInInstance;
                if (ImGui.Checkbox("Enabled in Duties", ref enabledInInstance))
                {
                    configuration.EnabledInInstance = enabledInInstance;
                    configuration.Save();
                }

                var smoothingInInstance = configuration.SmoothingInInstance;
                if (ImGui.Checkbox("Smoothing in Duties", ref smoothingInInstance))
                {
                    configuration.SmoothingInInstance = smoothingInInstance;
                    configuration.Save();
                }

                ImGui.NewLine();

                var outTilt = configuration.DisabledCameraTilt;
                if (ImGui.InputInt("Disabled Tilt", ref outTilt))
                {
                    configuration.DisabledCameraTilt = outTilt;
                    configuration.Save();
                }

                var inTilt = configuration.EnabledCameraTilt;
                if (ImGui.InputInt("Enabled Tilt", ref inTilt))
                {
                    configuration.EnabledCameraTilt = inTilt;
                    configuration.Save();
                }
            }
            ImGui.End();
        }
    }
}
