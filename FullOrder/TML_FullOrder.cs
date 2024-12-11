using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace TML_FullOrder;

[BepInPlugin(Guid, Name, Version)]
public class TmlFullOrder : BasePlugin
{
    public const string Guid = "Com.Bright.TML_FullOrder";
    public const string Name = "TML_FullOrder";
    public const string Version = "0.9.0";
    public const string Author = "Bright";
    public static Harmony Harmony = new(Name);
    public static ConfigEntry<bool> FullOrder;
    public static Dictionary<int, string> SpecialGuestOrder = new();

    public override void Load()
    {
        FullOrder = Config.Bind("Config", "FullOrder", true, "启用完整订单信息");
        Harmony.PatchAll(typeof(Hook));
    }
}