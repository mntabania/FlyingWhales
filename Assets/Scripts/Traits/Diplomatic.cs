﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diplomatic : Trait {
    public Diplomatic() {
        name = "Diplomatic";
        description = "This character is peaceful.";
        type = TRAIT_TYPE.BUFF;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
    }

    #region Overrides
    public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        if (targetPOI is Character) {
            Character targetCharacter = targetPOI as Character;
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                if(targetCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT && targetCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.BERSERKED
                    && targetCharacter.faction == characterThatWillDoJob.faction && targetCharacter.role.roleType != CHARACTER_ROLE.BEAST 
                    && !targetCharacter.returnedToLife && targetCharacter.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.ENEMY, false)) {
                    if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.RESOLVE_CONFLICT)) {
                        GoapPlanJob resolveConflictJob = new GoapPlanJob(JOB_TYPE.RESOLVE_CONFLICT, INTERACTION_TYPE.RESOLVE_CONFLICT, targetCharacter);
                        characterThatWillDoJob.jobQueue.AddJobInQueue(resolveConflictJob);
                    }
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
    }
    #endregion
}
