using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(WorldFactionsUIUtility), nameof(WorldFactionsUIUtility.DoRow))]
public static class DoRow_Patch
{
    public static void Prefix(ref Rect rect)
    {
        rect.width -= 20;
    }
}