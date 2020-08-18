using System.Collections;
using System.Collections.Generic;
using Locations.Region_Features;
using Scenario_Maps;
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
    
    public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
        if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
            //add dragon feature to second world
            Region chosenRegion = GridMap.Instance.allRegions[0];
            chosenRegion.regionFeatureComponent.AddFeature(RegionFeatureDB.Dragon);
            chosenRegion.regionFeatureComponent.AddFeature(RegionFeatureDB.Crystals);
        } else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom) {
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
        }
        yield return null;
    }

    #region Scenario Maps
    public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
        //TODO:
        yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
    }
    #endregion
    
    #region Saved World
    public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
        yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
    }
    #endregion
    
    // public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
    //     if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Second_World) {
    //         //add dragon feature to second world
    //         Region chosenRegion = GridMap.Instance.allRegions[0];
    //         string chosenFeature = RegionFeatureDB.Dragon;
    //         chosenRegion.regionFeatureComponent.AddFeature(chosenFeature);
    //     }
    //     yield return null;
    // }
}
