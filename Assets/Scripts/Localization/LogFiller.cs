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
    public int objID;
    public string objIdentifier;
    public TILE_OBJECT_TYPE objTileObjectType;
    public string value;
    public LOG_IDENTIFIER identifier;

    public void Save(LogFiller filler) {
        if(filler.obj != null) {
            if(filler.obj is Character) {
                objID = (filler.obj as Character).id;
                objIdentifier = "character";
            }else if (filler.obj is NPCSettlement) {
                objID = (filler.obj as NPCSettlement).id;
                objIdentifier = "npcSettlement";
            } else if (filler.obj is Region) {
                objID = (filler.obj as Region).id;
                objIdentifier = "region";
            } else if (filler.obj is BaseLandmark) {
                objID = (filler.obj as BaseLandmark).id;
                objIdentifier = "landmark";
            } else if (filler.obj is Faction) {
                objID = (filler.obj as Faction).id;
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
                objID = (filler.obj as TileObject).id;
                objIdentifier = "tile object";
                objTileObjectType = (filler.obj as TileObject).tileObjectType;
            }
        } else {
            objID = -1;
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

        if(objID != -1) {
            LogFiller tempFiller = filler;
            if (objIdentifier == "character") {
                tempFiller.obj = CharacterManager.Instance.GetCharacterByID(objID);
            } else if (objIdentifier == "npcSettlement") {
                tempFiller.obj = LandmarkManager.Instance.GetAreaByID(objID);
            } else if (objIdentifier == "region") {
                tempFiller.obj = GridMap.Instance.GetRegionByID(objID);
            } else if (objIdentifier == "landmark") {
                tempFiller.obj = LandmarkManager.Instance.GetLandmarkByID(objID);
            } else if (objIdentifier == "faction") {
                tempFiller.obj = FactionManager.Instance.GetFactionBasedOnID(objID);
            } 
            // else if (objIdentifier == "item") {
            //     tempFiller.obj = TokenManager.Instance.GetSpecialTokenByID(objID);
            // } else if (objIdentifier == "special object") {
            //     tempFiller.obj = TokenManager.Instance.GetSpecialObjectByID(objID);
            // }
            else if (objIdentifier == "tile object") {
                tempFiller.obj = InnerMapManager.Instance.GetTileObject(objTileObjectType, objID);
            }
            filler = tempFiller;
        }
        return filler;
    }
}
