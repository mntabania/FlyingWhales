using UnityEngine;
using System.Collections;
using Inner_Maps;

public struct LogFiller {
	public object obj;
	public string value;
	public LOG_IDENTIFIER identifier;

	public LogFiller(object obj, string value, LOG_IDENTIFIER identifier){
		this.obj = obj;
		this.value = value;
		this.identifier = identifier;
	}
}

[System.Serializable]
public struct SaveDataLogFiller {
    public string objID;
    public string objIdentifier;
    public TILE_OBJECT_TYPE objTileObjectType;
    public string value;
    public LOG_IDENTIFIER identifier;

    public void Save(LogFiller filler) {
        if(filler.obj != null) {
            if(filler.obj is Character) {
                objID = (filler.obj as Character).persistentID;
                objIdentifier = "character";
            }else if (filler.obj is NPCSettlement) {
                objID = (filler.obj as NPCSettlement).persistentID;
                objIdentifier = "npcSettlement";
            } else if (filler.obj is Region) {
                objID = (filler.obj as Region).persistentID;
                objIdentifier = "region";
            }
            //else if (filler.obj is BaseLandmark) {
            //    objID = (filler.obj as BaseLandmark).persistentID;
            //    objIdentifier = "landmark";
            //} 
            else if (filler.obj is Faction) {
                objID = (filler.obj as Faction).persistentID;
                objIdentifier = "faction";
            } 
            // else if (filler.obj is SpecialToken) {
            //     objID = (filler.obj as SpecialToken).id;
            //     objIdentifier = "item";
            // }
            // else if (filler.obj is SpecialObject) {
            //     objID = (filler.obj as SpecialObject).id;
            //     objIdentifier = "special object";
            // } 
            else if (filler.obj is TileObject) {
                objID = (filler.obj as TileObject).persistentID;
                objIdentifier = "tile object";
                objTileObjectType = (filler.obj as TileObject).tileObjectType;
            }
        } else {
            objID = string.Empty;
        }

        value = filler.value;
        identifier = filler.identifier;
    }

    public LogFiller Load() {
        LogFiller filler = new LogFiller() {
            value = value,
            identifier = identifier,
            obj = null,
        };

        if(!string.IsNullOrEmpty(objID)) {
            LogFiller tempFiller = filler;
            if (objIdentifier == "character") {
                tempFiller.obj = CharacterManager.Instance.GetCharacterByPersistentID(objID);
            } else if (objIdentifier == "npcSettlement") {
                tempFiller.obj = LandmarkManager.Instance.GetSettlementByPersistentID(objID);
            } else if (objIdentifier == "region") {
                tempFiller.obj = GridMap.Instance.mainRegion;
            } 
            //else if (objIdentifier == "landmark") {
            //    tempFiller.obj = LandmarkManager.Instance.GetLandmarkByID(objID);
            //} 
            else if (objIdentifier == "faction") {
                tempFiller.obj = FactionManager.Instance.GetFactionByPersistentID(objID);
            } 
            // else if (objIdentifier == "item") {
            //     tempFiller.obj = TokenManager.Instance.GetSpecialTokenByID(objID);
            // } else if (objIdentifier == "special object") {
            //     tempFiller.obj = TokenManager.Instance.GetSpecialObjectByID(objID);
            // }
            else if (objIdentifier == "tile object") {
                tempFiller.obj = InnerMapManager.Instance.GetTileObjectByPersistentID(objID);
            }
            filler = tempFiller;
        }
        return filler;
    }
}
