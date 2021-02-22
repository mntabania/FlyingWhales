using System;
using System.Collections.Generic;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
public class SaveDataLocationGridTile : SaveData<LocationGridTile> {
    public string persistentID;
    public bool isDefault;
    public Vector3Save localPlace;
    public Vector3Save worldLocation;
    public Vector3Save centeredWorldLocation;
    public Vector3Save localLocation;
    public Vector3Save centeredLocalLocation;
    public LocationGridTile.Tile_Type tileType;
    public LocationGridTile.Tile_State tileState;
    public string genericTileObjectID;
    public bool hasLandmine;
    public bool hasFreezingTrap;
    public bool hasSnareTrap;
    public int meteorCount;
    public int connectorsCount;
    public List<RACE> freezingTrapExclusions;

    //tilemap assets
    public string groundTileMapAssetName;
    public string wallTileMapAssetName;
    public float floorSample;

    //Components
    public SaveDataGridTileCorruptionComponent corruptionComponent;
    public SaveDataGridTileMouseEventsComponent mouseEventsComponent;
    public SaveDataGridTileTileObjectComponent tileObjectComponent;

    public override void Save(LocationGridTile gridTile) {
        persistentID = gridTile.persistentID;
        isDefault = gridTile.isDefault;
        Assert.IsFalse(isDefault, $"{gridTile} is being saved but its default toggle is on!");
        localPlace = new Vector3Save(gridTile.localPlace);
        worldLocation = gridTile.worldLocation;
        centeredWorldLocation = gridTile.centeredWorldLocation;
        localLocation = gridTile.localLocation;
        centeredLocalLocation = gridTile.centeredLocalLocation;
        tileType = gridTile.tileType;
        tileState = gridTile.tileState;
        hasLandmine = gridTile.hasLandmine;
        hasFreezingTrap = gridTile.hasFreezingTrap;
        hasSnareTrap = gridTile.hasSnareTrap;
        meteorCount = gridTile.meteorCount;
        connectorsCount = gridTile.connectorsOnTile;
        freezingTrapExclusions = gridTile.freezingTrapExclusions;
        genericTileObjectID = gridTile.genericTileObject.persistentID;

        //tilemap assets
        groundTileMapAssetName = gridTile.parentMap.groundTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;
        wallTileMapAssetName = gridTile.parentMap.structureTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;

        floorSample = gridTile.floorSample;

        corruptionComponent = new SaveDataGridTileCorruptionComponent(); corruptionComponent.Save(gridTile.corruptionComponent);
        mouseEventsComponent = new SaveDataGridTileMouseEventsComponent(); mouseEventsComponent.Save(gridTile.mouseEventsComponent);
        tileObjectComponent = new SaveDataGridTileTileObjectComponent(); tileObjectComponent.Save(gridTile.tileObjectComponent);
    }

    public LocationGridTile InitialLoad(Tilemap tilemap, InnerTileMap parentAreaMap, SaveDataCurrentProgress saveData, Area p_area) {
        LocationGridTile tile = new LocationGridTile(this, tilemap, parentAreaMap, p_area);
        tile.SetFloorSample(floorSample);
        SaveDataTileObject saveDataTileObject = saveData.GetFromSaveHub<SaveDataTileObject>(OBJECT_TYPE.Tile_Object, genericTileObjectID);
        TileObject loadedObject = saveDataTileObject.Load();
        GenericTileObject genericTileObject = loadedObject as GenericTileObject;
        Assert.IsNotNull(genericTileObject);
        genericTileObject.SetTileOwner(tile);
        genericTileObject.ManualInitializeLoad(tile, saveDataTileObject);
        tile.LoadGenericTileObject(genericTileObject);
        return tile;
    }

    // public void LoadTraits() {
        // for (int i = 0; i < traits.Count; i++) {
        //     Character responsibleCharacter = null;
        //     Trait trait = traits[i].Load(ref responsibleCharacter);
        //     loadedGridTile.genericTileObject.traitContainer.AddTrait(loadedGridTile.genericTileObject, trait, responsibleCharacter);
        // }
    // }
}
