using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

[System.Serializable]
public class SaveDataWorldObject {
    public int id;
    public WORLD_OBJECT_TYPE type;
    public TILE_OBJECT_TYPE tileObjectType;

    public void Save(IWorldObject worldObject) {
        id = worldObject.id;
        type = worldObject.worldObjectType;
        if(worldObject is Artifact) {
            tileObjectType = (worldObject as Artifact).tileObjectType;
        }
    }

    public IWorldObject Load() {
        // if(type == WORLD_OBJECT_TYPE.ARTIFACT) {
        //      return InnerMapManager.Instance.GetTileObject(tileObjectType, id) as Artifact;
        // } else
        if (type == WORLD_OBJECT_TYPE.SUMMON) {
            return CharacterManager.Instance.GetCharacterByID(id) as Summon;
        } 
        // else if (type == WORLD_OBJECT_TYPE.SPECIAL_OBJECT) {
        //     return TokenManager.Instance.GetSpecialObjectByID(id);
        // }
        return null;
    }
}

public class SaveDataArtifact : SaveDataTileObject {
    public ARTIFACT_TYPE artifactType;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Artifact artifact = tileObject as Artifact;
        artifactType = artifact.type;
    }

    public override TileObject Load() {
        Artifact artifact = base.Load() as Artifact;
        return artifact;
    }
}

public class SaveDataSummon {
    public int id;

    public void Save(IWorldObject worldObject) {
        //base.Save(worldObject);
        if (worldObject is Summon) {
            Summon summon = worldObject as Summon;
            id = summon.id;
        }
    }

    public IWorldObject Load() {
        Summon summon = CharacterManager.Instance.GetCharacterByID(id) as Summon;
        return summon;
    }
}
