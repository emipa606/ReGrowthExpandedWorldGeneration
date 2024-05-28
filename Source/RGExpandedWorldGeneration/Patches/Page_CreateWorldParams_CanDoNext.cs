using HarmonyLib;
using RimWorld;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(Page_CreateWorldParams), nameof(Page_CreateWorldParams.CanDoNext))]
public static class Page_CreateWorldParams_CanDoNext
{
    public static void Prefix()
    {
        if (Page_CreateWorldParams_DoWindowContents.thread != null)
        {
            Page_CreateWorldParams_DoWindowContents.thread.Abort();
            Page_CreateWorldParams_DoWindowContents.thread.Join(1000);
            Page_CreateWorldParams_DoWindowContents.thread = null;
        }

        Page_CreateWorldParams_DoWindowContents.generatingWorld = false;
    }
}