using System;
using System.Collections.Generic;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UnityEngine.Tilemaps;
public class SaveDataLocationGridTile : SaveData<LocationGridTile> {
    public Vector3Save localPlace; //this is the id
    public Vector3Save worldLocation;
    public Vector3Save centeredWorldLocation;
    public Vector3Save localLocation;
    public Vector3Save centeredLocalLocation;
    public LocationGridTile.Tile_Type tileType;
    public LocationGridTile.Tile_State tileState;
    public SaveDataTileObject genericTileObjectSave; //TODO:

    //tilemap assets
    public string groundTileMapAssetName;
    public string wallTileMapAssetName;
    public float floorSample;

    public override void Save(LocationGridTile gridTile) {
        localPlace = new Vector3Save(gridTile.localPlace);
        worldLocation = gridTile.worldLocation;
        centeredWorldLocation = gridTile.centeredWorldLocation;
        localLocation = gridTile.localLocation;
        centeredLocalLocation = gridTile.centeredLocalLocation;
        tileType = gridTile.tileType;
        tileState = gridTile.tileState;

        // traits = new List<SaveDataTrait>();
        // for (int i = 0; i < gridTile.normalTraits.Count; i++) {
        //     SaveDataTrait saveDataTrait = SaveManager.ConvertTraitToSaveDataTrait(gridTile.normalTraits[i]);
        //     if (saveDataTrait != null) {
        //         saveDataTrait.Save(gridTile.normalTraits[i]);
        //         traits.Add(saveDataTrait);
        //     }
        // }

        //tilemap assets
        groundTileMapAssetName = gridTile.parentMap.groundTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;
        wallTileMapAssetName = gridTile.parentMap.structureTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;

        floorSample = gridTile.floorSample;
    }

    public LocationGridTile InitialLoad(Tilemap tilemap, InnerTileMap parentAreaMap) {
        LocationGridTile tile = new LocationGridTile(this, tilemap, parentAreaMap);
        tile.CreateGenericTileObject();
        tile.genericTileObject.ManualInitialize(tile);
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
