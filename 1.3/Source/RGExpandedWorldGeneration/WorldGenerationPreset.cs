﻿using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RGExpandedWorldGeneration
{
    public class WorldGenerationPreset : IExposable
    {
		public Dictionary<string, int> factionCounts;
		public Dictionary<string, float> biomeCommonalities;
		public string seedString;
		public float planetCoverage;
		public OverallRainfall rainfall;
		public OverallTemperature temperature;
		public OverallPopulation population;
		public float riverDensity;
		public float ancientRoadDensity;
		public float factionRoadDensity;
		public float mountainDensity;
		public float seaLevel;
		public void Init()
        {
			seedString = GenText.RandomSeedString();
			planetCoverage = 0.3f;
			rainfall = OverallRainfall.Normal;
			temperature = OverallTemperature.Normal;
			population = OverallPopulation.Normal;
			riverDensity = 1f;
			ancientRoadDensity = 1f;
			factionRoadDensity = 1f;
			mountainDensity = 1f;
			seaLevel = 1f;
			ResetFactionCounts();
			ResetBiomeCommonalities();
		}

		public bool IsDifferentFrom(WorldGenerationPreset other)
		{
			if (seedString != other.seedString || planetCoverage != other.planetCoverage || rainfall != other.rainfall || temperature != other.temperature
				|| population != other.population || riverDensity != other.riverDensity || ancientRoadDensity != other.ancientRoadDensity
				|| factionRoadDensity != other.factionRoadDensity || mountainDensity != other.mountainDensity || seaLevel != other.seaLevel)
            {
				return true;
            }

			if (factionCounts.Count != other.factionCounts.Count || !factionCounts.ContentEquals(other.factionCounts))
			{
				return true;
			}
			if (biomeCommonalities.Count != other.biomeCommonalities.Count || !biomeCommonalities.ContentEquals(other.biomeCommonalities))
			{
				return true;
            }
			return false;
		}

		public WorldGenerationPreset MakeCopy()
        {
			var copy = new WorldGenerationPreset();
			copy.factionCounts = this.factionCounts.ToDictionary(x => x.Key, y => y.Value);
			copy.biomeCommonalities = this.biomeCommonalities.ToDictionary(x => x.Key, y => y.Value);
			copy.seedString = this.seedString;
			copy.planetCoverage = this.planetCoverage;
			copy.rainfall = this.rainfall;
			copy.temperature = this.temperature;
			copy.population = this.population;
			copy.riverDensity = this.riverDensity;
			copy.ancientRoadDensity = this.ancientRoadDensity;
			copy.factionRoadDensity = this.factionRoadDensity;
			copy.mountainDensity = this.mountainDensity;
			copy.seaLevel = this.seaLevel;
			return copy;
		}
		private void ResetFactionCounts()
		{
			factionCounts = new Dictionary<string, int>();
			foreach (FactionDef configurableFaction in FactionGenerator.ConfigurableFactions)
			{
				factionCounts.Add(configurableFaction.defName, configurableFaction.startingCountAtWorldCreation);
			}
		}

		private void ResetBiomeCommonalities()
		{
			biomeCommonalities = new Dictionary<string, float>();
			foreach (BiomeDef biomeDef in DefDatabase<BiomeDef>.AllDefs)
			{
				biomeCommonalities.Add(biomeDef.defName, 1f);
			}
		}
		public void ExposeData()
        {
			Scribe_Collections.Look(ref factionCounts, "factionCounts", LookMode.Value, LookMode.Value);
			Scribe_Collections.Look(ref biomeCommonalities, "biomeCommonalities", LookMode.Value, LookMode.Value);
			Scribe_Values.Look(ref seedString, "seedString");
			Scribe_Values.Look(ref planetCoverage, "planetCoverage");
			Scribe_Values.Look(ref rainfall, "rainfall");
			Scribe_Values.Look(ref temperature, "temperature");
			Scribe_Values.Look(ref population, "population");
			Scribe_Values.Look(ref riverDensity, "riverDensity");
			Scribe_Values.Look(ref ancientRoadDensity, "ancientRoadDensity");
			Scribe_Values.Look(ref factionRoadDensity, "settlementRoadDensity");
			Scribe_Values.Look(ref mountainDensity, "mountainDensity");
			Scribe_Values.Look(ref seaLevel, "seaLevel");
		}
	}
}
