using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class TreasureChest : TileObject {

    private bool _hasBeenAwakened;

    public IPointOfInterest objectThatWasObtained { get; private set; }
    
    private readonly TILE_OBJECT_TYPE[] _possibleItems = new[] {
        TILE_OBJECT_TYPE.HEALING_POTION,
        TILE_OBJECT_TYPE.POISON_FLASK,
        TILE_OBJECT_TYPE.ANTIDOTE,
        TILE_OBJECT_TYPE.EMBER,
        TILE_OBJECT_TYPE.ICE,
        TILE_OBJECT_TYPE.WOOD_PILE,
        TILE_OBJECT_TYPE.STONE_PILE,
        TILE_OBJECT_TYPE.METAL_PILE,
        TILE_OBJECT_TYPE.ANIMAL_MEAT,
        TILE_OBJECT_TYPE.MIMIC_TILE_OBJECT,
    };
    
    public TreasureChest() {
        Initialize(TILE_OBJECT_TYPE.TREASURE_CHEST, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.OPEN);
    }
    public TreasureChest(SaveDataTileObject data) {
        Initialize(data, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.OPEN);
    }
    protected override string GenerateName() { return "Treasure Chest"; }
    public override void OnDoActionToObject(ActualGoapNode action) {
        if (action.goapType == INTERACTION_TYPE.OPEN) {
            RollForItem();
        }
        // if (action.action.actionCategory == ACTION_CATEGORY.DIRECT || action.action.actionCategory == ACTION_CATEGORY.CONSUME) {
        //     action.actor.StopCurrentActionNode();
        //     AwakenMimic();
        // }
    }

    private void RollForItem() {
        if (objectThatWasObtained != null) { return; }
        TILE_OBJECT_TYPE chosenType = CollectionUtilities.GetRandomElement(_possibleItems);
        if (chosenType == TILE_OBJECT_TYPE.MIMIC_TILE_OBJECT) {
            Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Mimic, FactionManager.Instance.neutralFaction, homeRegion: gridTileLocation.parentMap.region);
            objectThatWasObtained = summon;
        } else {
            objectThatWasObtained = InnerMapManager.Instance.CreateNewTileObject<TileObject>(chosenType);    
        }
    }
}