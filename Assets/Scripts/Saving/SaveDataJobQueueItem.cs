using System;
using System.Collections.Generic;
using System.Reflection;

[Serializable]
public abstract class SaveDataJobQueueItem : SaveData<JobQueueItem>, ISavableCounterpart {
    public string _persistentID;
    public OBJECT_TYPE _objectType;
    public int id;

    public string originalOwnerID;
    public OBJECT_TYPE originalOwnerType;

    public string assignedCharacterID;
    
    public string name;
    public JOB_TYPE jobType;
    public bool isStealth;
    public List<string> blacklistedCharacterIDs;

    public bool doNotRecalculate;
    public int invalidCounter;
    public bool isThisAPartyJob;
    public bool isThisAGatheringJob;
    public bool cannotBePushedBack;
    public bool shouldBeRemovedFromSettlementWhenUnassigned;
    public bool forceCancelOnInvalid;

    public string canTakeJobKey;
    public string applicabilityCheckerKey;

    #region getters
    public string persistentID => _persistentID;
    public OBJECT_TYPE objectType => _objectType;
    #endregion
    
    public override void Save(JobQueueItem job) {
        _persistentID = job.persistentID;
        _objectType = job.objectType;
        id = job.id;

        if (job.originalOwner != null) {
            originalOwnerID = job.originalOwner.persistentID;
            originalOwnerType = job.originalOwner.objectType;    
        } else {
            originalOwnerID = string.Empty;
            originalOwnerType = OBJECT_TYPE.Character;
        }
        

        assignedCharacterID = job.assignedCharacter == null ? string.Empty : job.assignedCharacter.persistentID;
        
        name = job.name;
        jobType = job.jobType;
        isStealth = job.isStealth;

        blacklistedCharacterIDs = new List<string>();
        for (int i = 0; i < job.blacklistedCharacters.Count; i++) {
            Character blacklistedCharacter = job.blacklistedCharacters[i];
            blacklistedCharacterIDs.Add(blacklistedCharacter.persistentID);
        }

        doNotRecalculate = job.doNotRecalculate;
        invalidCounter = job.invalidCounter;
        isThisAPartyJob = job.isThisAPartyJob;
        isThisAGatheringJob = job.isThisAGatheringJob;
        cannotBePushedBack = job.cannotBePushedBack;
        shouldBeRemovedFromSettlementWhenUnassigned = job.shouldBeRemovedFromSettlementWhenUnassigned;
        forceCancelOnInvalid = job.forceCancelOnInvalid;

        canTakeJobKey = job.canTakeJobChecker == null ? string.Empty : job.canTakeJobChecker.key;
        applicabilityCheckerKey = job.stillApplicable == null ? string.Empty : job.stillApplicable.key;
    }
    public abstract override JobQueueItem Load();
}