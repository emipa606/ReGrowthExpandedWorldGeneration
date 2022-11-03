using System.Collections.Generic;
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
        Page_CreateWorldParams_Patch.ApplyChanges(parent);

        parent.factions = new List<FactionDef>();
        Page_CreateWorldParams_Patch.tmpWorldGenerationPreset.factionCounts.ForEach(factionDef =>
            parent.factions.Add(FactionDef.Named(factionDef)));
        Close();
    }
}