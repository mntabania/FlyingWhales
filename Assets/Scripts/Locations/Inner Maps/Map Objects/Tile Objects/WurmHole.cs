using System.Collections.Generic;
using UnityEngine.Assertions;

public class WurmHole : TileObject{
    public WurmHole wurmHoleConnection { get; private set; }

    #region getters
    public override System.Type serializedData => typeof(SaveDataWurmHole);
    #endregion

    public WurmHole() {
        Initialize(TILE_OBJECT_TYPE.WURM_HOLE);
        traitContainer.AddTrait(this, "Indestructible");
        traitContainer.AddTrait(this, "Fireproof");
    }
    public WurmHole(SaveDataWurmHole data) { }

    public void SetWurmHoleConnection(WurmHole wurmHole) {
        wurmHoleConnection = wurmHole;
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        if(wurmHoleConnection.gridTileLocation != null) {
            wurmHoleConnection.gridTileLocation.structure.RemovePOI(wurmHoleConnection);
        }
    }

    public void TravelThroughWurmHole(Character character) {
        character.movementComponent.SetCameFromWurmHole(true);
        CharacterManager.Instance.Teleport(character, wurmHoleConnection.gridTileLocation);
        Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, character as IPointOfInterest, "");
        character.jobQueue.CancelAllJobs();
        character.combatComponent.ClearHostilesInRange();
        character.combatComponent.ClearAvoidInRange();
    }

    #region Loading
    public override void LoadSecondWave(SaveDataTileObject data) {
        base.LoadSecondWave(data);
        if(data is SaveDataWurmHole subData) {
            if (!string.IsNullOrEmpty(subData.wurmHoleConnection)) {
                wurmHoleConnection = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(subData.wurmHoleConnection) as WurmHole;
            }
        }
    }
    #endregion
}

#region Save Data
[System.Serializable]
public class SaveDataWurmHole : SaveDataTileObject {
    public string wurmHoleConnection;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        WurmHole wurmHole = tileObject as WurmHole;
        Assert.IsNotNull(wurmHole);
        wurmHoleConnection = wurmHole.wurmHoleConnection.persistentID;
        //Moved this to SaveDataCurrentProgress.SaveTileObjectsCoroutine() because this will produce an InvalidCastException and a StackOverflowException
        // SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub<TileObject>(wurmHole.wurmHoleConnection); 
    }
}
#endregion