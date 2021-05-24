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
    public bool isStoredAsTarget;
    public List<Point> tileCoordinates;
    public string factionOwnerID;
    public List<string> residents;
    public List<string> parties;

    public string persistentID => _persistentID;
    public OBJECT_TYPE objectType => OBJECT_TYPE.Settlement;

    public override void Save(BaseSettlement data) {
        _persistentID = data.persistentID;
        id = data.id;
        locationType = data.locationType;
        name = data.name;
        isStoredAsTarget = data.isStoredAsTarget;

        residents = SaveUtilities.ConvertSavableListToIDs(data.residents); 

        factionOwnerID = data.owner != null ? data.owner.persistentID : string.Empty;
        
        tileCoordinates = new List<Point>();
        for (int i = 0; i < data.areas.Count; i++) {
            Area area = data.areas[i];
            tileCoordinates.Add(new Point(area.areaData.xCoordinate, area.areaData.yCoordinate));
        }

        parties = new List<string>();
        for (int i = 0; i < data.parties.Count; i++) {
            Party party = data.parties[i];
            parties.Add(party.persistentID);
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(party);
        }
    }
    
}
