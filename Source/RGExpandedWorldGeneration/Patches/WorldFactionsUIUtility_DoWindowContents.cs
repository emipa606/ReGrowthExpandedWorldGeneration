using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(WorldFactionsUIUtility), nameof(WorldFactionsUIUtility.DoWindowContents))]
public static class WorldFactionsUIUtility_DoWindowContents
{
    public const float LowerWidgetHeight = 210;

    public static void Prefix(ref Rect rect)
    {
        var modifier = 0;
        if (!RGExpandedWorldGenerationSettingsMod.settings.showPreview)
        {
            modifier = 85;
        }

        rect.y += 425 - modifier;
        rect.height = LowerWidgetHeight + modifier;
    }
}