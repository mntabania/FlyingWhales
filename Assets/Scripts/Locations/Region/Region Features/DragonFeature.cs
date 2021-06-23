using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;
namespace Locations.Region_Features {
    public class DragonFeature : RegionFeature {
        public override void ActivateFeatureInWorldGen(Region region) {
            base.ActivateFeatureInWorldGen(region);
            if (region.HasStructure(STRUCTURE_TYPE.CAVE)) {
                List<LocationStructure> caves = region.GetStructuresAtLocation(STRUCTURE_TYPE.CAVE);
                if (caves != null) {
                    for (int i = 0; i < caves.Count; i++) {
                        LocationStructure structure = caves[i];
                        if (structure is Cave cave && !cave.hasConnectedMine) {
                            //do not spawn monsters on caves with currently connected mines.
                            //Reference: https://trello.com/c/oFzZ2tV7/4811-monsters-should-no-longer-spawn-in-caves-connected-to-a-claimed-mine
                            //Spawn Dragon
                            Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Dragon, FactionManager.Instance.neutralFaction, 
                                null, region, cave);
                            CharacterManager.Instance.PlaceSummonInitially(summon, CollectionUtilities.GetRandomElement(cave.unoccupiedTiles));
                            break;
                        }
                    }
                    // LocationStructure cave = region.GetRandomStructureOfType(STRUCTURE_TYPE.CAVE);
                    // //Spawn Dragon
                    // Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Dragon, FactionManager.Instance.neutralFaction, 
                    //     null, region, cave);
                    // CharacterManager.Instance.PlaceSummonInitially(summon, CollectionUtilities.GetRandomElement(cave.unoccupiedTiles));    
                }
                
            }
        }
    }
}