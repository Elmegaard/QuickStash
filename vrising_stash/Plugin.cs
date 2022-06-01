using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace vrising_stash
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public static ManualLogSource Logger;

        public override void Load()
        {
            Logger = Log;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo(Harmony.GetAllPatchedMethods().Join());
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
