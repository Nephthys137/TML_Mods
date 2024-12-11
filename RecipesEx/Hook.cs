using System;
using System.IO;
using System.Reflection;
using System.Text;
using DayScene;
using GameData.CoreLanguage.Collections;
using GameData.Profile;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;

namespace TML_RecipesEx;

public static class Hook
{
    public static List<string> ActiveDlcLabel = new();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameDataProfile), nameof(GameDataProfile.ActiveDLCLabel), MethodType.Getter)]
    private static void get_ActiveDLCLabel(MethodBase __originalMethod, GameDataProfile __instance,
        ref List<string> __result)
    {
        if (__result.Count + TmlRecipesEx.Mods.Count != ActiveDlcLabel.Count)
        {
            ActiveDlcLabel.Clear();
            ActiveDlcLabel.AddRange(__result.Cast<IEnumerable<string>>());
            foreach (var item in TmlRecipesEx.Mods.Keys) ActiveDlcLabel.Add(item);
        }

        __result = ActiveDlcLabel;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.Awake))]
    private static void AwakePostfix(SceneManager __instance)
    {
        foreach (var mod in TmlRecipesEx.Mods) TmlRecipesEx.AddRecipes(mod.Key, mod.Value.Recipelist);

        File.WriteAllText("./RecipeMods/FoodTags.txt",
            DataBaseLanguage.FoodTags.DictionaryToString());
        File.WriteAllText("./RecipeMods/Ingredients.txt",
            DataBaseLanguage.Ingredients.DictionaryToString(null, x => x.Name.ToString()));
    }

    public static string DictionaryToString<T, T2>(this Dictionary<T, T2> Dic, Func<T, string> func = null,
        Func<T2, string> func2 = null)
    {
        if (Dic.Count == 0) return "Null";
        var sb = new StringBuilder();
        foreach (var dic in Dic)
            sb.Append(func == null ? dic.Key.ToString() : func(dic.Key)).Append(':')
                .AppendLine(func2 == null ? dic.Value.ToString() : func2(dic.Value));
        return sb.ToString();
    }
}