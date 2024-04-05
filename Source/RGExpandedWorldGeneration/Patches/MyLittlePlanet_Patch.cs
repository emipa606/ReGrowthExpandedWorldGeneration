using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace RGExpandedWorldGeneration;

[HarmonyPatch]
public static class MyLittlePlanet_Patch
{
    private static MethodBase planetShapeGenerator_DoGenerate_Patch;

    public static bool Prepare()
    {
        planetShapeGenerator_DoGenerate_Patch =
            AccessTools.Method("WorldGenRules.RulesOverrider+PlanetShapeGenerator_DoGenerate_Patch:Prefix");
        return planetShapeGenerator_DoGenerate_Patch != null;
    }

    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method("RimWorld.Planet.PlanetShapeGenerator:DoGenerate");
    }

    [HarmonyPriority(int.MinValue)]
    public static void Prefix(ref int ___subdivisionsCount)
    {
        var type = AccessTools.TypeByName("WorldGenRules.RulesOverrider");
        var gameComp = Current.Game.components.First(x => x.GetType().Name.Contains("RulesOverrider"));
        ___subdivisionsCount = (int)AccessTools.Field(type, "subcount").GetValue(gameComp);
    }
}