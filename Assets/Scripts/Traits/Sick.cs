﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sick : Trait {
    private Character _sourceCharacter;
    //private GoapPlanJob _removeTraitJob;

    public Sick() {
        name = "Sick";
        description = "This character is sick.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 480;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CURE_CHARACTER, };
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if (sourceCharacter is Character) {
            _sourceCharacter = sourceCharacter as Character;
            _sourceCharacter.AdjustSpeedModifier(-0.10f);
            //_sourceCharacter.CreateRemoveTraitJob(name);
            _sourceCharacter.AddTraitNeededToBeRemoved(this);
            _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter, Character removedBy) {
        _sourceCharacter.AdjustSpeedModifier(0.10f);
        //if (_removeTraitJob != null) {
        //    _removeTraitJob.jobQueueParent.CancelJob(_removeTraitJob);
        //}
        _sourceCharacter.CancelAllJobsTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name);
        _sourceCharacter.RemoveTraitNeededToBeRemoved(this);
        _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
        base.OnRemoveTrait(sourceCharacter, removedBy);
        //Messenger.Broadcast(Signals.OLD_NEWS_TRIGGER, sourceCharacter, gainedFromDoing);
    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        Character targetCharacter = traitOwner as Character;
        if (targetCharacter.isDead || !characterThatWillDoJob.isAtHomeArea) {
            return false;
        }
        if (targetCharacter.GetTraitOf(TRAIT_TYPE.CRIMINAL) == null && CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter, null)) {
            if (!targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name)) {
                if (CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter, null)) {
                    GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = targetCharacter };
                    GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect);
                    //job.SetCanTakeThisJobChecker(CanCharacterTakeRemoveTraitJob);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    return true;
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    #endregion
}
