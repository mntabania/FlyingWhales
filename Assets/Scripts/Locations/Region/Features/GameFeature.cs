using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class GameFeature : TileFeature {

    private const int MaxAnimals = 6;

    private HexTile owner;
    private int currentAnimalCount;
    private bool isGeneratingPerHour;
    
    public GameFeature() {
        name = "Game";
        description = "Hunters can obtain food here.";
    }
    
    #region Overrides
    public override void GameStartActions(HexTile tile) {
        owner = tile;
        Messenger.AddListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
        Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        List<TileObject> animals = tile.GetTileObjectsInHexTile(TILE_OBJECT_TYPE.SMALL_ANIMAL);
        currentAnimalCount = animals.Count;
        if (animals.Count <= MaxAnimals) {
            int missing = MaxAnimals - animals.Count;
            for (int i = 0; i < missing; i++) {
                if (CreateNewSmallAnimal() == false) {
                    break;
                }
            }
        }
    }
    public override void OnRemoveFeature(HexTile tile) {
        base.OnRemoveFeature(tile);
        Messenger.RemoveListener(Signals.HOUR_STARTED, TryGeneratePerHour);
    }
    #endregion


    private void OnTileObjectPlaced(TileObject tileObject, LocationGridTile tile) {
        if (tile.collectionOwner.isPartOfParentRegionMap && 
            tile.collectionOwner.partOfHextile.hexTileOwner == owner && tileObject.tileObjectType == TILE_OBJECT_TYPE.SMALL_ANIMAL) {
            AdjustAnimalCount(1);
        }
    }
    private void OnTileObjectRemoved(TileObject tileObject, Character character, LocationGridTile tile) {
        if (tile.collectionOwner.isPartOfParentRegionMap &&
            tile.collectionOwner.partOfHextile.hexTileOwner == owner && tileObject.tileObjectType == TILE_OBJECT_TYPE.SMALL_ANIMAL) {
            AdjustAnimalCount(-1);
        }
    }

    private void AdjustAnimalCount(int amount) {
        currentAnimalCount += amount;
        OnAnimalCountChanged();
    }


    private void OnAnimalCountChanged() {
        if (currentAnimalCount < MaxAnimals) {
            if (isGeneratingPerHour == false) {
                isGeneratingPerHour = true;
                Messenger.AddListener(Signals.HOUR_STARTED, TryGeneratePerHour);    
            }
        } else {
            isGeneratingPerHour = false;
            Messenger.RemoveListener(Signals.HOUR_STARTED, TryGeneratePerHour);
        }
    }

    private void TryGeneratePerHour() {
        if (Random.Range(0, 100) < 25) {
            CreateNewSmallAnimal();
        }
    }

    private bool CreateNewSmallAnimal() {
        List<LocationGridTile> choices = owner.locationGridTiles.Where(x => x.isOccupied == false 
                                                                            && x.structure.structureType.IsOpenSpace()).ToList();
        if (choices.Count > 0) {
            LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(choices);
            chosenTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.SMALL_ANIMAL),
                chosenTile);
            return true;
        }
        return false;
    }
}
