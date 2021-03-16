using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
public class DemonSnatchPartyQuest : PartyQuest {
    public Character targetCharacter { get; private set; }
    public DemonicStructure dropStructure { get; private set; }

    //public bool isSnatching { get; private set; }

    #region getters
    public override IPartyQuestTarget target => targetCharacter;
    public override System.Type serializedData => typeof(SaveDataDemonSnatchPartyQuest);
    public override bool waitingToWorkingStateImmediately => true;
    #endregion

    public DemonSnatchPartyQuest() : base(PARTY_QUEST_TYPE.Demon_Snatch) {
        minimumPartySize = 1;
        relatedBehaviour = typeof(DemonSnatchBehaviour);
    }
    public DemonSnatchPartyQuest(SaveDataDemonSnatchPartyQuest data) : base(data) {
        //isSnatching = data.isSnatching;
    }

    #region Overrides
    public override void OnAcceptQuest(Party partyThatAcceptedQuest) {
        base.OnAcceptQuest(partyThatAcceptedQuest);
        Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfSnatchJobIsFinished);
        Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
        Messenger.AddListener<Prisoner>(TraitSignals.HAS_BECOME_PRISONER, OnHasBecomePrisoner);
        dropStructure.SetPreOccupiedBy(targetCharacter);
    }
    protected override void OnEndQuest() {
        base.OnEndQuest();
        Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
        Messenger.RemoveListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfSnatchJobIsFinished);
        Messenger.RemoveListener<Prisoner>(TraitSignals.HAS_BECOME_PRISONER, OnHasBecomePrisoner);
        dropStructure.SetPreOccupiedBy(null);
    }
    public override IPartyTargetDestination GetTargetDestination() {
        if(targetCharacter.currentStructure != null && targetCharacter.currentStructure.structureType != STRUCTURE_TYPE.WILDERNESS) {
            return targetCharacter.currentStructure;
        } else if(targetCharacter.gridTileLocation != null) {
            return targetCharacter.areaLocation;
        }
        return base.GetTargetDestination();
    }
    public override string GetPartyQuestTextInLog() {
        return "Snatch " + targetCharacter.name;
    }
    #endregion

    #region General
    public void SetTargetCharacter(Character character) {
        targetCharacter = character;
    }
    public void SetDropStructure(DemonicStructure p_structure) {
        dropStructure = p_structure;
    }
    //public void SetIsSnatching(bool state) {
    //    isSnatching = state;
    //}
    private void CheckIfSnatchJobIsFinished(Character p_character, GoapPlanJob p_job) {
        if (p_job.jobType == JOB_TYPE.SNATCH && p_job.poiTarget == targetCharacter) {
            SetIsSuccessful(true);
            //if (!dropStructure.hasBeenDestroyed) {
            //    LocationGridTile chosenTile = dropStructure.GetRandomPassableTile();
            //    if (chosenTile != null) {
            //        CharacterManager.Instance.Teleport(targetCharacter, chosenTile);
            //    }
            //}
            //EndQuest("Finished quest");
        }
    }
    private void OnUnseizePOI(IPointOfInterest poi) {
        if (poi == targetCharacter && assignedParty != null) {
            for (int i = 0; i < assignedParty.membersThatJoinedQuest.Count; i++) {
                Character member = assignedParty.membersThatJoinedQuest[i];
                if (member.currentJob != null && member.currentJob.isThisAPartyJob
                    && (member.currentJob.jobType == JOB_TYPE.PARTY_GO_TO || member.currentJob.jobType == JOB_TYPE.GO_TO)) {
                    member.currentJob.CancelJob(false);
                }
            }
        }
    }
    private void OnHasBecomePrisoner(Prisoner p_prisoner) {
        if (p_prisoner.owner == targetCharacter && assignedParty != null) {
            for (int i = 0; i < assignedParty.membersThatJoinedQuest.Count; i++) {
                Character member = assignedParty.membersThatJoinedQuest[i];
                if (member.currentJob != null && member.currentJob.isThisAPartyJob
                    && (member.currentJob.jobType == JOB_TYPE.SNATCH_RESTRAIN || member.currentJob.jobType == JOB_TYPE.GO_TO || member.currentJob.jobType == JOB_TYPE.PARTY_GO_TO)) {
                    member.currentJob.CancelJob(false);
                }
            }
        }
    }
    #endregion


    #region Loading
    public override void LoadReferences(SaveDataPartyQuest data) {
        base.LoadReferences(data);
        if (data is SaveDataDemonSnatchPartyQuest subData) {
            if (!string.IsNullOrEmpty(subData.targetCharacter)) {
                targetCharacter = CharacterManager.Instance.GetCharacterByPersistentID(subData.targetCharacter);
            }
            if (!string.IsNullOrEmpty(subData.dropStructure)) {
                dropStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(subData.dropStructure) as DemonicStructure;
            }
            //if (isWaitTimeOver && !isDisbanded) {
            //    Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            //}
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataDemonSnatchPartyQuest : SaveDataPartyQuest {
    public string targetCharacter;
    public string dropStructure;
    //public bool isSnatching;

    #region Overrides
    public override void Save(PartyQuest data) {
        base.Save(data);
        if (data is DemonSnatchPartyQuest subData) {
            //isSnatching = subData.isSnatching;
            if (subData.targetCharacter != null) {
                targetCharacter = subData.targetCharacter.persistentID;
            }
            if (subData.dropStructure != null) {
                dropStructure = subData.dropStructure.persistentID;
            }
        }
    }
    #endregion
}