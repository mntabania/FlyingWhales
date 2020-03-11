using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class ThiefSummon : Summon {

    private int itemsToSteal; //number of items that the thief will steal.

    #region getters/setters
    public override string worldObjectName {
        get { return $"{name} (Thief)"; }
    }
    #endregion

    public ThiefSummon() : base(SUMMON_TYPE.ThiefSummon, CharacterManager.Instance.GetRandomClassByIdentifier("Normal"), RACE.HUMANS, UtilityScripts.Utilities.GetRandomGender()) {
        itemsToSteal = 1;
        //AddInteractionType(INTERACTION_TYPE.STEAL);
    }
    public ThiefSummon(SaveDataCharacter data) : base(data) {
        itemsToSteal = 1;
    }

    #region Overrides
    public override void OnPlaceSummon(LocationGridTile tile) {
        base.OnPlaceSummon(tile);
        //AddInitialAwareness(tile.parentAreaMap.npcSettlement);
        //Messenger.AddListener(Signals.TICK_STARTED, PerTickGoapPlanGeneration);
        ////add all characters that are not part of the player faction to this character's terrifying characters list, so this character can avoid them.
        //for (int i = 0; i < tile.parentAreaMap.npcSettlement.charactersAtLocation.Count; i++) {
        //    Character currCharacter = tile.parentAreaMap.npcSettlement.charactersAtLocation[i];
        //    if (currCharacter.faction != this.faction) {
        //        marker.AddTerrifyingObject(currCharacter);
        //    }
        //}
    }
    //public override List<ActualGoapNode> ThisCharacterSaw(IPointOfInterest target) {
    //    List<ActualGoapNode> actions = base.ThisCharacterSaw(target);
    //    if (target is Character) {
    //        Character targetCharacter = target as Character;
    //        //if the target is not from the player faction, add them to the terrifying characters list
    //        if (targetCharacter.faction != this.faction) {
    //            marker.AddTerrifyingObject(target);
    //        }
    //    }
    //    return actions;
    //}
    protected override void OnTickStarted() {
        //if (_hasAlreadyAskedForPlan) {
        //    return;
        //}
        //SetHasAlreadyAskedForPlan(true);
        //if (items.Count >= itemsToSteal) {
        //    //if the thief has already reached his/her max number of stolen items, exit the npcSettlement.
        //    LocationGridTile targetTile = GetNearestUnoccupiedEdgeTileFromThis();
        //    marker.GoTo(targetTile, OnReachExitAction);
        //} else {
        //    //if the warehouse still has items in it, try to steal them.
        //    LocationStructure warehouse = specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        //    if (warehouse.itemsInStructure.Count > 0) {
        //        SpecialToken chosenItem = warehouse.itemsInStructure[UnityEngine.Random.Range(0, warehouse.itemsInStructure.Count)];
        //        //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STEAL, INTERACTION_TYPE.STEAL, chosenItem);
        //        //job.SetCannotOverrideJob(true);
        //        //job.SetCannotCancelJob(true);
        //        //jobQueue.AddJobInQueue(job);
        //    } else {
        //        //just enter berserked mode.
        //        stateComponent.SwitchToState(CHARACTER_STATE.STROLL, null, specificLocation);
        //        SetHasAlreadyAskedForPlan(false);
        //    }
        //}
    }
    public override void LevelUp() {
        base.LevelUp();
        itemsToSteal += 1;
    }
    public override bool CanBeInstructedByPlayer() {
        bool canBeInstructed = base.CanBeInstructedByPlayer();
        if (canBeInstructed) {
            if (currentActionNode != null && !currentActionNode.action.goapType.IsHostileAction()) {
                canBeInstructed = false;
            } else if (stateComponent.currentState == null || !(stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT || stateComponent.currentState.characterState == CHARACTER_STATE.BERSERKED)) {
                canBeInstructed = false;
            }
        }
        return canBeInstructed;
    }
    public override bool IsStillConsideredPartOfAwarenessByCharacter(Character character) {
        return marker;
    }
    #endregion

    private void OnReachExitAction() {
        //remove character from other character's awareness
        //for (int i = 0; i < gridTileLocation.parentAreaMap.npcSettlement.charactersAtLocation.Count; i++) {
        //    Character currCharacter = gridTileLocation.parentAreaMap.npcSettlement.charactersAtLocation[i];
        //    currCharacter.RemoveAwareness(this);
        //}

        //marker.ClearTerrifyingObjects();
        currentRegion.RemoveCharacterFromLocation(this);
        DestroyMarker();
        UnsubscribeSignals();
        //ClearAllAwareness();
    }
}
