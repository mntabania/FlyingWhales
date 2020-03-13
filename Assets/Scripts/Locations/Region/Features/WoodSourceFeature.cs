using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class WoodSourceFeature : TileFeature {
    private const int MaxBigTrees = 4;
    private const int MaxSmallTrees = 8;
    
    private HexTile owner;
    private int currentBigTreeCount;
    private int currentSmallTreeCount;
    private bool isGeneratingBigTreePerHour;
    private bool isGeneratingSmallTreePerHour;

    public WoodSourceFeature() {
        name = "Wood Source";
        description = "Provides wood.";
        type = REGION_FEATURE_TYPE.PASSIVE;
        isRemovedOnInvade = true;
    }  
    
    #region Overrides
    public override void GameStartActions(HexTile tile) {
        owner = tile;
        Messenger.AddListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
        Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        
        List<TileObject> bigTrees = tile.GetTileObjectsInHexTile(TILE_OBJECT_TYPE.BIG_TREE_OBJECT);
        currentBigTreeCount = bigTrees.Count;
        if (bigTrees.Count < MaxBigTrees) {
            int missingTrees = MaxBigTrees - bigTrees.Count;
            for (int i = 0; i <= missingTrees; i++) {
                if (CreateNewBigTree() == false) {
                    break;
                }
            }
        }
        
        List<TileObject> smallTrees = tile.GetTileObjectsInHexTile(TILE_OBJECT_TYPE.TREE_OBJECT);
        currentSmallTreeCount = smallTrees.Count;
        if (smallTrees.Count < MaxSmallTrees) {
            int missingTrees = MaxSmallTrees - smallTrees.Count;
            for (int i = 0; i <= missingTrees; i++) {
                if (CreateNewSmallTree() == false) {
                    break;
                }
            }
        }
    }
    public override void OnRemoveFeature(HexTile tile) {
        base.OnRemoveFeature(tile);
        Messenger.RemoveListener(Signals.HOUR_STARTED, TryGenerateBigTreePerHour);
        Messenger.RemoveListener(Signals.HOUR_STARTED, TryGenerateSmallTreePerHour);
    }
    #endregion
    
    private void OnTileObjectPlaced(TileObject tileObject, LocationGridTile tile) {
        if (tile.collectionOwner.isPartOfParentRegionMap && tile.collectionOwner.partOfHextile.hexTileOwner == owner) {
            if (tileObject.tileObjectType == TILE_OBJECT_TYPE.BIG_TREE_OBJECT) {
                AdjustBigTreeCount(1);    
            } else if (tileObject.tileObjectType == TILE_OBJECT_TYPE.TREE_OBJECT) {
                AdjustSmallTreeCount(1);
            }
            
        }
    }
    private void OnTileObjectRemoved(TileObject tileObject, Character character, LocationGridTile tile) {
        if (tile.collectionOwner.isPartOfParentRegionMap && tile.collectionOwner.partOfHextile.hexTileOwner == owner) {
            if (tileObject.tileObjectType == TILE_OBJECT_TYPE.BIG_TREE_OBJECT) {
                AdjustBigTreeCount(-1);    
            } else if (tileObject.tileObjectType == TILE_OBJECT_TYPE.TREE_OBJECT) {
                AdjustSmallTreeCount(-1);
            }
        }
    }

    #region Big Tree
    private void AdjustBigTreeCount(int amount) {
        currentBigTreeCount += amount;
        OnBigTreeCountChanged();
    }
    private void OnBigTreeCountChanged() {
        if (currentBigTreeCount < MaxBigTrees) {
            if (isGeneratingBigTreePerHour == false) {
                isGeneratingBigTreePerHour = true;
                Messenger.AddListener(Signals.HOUR_STARTED, TryGenerateBigTreePerHour);    
            }
        } else {
            isGeneratingBigTreePerHour = false;
            Messenger.RemoveListener(Signals.HOUR_STARTED, TryGenerateBigTreePerHour);
        }
    }
    private void TryGenerateBigTreePerHour() {
        if (Random.Range(0, 100) < 10) {
            CreateNewBigTree();
        }
    }
    private bool CreateNewBigTree() {
        List<LocationGridTile> choices = owner.locationGridTiles.Where(x => x.isOccupied == false 
                                && x.structure.structureType.IsOpenSpace() && BigTreeObject.CanBePlacedOnTile(x)).ToList();
        if (choices.Count > 0) {
            LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(choices);
            chosenTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.BIG_TREE_OBJECT),
                chosenTile);
            return true;
        }
        return false;
    }
    #endregion
    
    #region Small Tree
    private void AdjustSmallTreeCount(int amount) {
        currentSmallTreeCount += amount;
        OnSmallTreeCountChanged();
    }
    private void OnSmallTreeCountChanged() {
        if (currentSmallTreeCount < MaxSmallTrees) {
            if (isGeneratingSmallTreePerHour == false) {
                isGeneratingSmallTreePerHour = true;
                Messenger.AddListener(Signals.HOUR_STARTED, TryGenerateSmallTreePerHour);    
            }
        } else {
            isGeneratingSmallTreePerHour = false;
            Messenger.RemoveListener(Signals.HOUR_STARTED, TryGenerateSmallTreePerHour);
        }
    }
    private void TryGenerateSmallTreePerHour() {
        if (Random.Range(0, 100) < 10) {
            CreateNewSmallTree();
        }
    }
    private bool CreateNewSmallTree() {
        List<LocationGridTile> choices = owner.locationGridTiles.Where(x => x.isOccupied == false 
                                                                            && x.structure.structureType.IsOpenSpace()
                                                                            && x.groundType != LocationGridTile.Ground_Type.Bone).ToList();
        if (choices.Count > 0) {
            LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(choices);
            chosenTile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.TREE_OBJECT),
                chosenTile);
            return true;
        }
        return false;
    }
    #endregion

    
}