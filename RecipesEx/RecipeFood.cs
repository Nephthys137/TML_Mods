using System.Collections.Generic;
using GameData.Core.Collections;
using GameData.CoreLanguage;
using Newtonsoft.Json;
using UnityEngine;

namespace TML_RecipesEx;

[JsonObject(MemberSerialization.OptOut)]
public class RecipeFood
{
    private Sprite _foodsprite;

    public int[] BanTags;

    public Cooker.CookerType CookerType;

    public float CookTime;

    public string Description;

    public string FoodSpritePath;
    internal int ID;

    public int[] Ingredients;

    public int Level;
    public int Money;

    public string Name;

    public int[] Tags;
    public string UnLockParameters;
    public UnLockType UnLockType;

    internal Sprite FoodSprite
    {
        get
        {
            if (!_foodsprite) _foodsprite = TmlRecipesEx.GetArtWork(FoodSpritePath);
            return _foodsprite;
        }
        set => _foodsprite = !value ? value : ObjectLanguageBase.defaultNull;
    }

    public override string ToString()
    {
        return Name;
    }
}

[JsonObject(MemberSerialization.OptOut)]
public class RecipeMod
{
    public string ModID;

    public List<RecipeFood> Recipelist;

    public override string ToString()
    {
        return ModID;
    }
}

public enum UnLockType
{
    LoadIn,
    GameFlag,
    SpecialGuest,
    Shop,
    Never
}