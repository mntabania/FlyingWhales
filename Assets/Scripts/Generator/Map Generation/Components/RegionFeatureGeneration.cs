using System.Collections;
using System.Collections.Generic;
using Locations.Region_Features;
using UnityEngine;
using UtilityScripts;

public class RegionFeatureGeneration : MapGenerationComponent {

    private readonly string[] regionFeatureChoices = new[] {
        RegionFeatureDB.Crystals, 
        RegionFeatureDB.Dragon, 
        RegionFeatureDB.Haunted, 
        RegionFeatureDB.Poison_Vents,
        RegionFeatureDB.Ruins, 
        RegionFeatureDB.Teeming, 
        RegionFeatureDB.Vapor_Vents
    };
    
    public override IEnumerator Execute(MapGenerationData data) {
        if (GameUtilities.RollChance(20)) { 
            int regionsWithSpecialFeatureCount = 0;
            if (GridMap.Instance.allRegions.Length == 1) {
                regionsWithSpecialFeatureCount = 1;
            } else {
                regionsWithSpecialFeatureCount = GameUtilities.RollChance(80) ? 1 : 2;
            }
            List<Region> regionChoices = new List<Region>(GridMap.Instance.allRegions);
            for (int i = 0; i < regionsWithSpecialFeatureCount; i++) {
                if (regionChoices.Count == 0) { break; }
                Region chosenRegion = CollectionUtilities.GetRandomElement(regionChoices);
                string chosenFeature = CollectionUtilities.GetRandomElement(regionFeatureChoices);
                chosenRegion.regionFeatureComponent.AddFeature(chosenFeature);
                regionChoices.Remove(chosenRegion);
                Debug.Log($"Added feature {chosenFeature} to {chosenRegion.name}");
            }
        }
        yield return null;
    }
}
