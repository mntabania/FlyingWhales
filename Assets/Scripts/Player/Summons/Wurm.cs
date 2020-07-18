using System;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;
using UtilityScripts;
using Random = UnityEngine.Random;

public class Wurm : Summon {
    public override string raceClassName => characterClass.className;
    public Wurm() : base(SUMMON_TYPE.Wurm, "Wurm", RACE.WURM,
        UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
    }
    public Wurm(string className) : base(SUMMON_TYPE.Wurm, className, RACE.WURM,
        UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
    }
    public Wurm(SaveDataCharacter data) : base(data) {
        visuals.SetHasBlood(false);
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        if (!traitContainer.HasTrait("Tower")) {
            reactionComponent.SetIsHidden(true);
        }
        movementComponent.SetIsStationary(true);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Wurm_Behaviour);
    }
    #endregion

    //Whenever the wurm has a job, it must not be hidden, but it doesn't have any jobs at all, return back to being hidden
    public override void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character) {
        if(character == this) {
            reactionComponent.SetIsHidden(false);
        }
    }
    public override void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character) {
        if (character == this) {
            if(character.jobQueue.jobsInQueue.Count <= 0){
                if (!traitContainer.HasTrait("Tower")) {
                    reactionComponent.SetIsHidden(true);
                }
            }
        }
    }

    public override void OnSetIsHidden() {
        base.OnSetIsHidden();
        for (int i = 1; i < 5; i++) {
            Sprite oldIdleSprite = visuals.GetMarkerAnimationSprite("idle_" + i);
            Sprite newIdleSprite = visuals.GetMarkerAnimationSprite("hidden_" + i);
            visuals.ChangeMarkerAnimationSprite("idle_" + i, newIdleSprite);
            visuals.ChangeMarkerAnimationSprite("hidden_" + i, oldIdleSprite);
        }
    }
}
