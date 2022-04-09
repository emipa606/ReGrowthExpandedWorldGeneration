using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RGExpandedWorldGeneration;

public class WorldComponent_WorldGenerator : WorldComponent
{
    public static Dictionary<AxialTilt, SimpleCurve> mappedValues = new Dictionary<AxialTilt, SimpleCurve>
    {
        {
            AxialTilt.VeryLow, new SimpleCurve
            {
                new CurvePoint(0f, 0.75f),
                new CurvePoint(0.1f, 1f),
                new CurvePoint(1f, 7f)
            }
        },
        {
            AxialTilt.Low, new SimpleCurve
            {
                new CurvePoint(0f, 1.5f),
                new CurvePoint(0.1f, 2f),
                new CurvePoint(1f, 14f)
            }
        },
        {
            AxialTilt.Normal, new SimpleCurve
            {
                new CurvePoint(0f, 3f),
                new CurvePoint(0.1f, 4f),
                new CurvePoint(1f, 28f)
            }
        },
        {
            AxialTilt.High, new SimpleCurve
            {
                new CurvePoint(0f, 4.5f),
                new CurvePoint(0.1f, 6f),
                new CurvePoint(1f, 42f)
            }
        },
        {
            AxialTilt.VeryHigh, new SimpleCurve
            {
                new CurvePoint(0f, 6f),
                new CurvePoint(0.1f, 8f),
                new CurvePoint(1f, 56f)
            }
        }
    };

    public static WorldComponent_WorldGenerator Instance;
    public AxialTilt axialTilt = AxialTilt.Normal;

    public bool worldGenerated;

    public WorldComponent_WorldGenerator(World world) : base(world)
    {
        Instance = this;
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        if (worldGenerated || RGExpandedWorldGenerationSettings.curWorldGenerationPreset == null)
        {
            return;
        }

        axialTilt = RGExpandedWorldGenerationSettings.curWorldGenerationPreset.axialTilt;
        worldGenerated = true;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref worldGenerated, "worldGenerated");
        Scribe_Values.Look(ref axialTilt, "axialTilt", AxialTilt.Normal, true);
        Instance = this;
    }
}