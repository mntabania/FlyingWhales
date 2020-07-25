using System.Collections.Generic;
using System.Linq;
using Locations.Tile_Features;
using UnityEngine;
using UtilityScripts;
namespace Locations.Region_Features {
    public class TeemingFeature : RegionFeature {
        public override void LandmarkGenerationSecondPassActions(Region region) {
            base.LandmarkGenerationSecondPassActions(region);
            List<GameFeature> gameFeatures = GetGameFeaturesInRegion(region);
            if (gameFeatures.Count < 6) {
                int missing = Random.Range(6, 9) - gameFeatures.Count;
                //choose from random flat/tree tile without game feature
                List<HexTile> choices = region.tiles
                    .Where(x => (x.elevationType == ELEVATION.PLAIN || x.elevationType == ELEVATION.TREES) &&
                                x.featureComponent.HasFeature(TileFeatureDB.Game_Feature) == false).ToList();
                
                for (int i = 0; i < missing; i++) {
                    if (choices.Count == 0) { break; }
                    HexTile chosenTile = CollectionUtilities.GetRandomElement(choices);
                    GameFeature feature = LandmarkManager.Instance.CreateTileFeature<GameFeature>(TileFeatureDB.Game_Feature);
                    chosenTile.featureComponent.AddFeature(feature, chosenTile);
                    gameFeatures.Add(feature);
                    choices.Remove(chosenTile);
                }
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
            for (int i = 0; i < region.tiles.Count; i++) {
                HexTile tile = region.tiles[i];
                GameFeature feature = tile.featureComponent.GetFeature<GameFeature>();
                if (feature != null) {
                    gameFeatures.Add(feature);
                }
            }
            return gameFeatures;
        }
    }
}