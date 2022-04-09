using RimWorld;
using Verse;

namespace RGExpandedWorldGeneration;

public class Dialog_PresetList_Load : Dialog_PresetList
{
    public Dialog_PresetList_Load(Page_CreateWorldParams parent) : base(parent)
    {
        interactButLabel = "LoadGameButton".Translate();
    }

    protected override void DoPresetInteraction(string name)
    {
        Page_CreateWorldParams_Patch.tmpWorldGenerationPreset =
            RGExpandedWorldGenerationSettingsMod.settings.presets[name];
        Log.Message("RGExpandedWorldGenerationSettingsMod.settings.presets[name]: " +
                    RGExpandedWorldGenerationSettingsMod.settings.presets[name] + " - " + name + " - " +
                    RGExpandedWorldGenerationSettingsMod.settings.presets[name].seaLevel);
        Page_CreateWorldParams_Patch.ApplyChanges(parent);
        Close();
    }
}