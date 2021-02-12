using System.Collections;
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
    public List<string> parties;

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
        for (int i = 0; i < baseSettlement.areas.Count; i++) {
            Area tile = baseSettlement.areas[i];
            tileCoordinates.Add(new Point(tile.areaData.xCoordinate, tile.areaData.yCoordinate));
        }

        parties = new List<string>();
        for (int i = 0; i < baseSettlement.parties.Count; i++) {
            Party party = baseSettlement.parties[i];
            parties.Add(party.persistentID);
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(party);
        }
    }
    
}
