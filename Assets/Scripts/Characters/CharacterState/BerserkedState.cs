using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;

public class BerserkedState : CharacterState {

    private System.Func<Character, bool> hostileChecker;
    public bool areCombatsLethal { get; private set; }

    public BerserkedState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Berserked State";
        characterState = CHARACTER_STATE.BERSERKED;
        //stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 100;
        SetAreCombatsLethal(true);
    }

    #region Overrides
    protected override void StartState() {
        stateComponent.character.needsComponent.AdjustDoNotGetHungry(1);
        stateComponent.character.needsComponent.AdjustDoNotGetBored(1);
        stateComponent.character.needsComponent.AdjustDoNotGetTired(1);
        //stateComponent.character.traitContainer.AddTrait(stateComponent.character, "Berserked");
        //BerserkBuff berserkBuff = new BerserkBuff();
        //berserkBuff.SetLevel(level);
        //stateComponent.character.traitContainer.AddTrait(stateComponent.character, berserkBuff);
        base.StartState();
    }
    protected override void EndState() {
        stateComponent.character.needsComponent.AdjustDoNotGetHungry(-1);
        stateComponent.character.needsComponent.AdjustDoNotGetBored(-1);
        stateComponent.character.needsComponent.AdjustDoNotGetTired(-1);
        //stateComponent.character.needsComponent.AdjustHappiness(50);
        //stateComponent.character.needsComponent.AdjustTiredness(50);
        base.EndState();
        //stateComponent.character.traitContainer.RemoveTrait(stateComponent.character, "Berserked");
        //stateComponent.character.traitContainer.RemoveTrait(stateComponent.character, "Berserk Buff");
    }
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartBerserkedMovement();
    }
    public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
        //if(targetPOI is Character) {
        //    if (stateComponent.character.faction.isPlayerFaction) {
        //        return stateComponent.character.combatComponent.AddHostileInRange(targetPOI as Character, isLethal: areCombatsLethal); //check hostility if from player faction, so as not to attack other characters that are also from the same faction.
        //    } else {
        //        if (hostileChecker != null) {
        //            if (hostileChecker.Invoke(targetPOI as Character)) {
        //                return stateComponent.character.combatComponent.AddHostileInRange(targetPOI as Character, isLethal: areCombatsLethal);
        //            }
        //        } else {
        //            return stateComponent.character.combatComponent.AddHostileInRange(targetPOI as Character, isLethal: areCombatsLethal);
        //        }
        //    }
        //    //return true;
        //}
        //else if (targetPOI is TileObject) {
        //    TileObject target = targetPOI as TileObject;
        //    if(target.tileObjectType != TILE_OBJECT_TYPE.TREE_OBJECT && target.advertisedActions.Contains(INTERACTION_TYPE.ASSAULT)) {
        //        int chance = UnityEngine.Random.Range(0, 100);
        //        if (chance < 20) {
        //            //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.TILE_OBJECT_DESTROY, stateComponent.character, targetPOI);
        //            //if (goapAction.targetTile != null) {
        //            //    SetCurrentlyDoingAction(goapAction);
        //            //    PauseState();
        //            //    goapAction.CreateStates();
        //            //    goapAction.SetEndAction(BerserkAgain);
        //            //    goapAction.DoAction();
        //            //    //stateComponent.character.SetCurrentAction(goapAction);
        //            //    //stateComponent.character.marker.GoTo(goapAction.targetTile, OnArriveAtLocation);
        //            //} else {
        //            //    Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + stateComponent.character.name + " can't destroy tile object " + targetPOI.name + " because there is no tile to go to!");
        //            //}
        //            return true;
        //        }
        //    }
        //} else if (targetPOI is SpecialToken) {
        //    int chance = UnityEngine.Random.Range(0, 100);
        //    if (chance < 20) {
        //        //GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.ITEM_DESTROY, stateComponent.character, targetPOI);
        //        //if (goapAction.targetTile != null) {
        //        //    goapAction.CreateStates();
        //        //    PauseState();
        //        //    goapAction.SetEndAction(BerserkAgain);
        //        //    goapAction.DoAction();
                    
        //        //    //stateComponent.character.SetCurrentAction(goapAction);
        //        //    //stateComponent.character.marker.GoTo(goapAction.targetTile, OnArriveAtLocation);
                    
        //        //} else {
        //        //    Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + stateComponent.character.name + " can't destroy item " + targetPOI.name + " because there is no tile to go to!");
        //        //}
        //        return true;
        //    }
        //}
        return base.OnEnterVisionWith(targetPOI);
    }
    public override bool ProcessInVisionPOIsOnStartState() {
        //for (int i = 0; i < stateComponent.character.combatComponent.avoidInRange.Count; i++) {
        //    IPointOfInterest hostile = stateComponent.character.combatComponent.avoidInRange[i];
        //    if (hostile is Character) {
        //        Character hostileChar = hostile as Character;
        //        if (stateComponent.character.marker.inVisionCharacters.Contains(hostileChar)) {
        //            stateComponent.character.combatComponent.AddHostileInRange(hostileChar, false, isLethal: areCombatsLethal);
        //        } else {
        //            stateComponent.character.combatComponent.RemoveAvoidInRange(hostile, false);
        //            i--;
        //        }
        //    } else {
        //        stateComponent.character.combatComponent.RemoveAvoidInRange(hostile, false);
        //        i--;
        //    }
            
        //}
        stateComponent.character.combatComponent.ClearAvoidInRange(false);

        bool hasProcessedCombatBehavior = false;
        if (base.ProcessInVisionPOIsOnStartState()) {
            for (int i = 0; i < stateComponent.character.marker.inVisionPOIs.Count; i++) {
                IPointOfInterest poi = stateComponent.character.marker.inVisionPOIs[i];
                if (OnEnterVisionWith(poi)) {
                    if(poi is Character) {
                        hasProcessedCombatBehavior = true;
                    }
                    break;
                }
            }
        }
        if (stateComponent.character.combatComponent.hostilesInRange.Count > 0 && !hasProcessedCombatBehavior) {
            CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.COMBAT, CHARACTER_STATE.COMBAT, stateComponent.character);
            stateComponent.character.jobQueue.AddJobInQueue(job);
            //stateComponent.SwitchToState(CHARACTER_STATE.COMBAT);
        }
        return true;
    }
    //protected override void PerTickInState() {
    //    base.PerTickInState();
    //    if (!isDone) {
    //        if(stateComponent.character.GetTrait("Injured") != null) {
    //            StopStatePerTick();
    //            OnExitThisState();
    //        }
    //    }
    //}
    //public override void AfterExitingState() {
    //    base.AfterExitingState();
    //    Spooked spooked = stateComponent.character.traitContainer.GetNormalTrait<Trait>("Spooked") as Spooked;
    //    if (spooked != null) {
    //        //If has spooked, add them in avoid list and transfer all in engage list to flee list
    //        stateComponent.character.marker.AddAvoidsInRange(spooked.terrifyingCharacters, false);
    //        Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, stateComponent.character);
    //    }
    //}
    #endregion

    private void OnArriveAtLocation() {
        if (stateComponent.character.currentActionNode == null) {
            Debug.LogWarning(
                $"{GameManager.Instance.TodayLogString()}{stateComponent.character.name} arrived at location of item/tile object to be destroyed during {stateName}, but current action is null");
            return;
        }
        //stateComponent.character.currentActionNode.SetEndAction(BerserkAgain);
        //stateComponent.character.currentActionNode.Perform();
    }
    //private void BerserkAgain(string result, GoapAction goapAction) {
    //    string summary = stateComponent.character.name + " is checking for berserk again";
    //    //SetCurrentlyDoingAction(null);
    //    if (stateComponent.currentState != null && stateComponent.currentState != this) {
    //        summary += "\nCould not berserk again because current state is " + stateComponent.currentState.ToString();
    //        Debug.Log(summary);
    //        return;
    //    }
    //    summary += "Berserk resuming!";
    //    Debug.Log(summary);
    //    stateComponent.character.SetCurrentActionNode(null, null, null);
    //    ResumeState();
    //}
    private void StartBerserkedMovement() {
        stateComponent.character.marker.GoTo(PickRandomTileToGoTo(), StartBerserkedMovement);
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

    public void SetHostileChecker(System.Func<Character, bool> hostileChecker) {
        this.hostileChecker = hostileChecker;
    }
    public void SetAreCombatsLethal(bool state) {
        areCombatsLethal = state;
    }
}
