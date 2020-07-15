public class SpiderEgg : MonsterEgg {

    public SpiderEgg() : base(TILE_OBJECT_TYPE.SPIDER_EGG, SUMMON_TYPE.Giant_Spider, GameManager.Instance.GetTicksBasedOnHour(1)) { }

    #region Overrides
    public override string ToString() {
        return $"Spider Egg {id.ToString()}";
    }
    protected override void Hatch() {
        int numOfSpiders = UnityEngine.Random.Range(2, 4);
        for (int i = 0; i < numOfSpiders; i++) {
            Character monster = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Small_Spider, characterThatLay.faction, homeRegion: gridTileLocation.parentMap.region);
            if (monster.faction.isPlayerFaction) {
                monster.traitContainer.RemoveTrait(monster, monster.characterClass.traitNameOnTamedByPlayer);
                monster.traitContainer.AddTrait(monster, "Baby Infestor");
            }
            monster.CreateMarker();
            monster.InitialCharacterPlacement(gridTileLocation, true);
            if (gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                monster.ClearTerritory();
                monster.AddTerritory(gridTileLocation.collectionOwner.partOfHextile.hexTileOwner);
            }
        }

    }
    #endregion

}
