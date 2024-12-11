using System;
using System.Linq;
using System.Text;
using DEYU.UniversalUISystem;
using GameData.CoreLanguage.Collections;
using HarmonyLib;
using NightScene.GuestManagementUtility;
using NightScene.UI.GuestManagementUtility;
using NightScene.UI.HUDUtility;
using TMPro;
using UnityEngine.UI;

namespace TML_FullOrder;

public static class Hook
{
    public static string ArrayToString<T>(this T[] Array, Func<T, string> func)
    {
        return Array.Length != 0 ? string.Join(", ", Array.Select(func).ToArray()) : "Null";
    }

    [HarmonyPatch(typeof(GuestGroupController), nameof(GuestGroupController.PushToOrder))]
    [HarmonyPostfix]
    private static void PushToOrder(GuestGroupController __instance, GuestsManager.OrderBase __0)
    {
        if (!TmlFullOrder.FullOrder.Value || __0.Type != GuestsManager.OrderBase.OrderType.Special) return;
        var specialGuestsController = __instance.Cast<SpecialGuestsController>();
        var foodRequest = __0.foodRequest;
        var beverageRequest = __0.beverageRequest;
        int[] likeBevTags = specialGuestsController.LikeBevTags;
        int[] hateBevTags = specialGuestsController.HateBevTags;
        int[] likeFoodTags = specialGuestsController.LikeFoodTags;
        int[] hateFoodTags = specialGuestsController.HateFoodTags;
        var fund = specialGuestsController.GetFund;
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("还可以消费:" + fund);
        stringBuilder.AppendLine("需求食物Tag:" + foodRequest.GetFoodTag());
        stringBuilder.AppendLine("喜好食物Tags:" + likeFoodTags.ArrayToString(x => DataBaseLanguage.FoodTags[x]));
        if (hateFoodTags.Length != 0)
            stringBuilder.AppendLine("厌恶食物Tags:" + hateFoodTags.ArrayToString(x => DataBaseLanguage.FoodTags[x]));
        stringBuilder.AppendLine("需求酒水Tag:" + beverageRequest.GetBeverageTag());
        stringBuilder.AppendLine("喜好酒水Tags:" + likeBevTags.ArrayToString(x => DataBaseLanguage.BeverageTags[x]));
        if (hateBevTags.Length != 0)
            stringBuilder.AppendLine("厌恶酒水Tags:" + hateBevTags.ArrayToString(x => DataBaseLanguage.BeverageTags[x]));
        TmlFullOrder.SpecialGuestOrder[specialGuestsController.DeskCode] = stringBuilder.ToString();
    }

    [HarmonyPatch(typeof(WorkSceneThrowDeliverPanel), nameof(WorkSceneThrowDeliverPanel.DescribeCurrentOrder))]
    [HarmonyPostfix]
    private static void DescribeCurrentOrder(GuestsManager.OrderBase orderBase, Image servedFood, Image servedBev,
        UIElementCluster normalRequestUI, UIElementCluster specialRequestUI, TextMeshProUGUI guestName,
        TextMeshProUGUI deskCode, GuestGroupController guestGroupController)
    {
        if (!TmlFullOrder.FullOrder.Value || orderBase.Type != GuestsManager.OrderBase.OrderType.Special) return;
        specialRequestUI.allElements[3].transform.parent.gameObject.SetActive(false);
        specialRequestUI.allElements[4].GetComponent<TextMeshProUGUI>().text =
            TmlFullOrder.SpecialGuestOrder[orderBase.DeskCode];
    }

    [HarmonyPatch(typeof(WorkSceneServePannel), nameof(WorkSceneServePannel.OnPanelOpen))]
    [HarmonyPostfix]
    private static void OnPanelOpen(WorkSceneServePannel __instance)
    {
        if (!TmlFullOrder.FullOrder.Value ||
            __instance.operatingOrder.Type != GuestsManager.OrderBase.OrderType.Special) return;
        __instance.specialRequest.transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
        __instance.specialRequest.transform.GetChild(1).GetChild(0).GetChild(0)
            .GetComponent<TextMeshProUGUI>()
            .text = TmlFullOrder.SpecialGuestOrder[__instance.operatingOrder.DeskCode];
    }
}