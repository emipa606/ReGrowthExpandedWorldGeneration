using HarmonyLib;
using RimWorld.Planet;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(WITab_Planet), MethodType.Constructor)]
public static class WITab_Planet_Constructor
{
    public static void Postfix(ref WITab_Planet __instance)
    {
        if (__instance.size.y < 300)
        {
            __instance.size.y = 300;
        }
    }
}