using Inner_Maps;
using Traits;

public class Triton : Summon {

    public const string ClassName = "Triton";
    
    public override string raceClassName => "Triton";
    public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;
    public override System.Type serializedData => typeof(SaveDataTriton);

    public LocationGridTile spawnLocationTile { get; private set; }

    public Triton() : base(SUMMON_TYPE.Triton, ClassName, RACE.TRITON, UtilityScripts.Utilities.GetRandomGender()) {
    }
    public Triton(string className) : base(SUMMON_TYPE.Triton, className, RACE.TRITON, UtilityScripts.Utilities.GetRandomGender()) {
    }
    public Triton(SaveDataHarpy data) : base(data) {
    }

    #region Overrides
    public override void OnPlaceSummon(LocationGridTile tile) {
        base.OnPlaceSummon(tile);
        if(spawnLocationTile == null) {
            spawnLocationTile = tile.GetNeareastTileFromThisThatMeetCriteria(t => (t.IsPassable() || t.objHere is BlockWall) && t.structure.structureType != STRUCTURE_TYPE.OCEAN, null);
            if(spawnLocationTile == null) {
                spawnLocationTile = tile;
            }
        }
    }
    public override void Initialize() {
        base.Initialize();
        movementComponent.SetEnableDigging(true);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Triton_Behaviour);
    }
    public override void SubscribeToSignals() {
        base.SubscribeToSignals();
        Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
    }
    public override void UnsubscribeSignals() {
        base.UnsubscribeSignals();
        Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
    }
    public override void LoadReferences(SaveDataCharacter data) {
        base.LoadReferences(data);
        if (data is SaveDataTriton savedData) {
            if (!string.IsNullOrEmpty(savedData.spawnLocationTile)) {
                spawnLocationTile = DatabaseManager.Instance.locationGridTileDatabase.GetTileByPersistentID(savedData.spawnLocationTile);
            }
        }
    }
    #endregion

    #region Listeners
    private void OnJobRemovedFromQueue(JobQueueItem job, Character character) {
        if (character == this) {
            if (job.jobType == JOB_TYPE.TRITON_KIDNAP) {
                IPointOfInterest target = job.poiTarget;
                if (target != null) {
                    Prisoner prisoner = target.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                    if (prisoner != null && prisoner.prisonerOfCharacter == this) {
                        target.traitContainer.RemoveRestrainAndImprison(target);
                    }
                }
            }
        }
    }
    #endregion

    public void TriggerTritonKidnap(Character targetCharacter) {
        jobComponent.TriggerTritonKidnap(targetCharacter, spawnLocationTile.structure, spawnLocationTile);
    }
}

[System.Serializable]
public class SaveDataTriton : SaveDataSummon {
    public string spawnLocationTile { get; private set; }

    public override void Save(Character data) {
        base.Save(data);
        if (data is Triton summon) {
            if(summon.spawnLocationTile != null) {
                spawnLocationTile = summon.spawnLocationTile.persistentID;
            }
        }
    }
}