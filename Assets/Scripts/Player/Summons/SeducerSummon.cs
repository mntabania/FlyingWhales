using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine;

public class SeducerSummon : Summon {

    public int seduceChance;
    private List<Character> doneCharacters; //list of characters that the succubus has invited to make love with, regardless of success
    public override int ignoreHostility {
        get {
            if (currentActionNode != null && currentActionNode.action.goapType.IsHostileAction()) {
                return 0; //allow hostility checking
            } else if (stateComponent.currentState != null && (stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT || stateComponent.currentState.characterState == CHARACTER_STATE.BERSERKED)) {
                return 0; //if in combat or berserked state allow hostility checking
            }
            return 1; //default is that succubi won't be seen as hostiles.
        }
    }


    public SeducerSummon(SUMMON_TYPE type, GENDER gender, string className) : base(type, CharacterRole.MINION, className, RACE.DEMON, gender) {
        seduceChance = 25;
        doneCharacters = new List<Character>();
        //AddInteractionType(INTERACTION_TYPE.INVITE);
        //AddInteractionType(INTERACTION_TYPE.MAKE_LOVE);
    }
    public SeducerSummon(SaveDataCharacter data) : base(data) {
        seduceChance = 25;
        doneCharacters = new List<Character>();
    }


    #region Overrides
    //public override void UnsubscribeSignals() {
    //    base.UnsubscribeSignals();
    //    Messenger.RemoveListener(Signals.TICK_STARTED, DailyGoapPlanGeneration);
    //}
    public override void OnPlaceSummon(LocationGridTile tile) {
        base.OnPlaceSummon(tile);
        //Messenger.AddListener(Signals.TICK_STARTED, PerTickGoapPlanGeneration);
        AdjustIgnoreHostilities(1);
    }
    //public override List<ActualGoapNode> ThisCharacterSaw(IPointOfInterest target) {
    //    if (traitContainer.GetNormalTrait<Trait>("Unconscious", "Resting") != null) {
    //        return null;
    //    }
    //    for (int i = 0; i < traitContainer.allTraits.Count; i++) {
    //        traitContainer.allTraits[i].OnSeePOI(target, this);
    //    }
    //    return null;
    //}
    // protected override void OnTickStarted() {
        // if (hasSucceeded) {
        //     //disappear
        //     Disappear();
        // } else if (!jobQueue.HasJob(JOB_TYPE.SEDUCE)) {
        //     //pick a random character that is sexually compatible with this character, to seduce. Exclude characters that this succubus has already invited.
        //     List<Character> choices = currentRegion.charactersAtLocation.Where(x => x.faction != this.faction
        //     && !doneCharacters.Contains(x)
        //     && RelationshipManager.Instance.IsSexuallyCompatibleOneSided(x, this)
        //     && !x.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
        //     && x.IsAvailable()).ToList();
        //     if (choices.Count > 0) {
        //         Character chosenCharacter = choices[Random.Range(0, choices.Count)];
        //         GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SEDUCE, INTERACTION_TYPE.MAKE_LOVE, chosenCharacter, this);
        //         jobQueue.AddJobInQueue(job);
        //     } else {
        //         PlanIdleStrollOutside(); //currentStructure
        //     }
        // }
        
    // }
    public override void OnActionPerformed(ActualGoapNode node) {
        if (node.actor == this && node.goapType == INTERACTION_TYPE.INVITE) {
            doneCharacters.Add(node.poiTarget as Character);
        }
    }
    public override void LevelUp() {
        base.LevelUp();
        seduceChance += 25;
    }
    public override bool CanBeInstructedByPlayer() {
        bool canBeInstructed = base.CanBeInstructedByPlayer();
        if (canBeInstructed) {
            if (ignoreHostility > 0) {
                canBeInstructed = false;
            }
        }
        return canBeInstructed;
    }
    #endregion

    private void Disappear() {
        LocationGridTile disappearTile = gridTileLocation;
        if (stateComponent.currentState != null) {
            stateComponent.ExitCurrentState();
            //This call is doubled so that it will also exit the previous major state if there's any
            //if (stateComponent.currentState != null) {
            //    stateComponent.currentState.OnExitThisState();
            //}
        }
        //else if (stateComponent.currentState != null) {
        //    stateComponent.SetStateToDo(null);
        //}

        if (currentParty.icon.isTravelling) {
            marker.StopMovement();
        }
        //AdjustIsWaitingForInteraction(1);
        //StopCurrentAction(false);
        //AdjustIsWaitingForInteraction(-1);
        currentRegion.RemoveCharacterFromLocation(this);
        PlayerManager.Instance.player.playerSettlement.region.AddCharacterToLocation(this);
        SetRegionLocation(PlayerManager.Instance.player.playerSettlement.region);
        //ownParty.SetSpecificLocation(PlayerManager.Instance.player.playerSettlement);
        //ClearAllAwareness();
        CancelAllJobs();
        traitContainer.RemoveAllNonPersistentTraits(this);
        ResetToFullHP();
        UnsubscribeSignals();
        DestroyMarker(disappearTile);
        SchedulingManager.Instance.ClearAllSchedulesBy(this);
    }
}   

