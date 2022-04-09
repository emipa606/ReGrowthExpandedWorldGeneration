using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(WorldFactionsUIUtility), "DoWindowContents")]
public static class DoWindowContents_Patch
{
    public const float LowerWidgetHeight = 210;

    public static void Prefix(ref Rect rect)
    {
        rect.y += 425;
        rect.height = LowerWidgetHeight;
    }
}