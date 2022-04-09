using System.Collections.Generic;
using Verse;

namespace RGExpandedWorldGeneration;

internal class RGExpandedWorldGenerationSettings : ModSettings
{
    public static WorldGenerationPreset curWorldGenerationPreset;

    public Dictionary<string, WorldGenerationPreset> presets;

    public RGExpandedWorldGenerationSettings()
    {
        presets = new Dictionary<string, WorldGenerationPreset>();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref presets, "presets", LookMode.Value, LookMode.Deep);
    }
}