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
        // RegionFeatureDB.Poison_Vents,
        RegionFeatureDB.Ruins, 
        RegionFeatureDB.Teeming, 
        // RegionFeatureDB.Vapor_Vents
    };
    
    public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
        if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
            //add dragon feature to second world
            Region chosenRegion = GridMap.Instance.allRegions[0];
            chosenRegion.regionFeatureComponent.AddFeature(RegionFeatureDB.Dragon);
            chosenRegion.regionFeatureComponent.AddFeature(RegionFeatureDB.Crystals);
        } else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom) {
            if (GameUtilities.RollChance(20)) {
                Region region = GridMap.Instance.mainRegion;
                string chosenFeature = CollectionUtilities.GetRandomElement(regionFeatureChoices);
                region.regionFeatureComponent.AddFeature(chosenFeature);
#if DEBUG_LOG
                Debug.Log($"Added feature {chosenFeature} to {region.name}");
#endif
            }
        }
        yield return null;
    }

#region Scenario Maps
    public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
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
