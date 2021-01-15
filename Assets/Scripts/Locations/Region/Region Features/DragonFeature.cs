using Inner_Maps.Location_Structures;
using UtilityScripts;
namespace Locations.Region_Features {
    public class DragonFeature : RegionFeature {
        public override void ActivateFeatureInWorldGen(Region region) {
            base.ActivateFeatureInWorldGen(region);
            if (region.HasStructure(STRUCTURE_TYPE.CAVE)) {
                LocationStructure cave = region.GetRandomStructureOfType(STRUCTURE_TYPE.CAVE);
                //Spawn Dragon
                Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Dragon, FactionManager.Instance.neutralFaction, 
                    null, region, cave);
                CharacterManager.Instance.PlaceSummonInitially(summon, CollectionUtilities.GetRandomElement(cave.unoccupiedTiles));
            }
        }
    }
}