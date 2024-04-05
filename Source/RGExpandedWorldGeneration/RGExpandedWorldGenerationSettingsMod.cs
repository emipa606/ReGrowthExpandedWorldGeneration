using Verse;

namespace RGExpandedWorldGeneration;

internal class RGExpandedWorldGenerationSettingsMod : Mod
{
    public static RGExpandedWorldGenerationSettings settings;

    public RGExpandedWorldGenerationSettingsMod(ModContentPack content) : base(content)
    {
        settings = GetSettings<RGExpandedWorldGenerationSettings>();
    }
}