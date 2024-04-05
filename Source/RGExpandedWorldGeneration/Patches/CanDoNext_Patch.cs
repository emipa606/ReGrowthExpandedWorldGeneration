using HarmonyLib;
using RimWorld;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(Page_CreateWorldParams), nameof(Page_CreateWorldParams.CanDoNext))]
public static class CanDoNext_Patch
{
    public static void Prefix()
    {
        if (Page_CreateWorldParams_Patch.thread != null)
        {
            Page_CreateWorldParams_Patch.thread.Abort();
            Page_CreateWorldParams_Patch.thread.Join(1000);
            Page_CreateWorldParams_Patch.thread = null;
        }

        Page_CreateWorldParams_Patch.generatingWorld = false;
    }
}