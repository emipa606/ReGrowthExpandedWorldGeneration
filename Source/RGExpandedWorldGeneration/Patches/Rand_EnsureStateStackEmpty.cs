using HarmonyLib;
using Verse;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(Rand), nameof(Rand.EnsureStateStackEmpty))]
public static class Rand_EnsureStateStackEmpty
{
    public static bool Prefix()
    {
        return !Page_CreateWorldParams_DoWindowContents.generatingWorld;
    }
}