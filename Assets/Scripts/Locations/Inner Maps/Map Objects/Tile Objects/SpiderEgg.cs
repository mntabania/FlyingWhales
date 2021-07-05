using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class SpiderEgg : MonsterEgg {

    public override System.Type serializedData => typeof(SaveDataSpiderEgg);

    public SpiderEgg() : base(TILE_OBJECT_TYPE.SPIDER_EGG, SUMMON_TYPE.Giant_Spider, GameManager.Instance.GetTicksBasedOnHour(1)) { }
    public SpiderEgg(SaveDataSpiderEgg data) : base(data) { }
    
    #region Overrides
    public override string ToString() {
        return $"Spider Egg {id.ToString()}";
    }
    protected override void Hatch() {
        BaseSettlement homeSettlement = settlement;
        LocationStructure homeStructure = structure;
        Region homeRegion = gridTileLocation.area.region;
        if (settlement != null) {
            homeRegion = settlement.region;
        }
        ProcessHomeOfHatchedEggs(ref homeSettlement, ref homeStructure, ref homeRegion);

        int numOfSpiders = UnityEngine.Random.Range(2, 4);
        for (int i = 0; i < numOfSpiders; i++) {
            Summon monster = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Small_Spider, faction: faction, homeLocation: homeSettlement, homeRegion: homeRegion, homeStructure: homeStructure, bypassIdeologyChecking: true);
            // if (monster.faction.isPlayerFaction) {
            //     monster.traitContainer.RemoveTrait(monster, monster.bredBehaviour);
            //     monster.traitContainer.AddTrait(monster, "Baby Infestor");
            // }
            CharacterManager.Instance.PlaceSummonInitially(monster, gridTileLocation);

            //BaseSettlement settlement;
            //if (gridTileLocation.structure.structureType.IsSpecialStructure()) {
            //    monster.ClearTerritoryAndMigrateHomeStructureTo(gridTileLocation.structure);
            //} else if (gridTileLocation.IsPartOfSettlement(out settlement)) {
            //    monster.ClearTerritory();
            //    monster.MigrateHomeTo(settlement);
            //} else 
            if (!monster.HasHome()) {
                monster.ClearTerritory();
                monster.SetTerritory(gridTileLocation.area);
            }
        }

    }
    #endregion

}
public class SaveDataSpiderEgg : SaveDataMonsterEgg { }