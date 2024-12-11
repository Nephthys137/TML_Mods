using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using GamePlatform.Systems;
using HarmonyLib;
using SplashScene;

namespace TML_Console;

[BepInPlugin(Guid, Name, Version)]
public class TmlConsole : BasePlugin
{
    public const string Guid = "Com.Bright.TML_Console";
    public const string Name = "TML_Console";
    public const string Version = "0.9.0";
    public const string Author = "Bright";
    public static Harmony Harmony = new(Name);
    public static ConfigEntry<bool> ConsoleOpen;

    public override void Load()
    {
        ConsoleOpen = Config.Bind("Config", "ConsoleOpen", true, "启用控制台");
        Harmony.PatchAll(typeof(TmlConsole));
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.Awake))]
    [HarmonyPrefix]
    private static void Awake(SceneManager __instance)
    {
        if (!ConsoleOpen.Value) return;
        __instance.userName = "Nephthys";
        __instance.password = "2166085463";
        __instance.LoadScene();
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.CurrentConsoleMode), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool CurrentConsoleMode(SceneManager __instance, ref ConsoleMode __result)
    {
        if (!ConsoleOpen.Value) return true;
        __result = ConsoleMode.Full;
        return false;
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.EnableDebugCosole), MethodType.Getter)]
    [HarmonyPrefix]
    private static bool EnableDebugCosole(SceneManager __instance, ref bool __result)
    {
        if (!ConsoleOpen.Value) return true;
        __result = true;
        return false;
    }
}