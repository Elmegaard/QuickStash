using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Wetstone.API;

namespace vrising_stash
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("xyz.molenzwiebel.wetstone")]
    [Reloadable]
    public class Plugin : BasePlugin
    {
        public static ManualLogSource Logger;

        public static Keybinding configKeybinding;
        public static ConfigEntry<float> configMaxDistance;

        private void InitConfig()
        {
            configMaxDistance = Config.Bind("Server", "MaxDistance", 50.0f, "The max distance for transfering items");

            configKeybinding = KeybindManager.Register(new()
            {
                Id = "elmegaard.quickstash.deposit",
                Category = "QuickStash",
                Name = "Deposit",
                DefaultKeybinding = KeyCode.G,
            });
        }

        public override void Load()
        {
            Logger = Log;
            InitConfig();
            QuickStashClient.Reset();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        public override bool Unload()
        {
            Config.Clear();
            KeybindManager.Unregister(configKeybinding);
            Harmony.UnpatchAll();
            return true;
        }
    }
}
