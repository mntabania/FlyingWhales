﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine.Serialization;

[System.Serializable]
public class SaveDataBaseSettlement : SaveData<BaseSettlement>, ISavableCounterpart {
    public string _persistentID;
    public int id;
    public LOCATION_TYPE locationType;
    public string name;
    public List<Point> tileCoordinates;
    public string factionOwnerID;
    public List<string> residents;

    public string persistentID => _persistentID;
    public OBJECT_TYPE objectType => OBJECT_TYPE.Settlement;

    public override void Save(BaseSettlement baseSettlement) {
        _persistentID = baseSettlement.persistentID;
        id = baseSettlement.id;
        locationType = baseSettlement.locationType;
        name = baseSettlement.name;

        residents = SaveUtilities.ConvertSavableListToIDs(baseSettlement.residents); 

        factionOwnerID = baseSettlement.owner != null ? baseSettlement.owner.persistentID : string.Empty;
        
        tileCoordinates = new List<Point>();
        for (int i = 0; i < baseSettlement.tiles.Count; i++) {
            HexTile tile = baseSettlement.tiles[i];
            tileCoordinates.Add(new Point(tile.xCoordinate, tile.yCoordinate));
        }
    }
    
}
