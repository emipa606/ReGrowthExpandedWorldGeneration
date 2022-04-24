﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Profile;

namespace RGExpandedWorldGeneration;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(Page_CreateWorldParams), "DoWindowContents")]
public static class Page_CreateWorldParams_Patch
{
    public const int WorldCameraHeight = 315;
    public const int WorldCameraWidth = 315;

    private static readonly Color BackgroundColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 15);
    private static readonly Texture2D GeneratePreview = ContentFinder<Texture2D>.Get("UI/GeneratePreview");
    private static readonly Texture2D Visible = ContentFinder<Texture2D>.Get("UI/Visible");
    private static readonly Texture2D InVisible = ContentFinder<Texture2D>.Get("UI/InVisible");

    public static WorldGenerationPreset tmpWorldGenerationPreset;

    public static Vector2 scrollPosition;

    public static bool dirty;

    public static Texture2D worldPreview;

    public static bool isActive;

    public static bool hidePreview;

    private static World threadedWorld;

    public static Thread thread;

    public static int updatePreviewCounter;

    private static float texSpinAngle;

    private static readonly HashSet<WorldGenStepDef> worldGenStepDefs = new HashSet<WorldGenStepDef>
    {
        DefDatabase<WorldGenStepDef>.GetNamed("Components"),
        DefDatabase<WorldGenStepDef>.GetNamed("Terrain"),
        DefDatabase<WorldGenStepDef>.GetNamed("Lakes"),
        DefDatabase<WorldGenStepDef>.GetNamed("Rivers"),
        DefDatabase<WorldGenStepDef>.GetNamed("AncientSites"),
        DefDatabase<WorldGenStepDef>.GetNamed("AncientRoads"),
        DefDatabase<WorldGenStepDef>.GetNamed("Roads")
    };

    public static bool generatingWorld;

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var planetCoverage = AccessTools.Field(typeof(Page_CreateWorldParams), "planetCoverage");
        var doGlobeCoverageSliderMethod =
            AccessTools.Method(typeof(Page_CreateWorldParams_Patch), "DoGlobeCoverageSlider");
        var doGuiMethod = AccessTools.Method(typeof(Page_CreateWorldParams_Patch), "DoGui");
        var endGroupMethod = AccessTools.Method(typeof(Widgets), "EndGroup");
        var codes = instructions.ToList();
        var found = false;

        for (var i = 0; i < codes.Count; i++)
        {
            var code = codes[i];

            if (codes[i].opcode == OpCodes.Ldloc_S && codes[i].operand is LocalBuilder { LocalIndex: 9 } &&
                i + 2 < codes.Count && codes[i + 2].LoadsField(planetCoverage))
            {
                var i1 = i;
                i += codes.FirstIndexOf(x =>
                    x.Calls(AccessTools.Method(typeof(WindowStack), "Add")) && codes.IndexOf(x) > i1) - i;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldloc_S, 9);
                yield return new CodeInstruction(OpCodes.Call, doGlobeCoverageSliderMethod);
            }
            else
            {
                yield return code;
            }

            if (found || i + 1 < codes.Count && !codes[i + 1].Calls(endGroupMethod))
            {
                continue;
            }

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldloca_S, 6);
            yield return new CodeInstruction(OpCodes.Ldloc_S, 7);
            yield return new CodeInstruction(OpCodes.Call, doGuiMethod);
            found = true;
        }
    }

    public static void DoBottomButtons(Page_CreateWorldParams window, Rect rect, string nextLabel = null,
        string midLabel = null, Action midAct = null, bool showNext = true, bool doNextOnKeypress = true)
    {
        var y = rect.y + rect.height - 38f;
        Text.Font = GameFont.Small;
        string label = "Back".Translate();
        var buttonSizeField =
            (Vector2)AccessTools.Field(typeof(Page_CreateWorldParams), "BottomButSize").GetValue(window);
        var canDoBackMethod = AccessTools.Method(typeof(Page_CreateWorldParams), "CanDoBack");
        var doBackMethod = AccessTools.Method(typeof(Page_CreateWorldParams), "DoBack");
        var canDoNextMethod = AccessTools.Method(typeof(Page_CreateWorldParams), "CanDoNext");
        var doNextMethod = AccessTools.Method(typeof(Page_CreateWorldParams), "DoNext");
        var backRect = new Rect(rect.x, y, buttonSizeField.x, buttonSizeField.y);
        if ((Widgets.ButtonText(backRect, label)
             || KeyBindingDefOf.Cancel.KeyDownEvent) && (bool)canDoBackMethod.Invoke(window, new object[] { }))
        {
            doBackMethod.Invoke(window, new object[] { });
        }

        if (showNext)
        {
            if (nextLabel.NullOrEmpty())
            {
                nextLabel = "Next".Translate();
            }

            var rect2 = new Rect(rect.x + rect.width - buttonSizeField.x, y, buttonSizeField.x, buttonSizeField.y);
            if ((Widgets.ButtonText(rect2, nextLabel) || doNextOnKeypress && KeyBindingDefOf.Accept.KeyDownEvent) &&
                (bool)canDoNextMethod.Invoke(window, new object[] { }))
            {
                doNextMethod.Invoke(window, new object[] { });
            }

            UIHighlighter.HighlightOpportunity(rect2, "NextPage");
        }

        var savePresetRect = new Rect(backRect.xMax + 100, y, buttonSizeField.x, buttonSizeField.y);
        string labelSavePreset = "RG.SavePreset".Translate();
        if (Widgets.ButtonText(savePresetRect, labelSavePreset))
        {
            var saveWindow = new Dialog_PresetList_Save(window);
            Find.WindowStack.Add(saveWindow);
        }

        var loadPresetRect = new Rect(savePresetRect.xMax + 15, y, buttonSizeField.x, buttonSizeField.y);
        string labelLoadPreset = "RG.LoadPreset".Translate();
        if (Widgets.ButtonText(loadPresetRect, labelLoadPreset))
        {
            var loadWindow = new Dialog_PresetList_Load(window);
            Find.WindowStack.Add(loadWindow);
        }

        var midActRect = new Rect(loadPresetRect.xMax + 15, y, buttonSizeField.x, buttonSizeField.y);
        if (midAct != null && Widgets.ButtonText(midActRect, midLabel))
        {
            midAct();
        }
    }

    private static void Postfix(Page_CreateWorldParams __instance)
    {
        DoWorldPreviewArea(__instance);
    }

    private static void DoGlobeCoverageSlider(Page_CreateWorldParams window, Rect rect)
    {
        var planetCoverage =
            (float)AccessTools.Field(typeof(Page_CreateWorldParams), "planetCoverage").GetValue(window);
        var value = (double)Widgets.HorizontalSlider(rect, planetCoverage, 0.05f, 1, false,
            (planetCoverage * 100) + "%", "RG.Small".Translate(), "RG.Large".Translate()) * 100;
        AccessTools.Field(typeof(Page_CreateWorldParams), "planetCoverage")
            .SetValue(window, (float)Math.Round(value / 5) * 5 / 100);
    }

    private static void DoGui(Page_CreateWorldParams window, ref float num, float width2)
    {
        isActive = true;
        UpdateCurPreset(window);
        DoSlider(0, ref num, width2, "RG.RiverDensity".Translate(), ref tmpWorldGenerationPreset.riverDensity,
            "None".Translate());
        DoSlider(0, ref num, width2, "RG.MountainDensity".Translate(), ref tmpWorldGenerationPreset.mountainDensity,
            "None".Translate());
        DoSlider(0, ref num, width2, "RG.SeaLevel".Translate(), ref tmpWorldGenerationPreset.seaLevel,
            "None".Translate());

        Rect labelRect;
        if (!ModCompat.MyLittlePlanetActive)
        {
            num += 40f;
            labelRect = new Rect(0, num, 200f, 30f);
            var slider = new Rect(labelRect.xMax, num, width2, 30f);
            Widgets.Label(labelRect, "RG.AxialTilt".Translate());
            tmpWorldGenerationPreset.axialTilt = (AxialTilt)Mathf.RoundToInt(Widgets.HorizontalSlider(slider,
                (float)tmpWorldGenerationPreset.axialTilt, 0f, AxialTiltUtility.EnumValuesCount - 1, true,
                "PlanetRainfall_Normal".Translate(), "PlanetRainfall_Low".Translate(),
                "PlanetRainfall_High".Translate(), 1f));
        }

        if (hidePreview)
        {
            DoSlider(0, ref num, width2, "RG.AncientRoadDensity".Translate(),
                ref tmpWorldGenerationPreset.ancientRoadDensity, "None".Translate());
            DoSlider(0, ref num, width2, "RG.FactionRoadDensity".Translate(),
                ref tmpWorldGenerationPreset.factionRoadDensity, "None".Translate());
        }
        else
        {
            labelRect = new Rect(0f, num + 64, 80, 30);
            Widgets.Label(labelRect, "RG.Biomes".Translate());
            var outRect = new Rect(labelRect.x, labelRect.yMax - 3, width2 + 195,
                DoWindowContents_Patch.LowerWidgetHeight - 50);
            var viewRect = new Rect(outRect.x, outRect.y, outRect.width - 16f,
                (DefDatabase<BiomeDef>.DefCount * 90) + 10);
            var rect3 = new Rect(outRect.xMax - 200f - 16f, labelRect.y, 200f, Text.LineHeight);


            Widgets.DrawBoxSolid(new Rect(outRect.x, outRect.y, outRect.width - 16f, outRect.height), BackgroundColor);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            num = outRect.y + 15;
            foreach (var biomeDef in DefDatabase<BiomeDef>.AllDefs.OrderBy(x => x.label ?? x.defName))
            {
                DoBiomeSliders(biomeDef, 10, ref num, biomeDef.label?.CapitalizeFirst() ?? biomeDef.defName);
            }

            Widgets.EndScrollView();
            if (tmpWorldGenerationPreset.biomeCommonalities.Any(x => x.Value != 10) ||
                tmpWorldGenerationPreset.biomeScoreOffsets.Any(y => y.Value != 0))
            {
                if (Widgets.ButtonText(rect3, "ResetFactionsToDefault".Translate()))
                {
                    tmpWorldGenerationPreset.ResetBiomeCommonalities();
                    tmpWorldGenerationPreset.ResetBiomeScoreOffsets();
                }
            }
        }

        if (RGExpandedWorldGenerationSettings.curWorldGenerationPreset is null)
        {
            RGExpandedWorldGenerationSettings.curWorldGenerationPreset = tmpWorldGenerationPreset.MakeCopy();
        }
        else if (RGExpandedWorldGenerationSettings.curWorldGenerationPreset.IsDifferentFrom(tmpWorldGenerationPreset))
        {
            RGExpandedWorldGenerationSettings.curWorldGenerationPreset = tmpWorldGenerationPreset.MakeCopy();
            updatePreviewCounter = 60;
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }

        if (thread is null)
        {
            if (updatePreviewCounter == 0)
            {
                StartRefreshWorldPreview(window);
            }
        }

        if (updatePreviewCounter > -2)
        {
            updatePreviewCounter--;
        }
    }

    private static void DoWorldPreviewArea(Page_CreateWorldParams window)
    {
        var previewAreaRect = new Rect(545, 10, WorldCameraHeight, WorldCameraWidth);
        Rect generateButtonRect;
        //if (worldPreview is null)
        //{
        //    generateButtonRect = new Rect(previewAreaRect.center.x - 12, previewAreaRect.center.y - 12, 35, 35);
        //    Text.Font = GameFont.Medium;
        //    var textSize = Text.CalcSize("RG.GeneratePreview".Translate());
        //    Widgets.Label(
        //        new Rect(generateButtonRect.center.x - (textSize.x / 2), generateButtonRect.yMax, textSize.x,
        //            textSize.y), "RG.GeneratePreview".Translate());
        //    Text.Font = GameFont.Small;
        //}
        //else
        //{
        generateButtonRect = new Rect(previewAreaRect.xMax - 35, previewAreaRect.y, 35, 35);
        //}

        var hideButtonRect = generateButtonRect;
        hideButtonRect.x += generateButtonRect.width * 1.1f;
        DrawHidePreviewButton(window, hideButtonRect);
        Rect labelRect;
        if (hidePreview)
        {
            labelRect = new Rect(previewAreaRect.x - 55, previewAreaRect.y + hideButtonRect.height,
                455, 25);
            Widgets.Label(labelRect, "RG.Biomes".Translate());
            var outRect = new Rect(labelRect.x, labelRect.yMax - 3, labelRect.width,
                previewAreaRect.height);
            var viewRect = new Rect(outRect.x, outRect.y, outRect.width - 16f,
                (DefDatabase<BiomeDef>.DefCount * 90) + 10);
            var rect3 = new Rect(outRect.xMax - 200f - 16f, labelRect.y, 200f, Text.LineHeight);

            Widgets.DrawBoxSolid(new Rect(outRect.x, outRect.y, outRect.width - 16f, outRect.height), BackgroundColor);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            var num = outRect.y + 15;
            foreach (var biomeDef in DefDatabase<BiomeDef>.AllDefs.OrderBy(x => x.label ?? x.defName))
            {
                DoBiomeSliders(biomeDef, labelRect.x + 10, ref num,
                    biomeDef.label?.CapitalizeFirst() ?? biomeDef.defName);
            }

            Widgets.EndScrollView();
            if (tmpWorldGenerationPreset.biomeCommonalities.All(x => x.Value == 10) &&
                tmpWorldGenerationPreset.biomeScoreOffsets.All(y => y.Value == 0))
            {
                return;
            }

            if (!Widgets.ButtonText(rect3, "ResetFactionsToDefault".Translate()))
            {
                return;
            }

            tmpWorldGenerationPreset.ResetBiomeCommonalities();
            tmpWorldGenerationPreset.ResetBiomeScoreOffsets();
            return;
        }

        DrawGeneratePreviewButton(window, generateButtonRect);
        var numAttempt = 0;
        if (thread is null && Find.World != null && Find.World.info.name != "DefaultWorldName" ||
            worldPreview != null)
        {
            if (dirty)
            {
                while (numAttempt < 5)
                {
                    worldPreview = GetWorldCameraPreview(WorldCameraHeight, WorldCameraWidth);
                    if (IsBlack(worldPreview))
                    {
                        numAttempt++;
                    }
                    else
                    {
                        dirty = false;
                        break;
                    }
                }
            }

            if (worldPreview != null)
            {
                GUI.DrawTexture(previewAreaRect, worldPreview);
            }
        }

        var numY = previewAreaRect.yMax - 40;
        if (tmpWorldGenerationPreset == null)
        {
            tmpWorldGenerationPreset = new WorldGenerationPreset();
        }

        DoSlider(previewAreaRect.x - 55, ref numY, 256, "RG.AncientRoadDensity".Translate(),
            ref tmpWorldGenerationPreset.ancientRoadDensity, "None".Translate());
        DoSlider(previewAreaRect.x - 55, ref numY, 256, "RG.FactionRoadDensity".Translate(),
            ref tmpWorldGenerationPreset.factionRoadDensity, "None".Translate());

        if (!ModCompat.MyLittlePlanetActive)
        {
            return;
        }

        numY += 40;
        labelRect = new Rect(previewAreaRect.x - 55, numY, 200f, 30f);
        var slider = new Rect(labelRect.xMax, numY, 256, 30f);
        Widgets.Label(labelRect, "RG.AxialTilt".Translate());
        tmpWorldGenerationPreset.axialTilt = (AxialTilt)Mathf.RoundToInt(Widgets.HorizontalSlider(slider,
            (float)tmpWorldGenerationPreset.axialTilt, 0f, AxialTiltUtility.EnumValuesCount - 1, true,
            "PlanetRainfall_Normal".Translate(), "PlanetRainfall_Low".Translate(),
            "PlanetRainfall_High".Translate(), 1f));
    }

    public static void ApplyChanges(Page_CreateWorldParams window)
    {
        AccessTools.Field(typeof(Page_CreateWorldParams), "rainfall")
            .SetValue(window, tmpWorldGenerationPreset.rainfall);
        AccessTools.Field(typeof(Page_CreateWorldParams), "population")
            .SetValue(window, tmpWorldGenerationPreset.population);
        AccessTools.Field(typeof(Page_CreateWorldParams), "planetCoverage")
            .SetValue(window, tmpWorldGenerationPreset.planetCoverage);
        AccessTools.Field(typeof(Page_CreateWorldParams), "seedString")
            .SetValue(window, tmpWorldGenerationPreset.seedString);
        AccessTools.Field(typeof(Page_CreateWorldParams), "temperature")
            .SetValue(window, tmpWorldGenerationPreset.temperature);
        var factionCounts =
            (Dictionary<FactionDef, int>)AccessTools.Field(typeof(Page_CreateWorldParams), "factionCounts")
                .GetValue(window);

        foreach (var data in tmpWorldGenerationPreset.factionCounts)
        {
            var factionDef = DefDatabase<FactionDef>.GetNamedSilentFail(data.Key);
            if (factionDef != null)
            {
                factionCounts[factionDef] = data.Value;
            }
        }

        AccessTools.Field(typeof(Page_CreateWorldParams), "factionCounts").SetValue(window, factionCounts);
    }

    private static bool IsBlack(Texture2D texture)
    {
        var pixel = texture.GetPixel(texture.width / 2, texture.height / 2);
        return pixel.r <= 0 && pixel.g <= 0 && pixel.b <= 0;
    }

    private static void StartRefreshWorldPreview(Page_CreateWorldParams window)
    {
        dirty = false;
        updatePreviewCounter = -1;
        if (thread is { IsAlive: true })
        {
            thread.Abort();
            generatingWorld = false;
        }

        if (hidePreview)
        {
            return;
        }

        thread = new Thread(delegate()
        {
            var planetCoverage =
                (float)AccessTools.Field(typeof(Page_CreateWorldParams), "planetCoverage").GetValue(window);
            var seedString =
                (string)AccessTools.Field(typeof(Page_CreateWorldParams), "seedString").GetValue(window);
            var rainfall =
                (OverallRainfall)AccessTools.Field(typeof(Page_CreateWorldParams), "rainfall").GetValue(window);
            var temperature =
                (OverallTemperature)AccessTools.Field(typeof(Page_CreateWorldParams), "temperature").GetValue(window);
            var population =
                (OverallPopulation)AccessTools.Field(typeof(Page_CreateWorldParams), "population").GetValue(window);
            var factionCounts =
                (Dictionary<FactionDef, int>)AccessTools.Field(typeof(Page_CreateWorldParams), "factionCounts")
                    .GetValue(window);
            GenerateWorld(planetCoverage, seedString, rainfall, temperature,
                population, factionCounts);
        });
        thread.Start();
    }

    private static void DrawHidePreviewButton(Page_CreateWorldParams window, Rect hideButtonRect)
    {
        var buttonTexture = Visible;
        if (hidePreview)
        {
            buttonTexture = InVisible;
        }

        if (Widgets.ButtonImageFitted(hideButtonRect, buttonTexture))
        {
            hidePreview = !hidePreview;
            if (!hidePreview)
            {
                StartRefreshWorldPreview(window);
            }
        }

        Widgets.DrawHighlightIfMouseover(hideButtonRect);
        TooltipHandler.TipRegion(hideButtonRect, "RG.HidePreview".Translate());
    }

    private static void DrawGeneratePreviewButton(Page_CreateWorldParams window, Rect generateButtonRect)
    {
        if (thread != null)
        {
            if (texSpinAngle > 360f)
            {
                texSpinAngle -= 360f;
            }

            texSpinAngle += 3;
        }

        if (Prefs.UIScale != 1f)
        {
            GUI.DrawTexture(generateButtonRect, GeneratePreview);
        }
        else
        {
            Widgets.DrawTextureRotated(generateButtonRect, GeneratePreview, texSpinAngle);
        }

        if (Mouse.IsOver(generateButtonRect))
        {
            Widgets.DrawHighlightIfMouseover(generateButtonRect);
            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    StartRefreshWorldPreview(window);
                    Event.current.Use();
                }
            }
        }

        if (thread == null || thread.IsAlive || threadedWorld == null)
        {
            return;
        }

        InitializeWorld();
        threadedWorld = null;
        thread = null;
        dirty = true;
        generatingWorld = false;
    }

    private static void InitializeWorld()
    {
        var layers = (List<WorldLayer>)AccessTools.Field(typeof(WorldRenderer), "layers").GetValue(Find.World.renderer);
        for (var i = 0; i < layers.Count; i++)
        {
            var layer = layers[i];
            if (layer is WorldLayer_Hills || layer is WorldLayer_Rivers || layer is WorldLayer_Roads ||
                layer is WorldLayer_Terrain)
            {
                layer.RegenerateNow();
            }
        }

        var comps = Find.World.components.Where(x => x.GetType().Name == "TacticalGroups");
        foreach (var comp in comps)
        {
            comp.FinalizeInit();
        }
    }

    public static void GenerateWorld(float planetCoverage, string seedString, OverallRainfall overallRainfall,
        OverallTemperature overallTemperature, OverallPopulation population,
        Dictionary<FactionDef, int> factionCounts = null)
    {
        generatingWorld = true;
        Rand.PushState();

        var seed = Rand.Seed = GenText.StableStringHash(seedString);
        var prevFaction = Find.World?.factionManager?.OfPlayer;
        var prevProgramState = Current.ProgramState;
        var prevGrid = Find.World?.grid;
        Current.ProgramState = ProgramState.Entry;
        if (prevFaction is null)
        {
            Find.GameInitData.ResetWorldRelatedMapInitData();
        }

        try
        {
            Current.CreatingWorld = new World
            {
                renderer = new WorldRenderer(),
                UI = new WorldInterface(),
                factionManager = new FactionManager(),
                grid = prevGrid
            };

            AccessTools.Field(typeof(FactionManager), "ofPlayer")
                .SetValue(Current.CreatingWorld.factionManager, prevFaction);
            Current.CreatingWorld.dynamicDrawManager = new WorldDynamicDrawManager();
            Current.CreatingWorld.ticksAbsCache = new ConfiguredTicksAbsAtGameStartCache();
            Current.Game.InitData.playerFaction = prevFaction;
            Current.CreatingWorld.info.seedString = seedString;
            Current.CreatingWorld.info.planetCoverage = planetCoverage;
            Current.CreatingWorld.info.overallRainfall = overallRainfall;
            Current.CreatingWorld.info.overallTemperature = overallTemperature;
            Current.CreatingWorld.info.overallPopulation = population;
            Current.CreatingWorld.info.name = NameGenerator.GenerateName(RulePackDefOf.NamerWorld);

            var tmpGenSteps = new List<WorldGenStepDef>();
            tmpGenSteps.AddRange(WorldGenerator.GenStepsInOrder);
            for (var i = 0; i < tmpGenSteps.Count; i++)
            {
                try
                {
                    Rand.Seed = Gen.HashCombineInt(seed, GetSeedPart(tmpGenSteps, i));
                    if (!worldGenStepDefs.Contains(tmpGenSteps[i]))
                    {
                        continue;
                    }

                    tmpGenSteps[i].worldGenStep.GenerateFresh(seedString);
                    if (tmpGenSteps[i].defName == "Components" && prevFaction != null)
                    {
                        AccessTools.Field(typeof(FactionManager), "ofPlayer")
                            .SetValue(Current.CreatingWorld.factionManager, prevFaction);
                    }
                }
                catch (Exception ex)
                {
                    if (!(ex is ThreadAbortException))
                    {
                        Log.Error("Error in WorldGenStep: " + ex);
                    }
                    else
                    {
                        Rand.PopState();
                        Current.CreatingWorld = null;
                        generatingWorld = false;
                        Current.ProgramState = prevProgramState;
                        return;
                    }
                }
            }

            threadedWorld = Current.CreatingWorld;
            Current.Game.World = null;
            Current.Game.World = threadedWorld;
            if (Find.World != null)
            {
                Find.World.features = new WorldFeatures();
            }

            MemoryUtility.UnloadUnusedUnityAssets();
        }
        catch (Exception ex)
        {
            if (!(ex is ThreadAbortException))
            {
                Log.Error("Error: " + ex);
            }
            else
            {
                var stateStack =
                    (string)AccessTools.Field(typeof(Rand), "stateStack").GetValue(null);
                if (stateStack.Any())
                {
                    Rand.PopState();
                }

                generatingWorld = false;
                Current.ProgramState = prevProgramState;
                Current.CreatingWorld = null;
            }
        }
        finally
        {
            var stateStack =
                (string)AccessTools.Field(typeof(Rand), "stateStack").GetValue(null);
            if (stateStack.Any())
            {
                Rand.PopState();
            }

            generatingWorld = false;
            Current.CreatingWorld = null;
            Current.ProgramState = prevProgramState;
        }
    }

    private static int GetSeedPart(List<WorldGenStepDef> genSteps, int index)
    {
        var seedPart = genSteps[index].worldGenStep.SeedPart;
        var num = 0;
        for (var i = 0; i < index; i++)
        {
            if (genSteps[i].worldGenStep.SeedPart == seedPart)
            {
                num++;
            }
        }

        return seedPart + num;
    }

    private static Texture2D GetWorldCameraPreview(int width, int height)
    {
        Find.World.renderer.wantedMode = WorldRenderMode.Planet;
        Find.WorldCamera.gameObject.SetActive(true);
        Find.World.UI.Reset();
        AccessTools.Field(typeof(WorldCameraDriver), "desiredAltitude").SetValue(Find.WorldCameraDriver, 800);
        Find.WorldCameraDriver.altitude = 800;
        AccessTools.Method(typeof(WorldCameraDriver), "ApplyPositionToGameObject")
            .Invoke(Find.WorldCameraDriver, new object[] { });

        var rect = new Rect(0, 0, width, height);
        var renderTexture = new RenderTexture(width, height, 24);
        var screenShot = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Find.WorldCamera.targetTexture = renderTexture;
        Find.WorldCamera.Render();

        ExpandableWorldObjectsUtility.ExpandableWorldObjectsUpdate();
        Find.World.renderer.DrawWorldLayers();
        Find.World.dynamicDrawManager.DrawDynamicWorldObjects();
        Find.World.features.UpdateFeatures();
        NoiseDebugUI.RenderPlanetNoise();

        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();
        Find.WorldCamera.targetTexture = null;
        RenderTexture.active = null;

        Find.WorldCamera.gameObject.SetActive(false);
        Find.World.renderer.wantedMode = WorldRenderMode.None;
        return screenShot;
    }

    private static void UpdateCurPreset(Page_CreateWorldParams window)
    {
        if (tmpWorldGenerationPreset is null)
        {
            tmpWorldGenerationPreset = new WorldGenerationPreset();
            tmpWorldGenerationPreset.Init();
        }

        var planetCoverage =
            (float)AccessTools.Field(typeof(Page_CreateWorldParams), "planetCoverage").GetValue(window);
        var seedString =
            (string)AccessTools.Field(typeof(Page_CreateWorldParams), "seedString").GetValue(window);
        var rainfall =
            (OverallRainfall)AccessTools.Field(typeof(Page_CreateWorldParams), "rainfall").GetValue(window);
        var temperature =
            (OverallTemperature)AccessTools.Field(typeof(Page_CreateWorldParams), "temperature").GetValue(window);
        var population =
            (OverallPopulation)AccessTools.Field(typeof(Page_CreateWorldParams), "population").GetValue(window);
        var factionCounts =
            (Dictionary<FactionDef, int>)AccessTools.Field(typeof(Page_CreateWorldParams), "factionCounts")
                .GetValue(window);

        tmpWorldGenerationPreset.factionCounts = factionCounts.ToDictionary(x => x.Key.defName, y => y.Value);
        tmpWorldGenerationPreset.temperature = temperature;
        tmpWorldGenerationPreset.seedString = seedString;
        tmpWorldGenerationPreset.planetCoverage = planetCoverage;
        tmpWorldGenerationPreset.rainfall = rainfall;
        tmpWorldGenerationPreset.population = population;
    }

    private static void DoSlider(float x, ref float num, float width2, string label, ref float field, string leftLabel)
    {
        num += 40f;
        var labelRect = new Rect(x, num, 200f, 30f);
        Widgets.Label(labelRect, label);
        var slider = new Rect(labelRect.xMax, num, width2, 30f);
        field = Widgets.HorizontalSlider(slider, field, 0, 2f, true,
            "PlanetRainfall_Normal".Translate(), leftLabel, "PlanetRainfall_High".Translate(), 0.1f);
    }

    private static void DoBiomeSliders(BiomeDef biomeDef, float x, ref float num, string label)
    {
        var labelRect = new Rect(x, num - 10, 200f, 30f);
        Widgets.Label(labelRect, label);
        num += 10;
        var biomeCommonalityLabel = new Rect(labelRect.x, num + 5, 70, 30);
        var value = tmpWorldGenerationPreset.biomeCommonalities[biomeDef.defName];
        if (value < 10f)
        {
            GUI.color = Color.red;
        }
        else if (value > 10f)
        {
            GUI.color = Color.green;
        }

        Widgets.Label(biomeCommonalityLabel, "RG.Commonality".Translate());
        var biomeCommonalitySlider = new Rect(biomeCommonalityLabel.xMax + 5, num, 340, 30f);
        tmpWorldGenerationPreset.biomeCommonalities[biomeDef.defName] =
            (int)Widgets.HorizontalSlider(biomeCommonalitySlider, value, 0, 20, false, (value * 10) + "%");
        GUI.color = Color.white;
        num += 30f;

        var biomeOffsetLabel = new Rect(labelRect.x, num + 5, 70, 30);
        var value2 = tmpWorldGenerationPreset.biomeScoreOffsets[biomeDef.defName];
        if (value2 < 0f)
        {
            GUI.color = Color.red;
        }
        else if (value2 > 0f)
        {
            GUI.color = Color.green;
        }

        Widgets.Label(biomeOffsetLabel, "RG.ScoreOffset".Translate());
        var scoreOffsetSlider = new Rect(biomeOffsetLabel.xMax + 5, biomeCommonalitySlider.yMax, 340, 30f);
        tmpWorldGenerationPreset.biomeScoreOffsets[biomeDef.defName] =
            (int)Widgets.HorizontalSlider(scoreOffsetSlider, value2, -99, 99, false, value2.ToString());
        GUI.color = Color.white;
        num += 50f;
    }
}