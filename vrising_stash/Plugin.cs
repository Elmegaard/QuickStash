using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;


namespace vrising_stash
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public static ManualLogSource Logger;
        public static ConfigEntry<KeyCode> HotKey;

        public override void Load()
        {

            HotKey = Config.Bind("Controls", "QuickStash", KeyCode.G);

            Logger = Log;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo(Harmony.GetAllPatchedMethods().Join());
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded! QuickStash key is: {HotKey.Value}");
        }
    }
}
