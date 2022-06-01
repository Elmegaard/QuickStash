using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
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

        public static ConfigEntry<KeyCode> configKeybinding;
        public static ConfigEntry<float> configMaxDistance;

        private void InitConfig()
        {
            configKeybinding = Config.Bind("Client", "Keybinding", KeyCode.G, "The key to press to transfer items");
            configMaxDistance = Config.Bind("Server", "MaxDistance", 40.0f, "The max distance for transfering items");
        }

        public override void Load()
        {
            Logger = Log;
            InitConfig();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo(Harmony.GetAllPatchedMethods().Join());
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
