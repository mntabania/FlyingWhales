using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class HuntState : CharacterState {

    public HuntState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Hunt State";
        characterState = CHARACTER_STATE.HUNT;
        //stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 48;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartHuntMovement();
    }
    public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character targetCharacter = targetPOI as Character;
            if (targetCharacter.isDead) {
                //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.EAT_CORPSE, stateComponent.character, targetPOI);
                //if (goapAction.targetTile != null) {
                //    SetCurrentlyDoingAction(goapAction);
                //    goapAction.CreateStates();
                //    stateComponent.character.SetCurrentActionNode(goapAction);
                //    stateComponent.character.marker.GoTo(goapAction.targetTile, OnArriveAtCorpseLocation);
                //} else {
                //    Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + stateComponent.character.name + " can't eat corpse " + targetPOI.name + " because there is no tile to go to!");
                //}
            } else {
                //stateComponent.character.combatComponent.AddHostileInRange(targetCharacter);
            }
           
            return true;
        }
        //else if (targetPOI is Corpse) {
        //    GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.EAT_CORPSE, stateComponent.character, targetPOI);
        //    if (goapAction.targetTile != null) {
        //        goapAction.CreateStates();
        //        stateComponent.character.SetCurrentAction(goapAction);
        //        stateComponent.character.marker.GoTo(goapAction.targetTile, targetPOI, () => OnArriveAtCorpseLocation());
        //    } else {
        //        Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + stateComponent.character.name + " can't eat corpse " + targetPOI.name + " because there is no tile to go to!");
        //    }
        //    return true;
        //}
        return base.OnEnterVisionWith(targetPOI);
    }
    #endregion

    //private void OnArriveAtCorpseLocation() {
    //    if (stateComponent.character.currentActionNode == null) {
    //        Debug.LogWarning(GameManager.Instance.TodayLogString() + stateComponent.character.name + " arrived at corpse location during " + stateName + ", but current action is null");
    //        return;
    //    }
    //    stateComponent.character.currentActionNode.SetEndAction(HuntAgain);
    //    stateComponent.character.currentActionNode.Perform();
    //}
    //private void HuntAgain(string result, GoapAction goapAction) {
    //    //SetCurrentlyDoingAction(null);
    //    if (stateComponent.currentState != this) {
    //        return;
    //    }
    //    stateComponent.character.SetCurrentActionNode(null, null, null);
    //    StartHuntMovement();
    //}
    private void StartHuntMovement() {
        stateComponent.character.marker.GoTo(PickRandomTileToGoTo(), StartHuntMovement);
    }
    private LocationGridTile PickRandomTileToGoTo() {
        LocationStructure chosenStructure = stateComponent.character.currentRegion.GetRandomStructure();
        LocationGridTile chosenTile = chosenStructure.GetRandomTile();
        if (chosenTile != null) {
            return chosenTile;
        } else {
            throw new System.Exception(
                $"No tile in {chosenStructure.name} for {stateComponent.character.name} to go to in {stateName}");
        }
    }
}
