﻿using Inner_Maps.Location_Structures;
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
        int numOfSpiders = UnityEngine.Random.Range(2, 4);
        for (int i = 0; i < numOfSpiders; i++) {
            Summon monster = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Small_Spider, characterThatLay.faction, homeRegion: gridTileLocation.parentMap.region, bypassIdeologyChecking: true);
            if (monster.faction.isPlayerFaction) {
                monster.traitContainer.RemoveTrait(monster, monster.bredBehaviour);
                monster.traitContainer.AddTrait(monster, "Baby Infestor");
            }
            monster.CreateMarker();
            monster.InitialCharacterPlacement(gridTileLocation);
            monster.OnPlaceSummon(gridTileLocation);

            BaseSettlement settlement;
            if (gridTileLocation.structure.structureType.IsSpecialStructure()) {
                monster.ClearTerritoryAndMigrateHomeStructureTo(gridTileLocation.structure);
            } else if (gridTileLocation.IsPartOfSettlement(out settlement)) {
                monster.ClearTerritory();
                monster.MigrateHomeTo(settlement);
            } else if (gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                monster.ClearTerritory();
                monster.SetTerritory(gridTileLocation.collectionOwner.partOfHextile.hexTileOwner);
            }
        }

    }
    #endregion

}
public class SaveDataSpiderEgg : SaveDataMonsterEgg { }