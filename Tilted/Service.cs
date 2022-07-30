using System.Diagnostics.CodeAnalysis;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.IoC;
using Dalamud.Plugin;
using XivCommon;

namespace Tilted
{
    public class Service
    {
#pragma warning disable CS8618
        [PluginService] public static Condition Condition { get; private set; }
        [PluginService] public static Framework Framework { get; private set; }
        public static XivCommonBase XivCommon { get; private set; } = new XivCommonBase();
#pragma warning restore CS8618
    }
}