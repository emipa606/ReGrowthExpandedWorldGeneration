using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(Page), "DoBottomButtons")]
public static class DoBottomButtons_Patch
{
    public static bool Prefix(Page __instance, Rect rect, string nextLabel = null, string midLabel = null,
        Action midAct = null, bool showNext = true, bool doNextOnKeypress = true)
    {
        if (__instance is not Page_CreateWorldParams createWorldParams)
        {
            return true;
        }

        Page_CreateWorldParams_Patch.DoBottomButtons(createWorldParams, rect, nextLabel, midLabel, midAct, showNext,
            doNextOnKeypress);
        return false;
    }
}