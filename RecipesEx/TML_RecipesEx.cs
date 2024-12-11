using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using GameData.Core.Collections;
using GameData.CoreLanguage;
using GameData.CoreLanguage.Collections;
using GameData.RunTime.Common;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace TML_RecipesEx;

[BepInPlugin(Guid, Name, Version)]
public class TmlRecipesEx : BasePlugin
{
    public const string Guid = "Com.Bright.TML_RecipesEx";
    public const string Name = "TML_RecipesEx";
    public const string Version = "0.5.0";
    public const string Author = "Bright";
    public static Harmony Harmony = new(Name);
    public static Dictionary<string, RecipeMod> Mods = [];
    public static string RecipeModsPath;

    public static bool Inited;

    public override void Load()
    {
        RecipeModsPath = Paths.GameRootPath + "/RecipeMods";
        Mods["TML_RecipesEx.Test"] = new RecipeMod
        {
            ModID = "TML_RecipesEx.Test",
            Recipelist =
            [
                new RecipeFood
                {
                    Name = "Test",
                    Description = "Test",
                    Money = int.MaxValue,
                    CookTime = 0f,
                    CookerType = Cooker.CookerType.Pot,
                    Tags = [],
                    BanTags = [],
                    ID = GetMD5("TML_RecipesEx.Test-Test"),
                    Ingredients = []
                }
            ]
        };
        File.WriteAllText("./Test.json", JsonConvert.SerializeObject(Mods["TML_RecipesEx.Test"], Formatting.Indented));
        var directoryInfo = new DirectoryInfo(RecipeModsPath);
        if (!directoryInfo.Exists) directoryInfo.Create();

        InitRecipeMods(directoryInfo);
        Harmony.PatchAll(typeof(Hook));
    }

    public static void AddRecipes(string ModID, List<RecipeFood> list)
    {
        if (!Inited)
        {
            var modinitlog = new StringBuilder();
            modinitlog.AppendLine("[TML_RecipesEx]开始加载" + ModID);
            modinitlog.AppendLine("//////////////////////////");
            try
            {
                foreach (var recipeFood in list)
                {
                    var hasfood = DataBaseLanguage.Foods.TryAdd(recipeFood.ID,
                        new ObjectLanguageBase(recipeFood.Name, recipeFood.Description, recipeFood.FoodSprite));
                    var hassell = DataBaseCore.Foods.TryAdd(recipeFood.ID,
                        new Sellable(recipeFood.ID, recipeFood.Money, recipeFood.Level, recipeFood.Tags,
                            recipeFood.BanTags, Sellable.SellableType.Food,
                            new Il2CppSystem.Collections.Generic.List<int>(), false));
                    var hasrec = DataBaseCore.Recipes.TryAdd(recipeFood.ID,
                        new Recipe(recipeFood.ID, recipeFood.ID, recipeFood.CookerType, recipeFood.CookTime,
                            recipeFood.Ingredients));
                    var hasdlcmap = DataBaseCore.RecipesMapping.TryAdd(recipeFood.ID, ModID) &&
                                    DataBaseCore.FoodsMapping.TryAdd(recipeFood.ID, ModID);

                    if (!hasfood || !hassell || !hasrec || !hasdlcmap)
                        modinitlog.AppendLine($"Error!“{ModID}”模组的菜单“{recipeFood.Name}”加载出现问题!原因是ID冲突");
                    else
                        modinitlog.AppendLine($"“{ModID}”模组的菜单“{recipeFood.Name}”已成功加载");
                }
            }
            catch (Exception ex)
            {
                modinitlog.AppendLine(ex.ToString());
            }

            modinitlog.AppendLine("//////////////////////////");
            modinitlog.AppendLine("[TML_RecipesEx]已成功加载" + ModID);
            Debug.Log(modinitlog.ToString());
            Inited = true;
        }

        foreach (var recipeFood in list)
        {
            if (!LoadInUnLockCheak(recipeFood)) continue;
            var nothas = true;
            for (var i = RunTimeStorage.Recipes.Count - 1; i > -1; i--)
            {
                if (RunTimeStorage.Recipes[i] != recipeFood.ID) continue;
                nothas = false;
                break;
            }

            if (nothas) RunTimeStorage.Recipes.Add(recipeFood.ID);
        }
    }

    private static bool LoadInUnLockCheak(RecipeFood recipeFood)
    {
        return recipeFood.UnLockType switch
        {
            UnLockType.LoadIn => true,
            UnLockType.GameFlag => RunTimeScheduler.GetAllFinished().Exists((Func<string, bool>)CheakGameFlag),
            _ => false
        };

        bool CheakGameFlag(string x)
        {
            return recipeFood.UnLockParameters.Split(';').Any(x.Equals);
        }
    }

    private static void InitRecipeMods(DirectoryInfo dir)
    {
        foreach (var d in dir.GetDirectories()) InitRecipeMods(d);
        foreach (var fileInfo in dir.GetFiles())
        {
            if (!fileInfo.Name.EndsWith(".json")) continue;
            var recipeMod = JsonConvert.DeserializeObject<RecipeMod>(File.ReadAllText(fileInfo.FullName));
            foreach (var recipeFood in recipeMod.Recipelist)
            {
                recipeFood.FoodSpritePath = fileInfo.DirectoryName + recipeFood.FoodSpritePath;
                recipeFood.ID = GetMD5($"{recipeMod.ModID}-{recipeFood.Name}");
            }

            if (!Mods.TryGetValue(recipeMod.ModID, out var mod))
                Mods[recipeMod.ModID] = recipeMod;
            else
                mod.Recipelist.AddRange(recipeMod.Recipelist);
        }
    }

    // I can't do it : get uniquely determined int
    public static int GetMD5(string input)
    {
        using var md5 = MD5.Create();
        var data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToInt32(data);
    }

    public static Sprite GetArtWork(string FilePath)
    {
        if (!File.Exists(FilePath)) return ObjectLanguageBase.defaultNull;

        var texture2D = new Texture2D(2, 2)
        {
            filterMode = FilterMode.Point
        };
        ImageConversion.LoadImage(texture2D, File.ReadAllBytes(FilePath));
        texture2D.filterMode = FilterMode.Point;
        var value = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height),
            new Vector2(0.5f, 0.5f), 48f * (texture2D.height / 50f));
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(FilePath);
        value.name = fileNameWithoutExtension;
        texture2D.name = fileNameWithoutExtension;
        return value;
    }
}