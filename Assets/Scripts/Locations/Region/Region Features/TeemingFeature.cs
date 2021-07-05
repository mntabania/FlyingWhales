using System.Collections.Generic;
using System.Linq;
using Locations.Area_Features;
using UnityEngine;
using UtilityScripts;
namespace Locations.Region_Features {
    public class TeemingFeature : RegionFeature {
        public override void SpecialStructureGenerationSecondPassActions(Region region) {
            base.SpecialStructureGenerationSecondPassActions(region);
            List<GameFeature> gameFeatures = GetGameFeaturesInRegion(region);
            if (gameFeatures.Count < 6) {
                int missing = Random.Range(6, 9) - gameFeatures.Count;
                //choose from random flat/tree tile without game feature
                List<Area> choices = ObjectPoolManager.Instance.CreateNewAreaList();
                for (int i = 0; i < region.areas.Count; i++) {
                    Area currArea = region.areas[i];
                    if(currArea.elevationType == ELEVATION.PLAIN && 
                       currArea.featureComponent.HasFeature(AreaFeatureDB.Game_Feature) == false && currArea.structureComponent.HasStructureInArea() == false) {
                        choices.Add(currArea);
                    }
                }
                for (int i = 0; i < missing; i++) {
                    if (choices.Count == 0) { break; }
                    Area chosenArea = CollectionUtilities.GetRandomElement(choices);
                    GameFeature feature = LandmarkManager.Instance.CreateAreaFeature<GameFeature>(AreaFeatureDB.Game_Feature);
                    chosenArea.featureComponent.AddFeature(feature, chosenArea);
                    gameFeatures.Add(feature);
                    choices.Remove(chosenArea);
                }
                ObjectPoolManager.Instance.ReturnAreaListToPool(choices);
            }

            //set spawn type to same for every feature
            SUMMON_TYPE animalType = CollectionUtilities.GetRandomElement(GameFeature.spawnChoices);
            for (int i = 0; i < gameFeatures.Count; i++) {
                GameFeature gameFeature = gameFeatures[i];
                gameFeature.SetSpawnType(animalType);
            }
        }

        private List<GameFeature> GetGameFeaturesInRegion(Region region) {
            List<GameFeature> gameFeatures = new List<GameFeature>();
            for (int i = 0; i < region.areas.Count; i++) {
                Area area = region.areas[i];
                GameFeature feature = area.featureComponent.GetFeature<GameFeature>();
                if (feature != null) {
                    gameFeatures.Add(feature);
                }
            }
            return gameFeatures;
        }
    }
}