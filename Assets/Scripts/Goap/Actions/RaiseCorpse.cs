
using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;  
using Traits;

public class RaiseCorpse : GoapAction {

    public RaiseCorpse() : base(INTERACTION_TYPE.RAISE_CORPSE) {
        actionIconString = GoapActionStateDB.Magic_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON, RACE.DEMON, RACE.RATMAN };
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        logTags = new[] {LOG_TAG.Work, LOG_TAG.Life_Changes};
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.RAISE_CORPSE, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Raise Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        string stateName = "Target Missing";
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(false, stateName);
        //raise corpse cannot be invalid because all cases are handled by the requirements of the action
        return goapActionInvalidity;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        IPointOfInterest targetPOI = node.poiTarget;
        Character target = null;
        if (targetPOI is Character) {
            target = targetPOI as Character;
        } else if (targetPOI is Tombstone) {
            target = (targetPOI as Tombstone).character;
        }
        if(target != null) {
            log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget is Character targetCharacter) {
                return targetCharacter.isDead && !(targetCharacter is Summon) && !targetCharacter.hasBeenRaisedFromDead && targetCharacter.hasMarker;
            } else if (poiTarget is Tombstone tombstone) {
                return tombstone.gridTileLocation != null && tombstone.mapObjectVisual && !(tombstone.character is Summon) && !tombstone.character.hasBeenRaisedFromDead && tombstone.character.hasMarker;
            }
        }
        return false;
    }
#endregion

#region State Effects
    public void AfterRaiseSuccess(ActualGoapNode goapNode) {
        IPointOfInterest targetPOI = goapNode.poiTarget;
        Character target = null;
        if (targetPOI is Character) {
            target = targetPOI as Character;
        } else if (targetPOI is Tombstone) {
            target = (targetPOI as Tombstone).character;
        }
        if (target != null && target.hasMarker) {
            Summon summon = CharacterManager.Instance.RaiseFromDeadReplaceCharacterWithSkeleton(target, goapNode.actor.faction);
            Messenger.Broadcast(CharacterSignals.ON_CHARACTER_RAISE_DEAD_BY_NECRO, summon);
        } else {
            Debug.LogWarning($"Could not raise {target?.name} because it's marker is null!");
        }
    }
#endregion

}