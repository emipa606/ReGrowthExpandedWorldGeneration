using HarmonyLib;
using Verse;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(Rand), "EnsureStateStackEmpty")]
public static class EnsureStateStackEmpty_Patch
{
    public static bool Prefix()
    {
        if (Page_CreateWorldParams_Patch.generatingWorld)
        {
            return false;
        }

        return true;
    }
}