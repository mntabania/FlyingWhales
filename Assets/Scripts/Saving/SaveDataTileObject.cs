using System.Collections.Generic;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Traits;
using UnityEngine;

[System.Serializable]
public class SaveDataTileObject : SaveData<TileObject>, ISavableCounterpart {
    public string persistentID { get; set; }
    public int id;
    public string name;
    public TILE_OBJECT_TYPE tileObjectType;
    public string characterOwnerID;
    public int regionLocationID;
    public TileLocationSave tileLocationID;
    public bool isPreplaced;
    public string isBeingCarriedByID;
    public POI_STATE poiState;
    public List<INTERACTION_TYPE> advertisedActions;
    public List<string> jobsTargetingThis;
    public List<string> existingJobsTargetingThis;
    public MAP_OBJECT_STATE mapObjectState;
    public bool isDamageContributorToStructure;
    public bool isStoredAsTarget;
    public bool isDeadReference;

    //resources
    public SaveDataResourceStorageComponent resourceStorageComponent;

    //hp
    public int currentHP;
    
    //visuals
    public string spriteName;
    public QuaternionSave rotation;
    
    //Traits
    public SaveDataTraitContainer saveDataTraitContainer;

    //Components
    public SaveDataLogComponent logComponent;
    public SaveDataTileObjectHiddenComponent hiddenComponent;

    #region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Tile_Object;
    #endregion
    
    public override void Save(TileObject data) {
        persistentID = data.persistentID;
        id = data.id;
        name = data.name;
        tileObjectType = data.tileObjectType;
        characterOwnerID = data.characterOwner?.persistentID ?? string.Empty;
        if (data.gridTileLocation != null) {
            regionLocationID = data.gridTileLocation.parentMap.region.id;
            tileLocationID = new TileLocationSave(data.gridTileLocation);    
        } else {
            regionLocationID = -1;
            tileLocationID = new TileLocationSave();
        }
        isPreplaced = data.isPreplaced;
        poiState = data.state;
        isDamageContributorToStructure = data.isDamageContributorToStructure;
        isStoredAsTarget = data.isStoredAsTarget;
        isDeadReference = data.isDeadReference;

        advertisedActions = data.advertisedActions != null ? new List<INTERACTION_TYPE>(data.advertisedActions) : new List<INTERACTION_TYPE>();

        jobsTargetingThis = new List<string>();
        for (int i = 0; i < data.allJobsTargetingThis.Count; i++) {
            JobQueueItem jobQueueItem = data.allJobsTargetingThis[i];
            jobsTargetingThis.Add(jobQueueItem.persistentID);
        }
        
        existingJobsTargetingThis = new List<string>();
        for (int i = 0; i < data.allExistingJobsTargetingThis.Count; i++) {
            JobQueueItem jobQueueItem = data.allExistingJobsTargetingThis[i];
            existingJobsTargetingThis.Add(jobQueueItem.persistentID);
        }
        mapObjectState = data.mapObjectState;
        
        currentHP = data.currentHP;

        if (data.mapObjectVisual == null) {
            spriteName = string.Empty;
            rotation = Quaternion.identity;
        } else {
            spriteName = data.mapObjectVisual.usedSpriteName;
            rotation = data.mapObjectVisual.rotation;
        }

        resourceStorageComponent = new SaveDataResourceStorageComponent();
        resourceStorageComponent.Save(data.resourceStorageComponent);

        isBeingCarriedByID = data.isBeingCarriedBy != null ? data.isBeingCarriedBy.persistentID : string.Empty;
        
        saveDataTraitContainer = new SaveDataTraitContainer();
        saveDataTraitContainer.Save(data.traitContainer);

        logComponent = new SaveDataLogComponent(); logComponent.Save(data.logComponent);
        hiddenComponent = new SaveDataTileObjectHiddenComponent(); hiddenComponent.Save(data.hiddenComponent);
    }
    public override TileObject Load() {
        TileObject tileObject = InnerMapManager.Instance.LoadTileObject<TileObject>(this);
        tileObject.Initialize(this);
        return tileObject;
    }
}