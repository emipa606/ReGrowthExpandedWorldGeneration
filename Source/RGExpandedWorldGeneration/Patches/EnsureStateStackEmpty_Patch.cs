using HarmonyLib;
using Verse;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(Rand), "EnsureStateStackEmpty")]
public static class EnsureStateStackEmpty_Patch
{
    public static bool Prefix()
    {
        return !Page_CreateWorldParams_Patch.generatingWorld;
    }
}