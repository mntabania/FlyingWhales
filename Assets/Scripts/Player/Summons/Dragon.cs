using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class Dragon : Summon {
    public override string raceClassName => "Dragon";

    public bool isAwakened { get; private set; }
    public bool isAttackingPlayer { get; private set; }
    public bool willLeaveWorld { get; private set; }
    public LocationStructure targetStructure { get; private set; }
    public override bool defaultDigMode => true;
    private int _leaveWorldCounter;
    private readonly int _leaveWorldTimer;

    public Dragon() : base(SUMMON_TYPE.Dragon, "Dragon", RACE.DRAGON, UtilityScripts.Utilities.GetRandomGender()) {
        //SetMaxHPMod(1000);
        traitContainer.AddTrait(this, "Immune");
        traitContainer.AddTrait(this, "Hibernating");
        traitContainer.AddTrait(this, "Fireproof");
        //traitContainer.AddTrait(this, "Indestructible");
        _leaveWorldTimer = GameManager.Instance.GetTicksBasedOnHour(8);
    }
    public Dragon(string className) : base(SUMMON_TYPE.Dragon, className, RACE.DRAGON, UtilityScripts.Utilities.GetRandomGender()) {
        //SetMaxHPMod(1000);
        traitContainer.AddTrait(this, "Immune");
        traitContainer.AddTrait(this, "Hibernating");
        traitContainer.AddTrait(this, "Fireproof");
        //traitContainer.AddTrait(this, "Indestructible");
        _leaveWorldTimer = GameManager.Instance.GetTicksBasedOnHour(8);
    }
    public Dragon(SaveDataCharacter data) : base(data) { }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        movementComponent.SetEnableDigging(true);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Dragon_Behaviour);
    }
    protected override void OnTickStarted() {
        base.OnTickStarted();
        if (isAwakened && !willLeaveWorld) {
            CheckLeaveWorld();
        }
    }
    public override void ConstructDefaultActions() {
        base.ConstructDefaultActions();
        RemovePlayerAction(SPELL_TYPE.SNATCH);
    }
    #endregion

    public void Awaken() {
        if (!isAwakened) {
            isAwakened = true;
            traitContainer.RemoveTrait(this, "Immune");
            traitContainer.RemoveTrait(this, "Hibernating");
            //traitContainer.RemoveTrait(this, "Indestructible");
            //StartLeaveWorldTimer();
            Messenger.Broadcast(Signals.AWAKEN_DRAGON, this as Character);
        }
    }

    public void SetIsAttackingPlayer(bool state) {
        isAttackingPlayer = state;
    }

    private void CheckLeaveWorld() {
        _leaveWorldCounter++;
        if(_leaveWorldCounter >= _leaveWorldTimer) {
            LeaveWorld();
        }
    }
    public void SetWillLeaveWorld(bool state) {
        if(willLeaveWorld != state) {
            willLeaveWorld = state;
            if (willLeaveWorld) {
                combatComponent.SetCombatMode(COMBAT_MODE.Passive);
                jobQueue.CancelAllJobs();
                combatComponent.ClearHostilesInRange();
                combatComponent.ClearAvoidInRange();
            }
        }
    }
    private void LeaveWorld() {
        if (isDead) {
            return;
        }
        SetWillLeaveWorld(true);
    }
    //private void StartLeaveWorldTimer() {
    //    GameDate dueDate = GameManager.Instance.Today();
    //    dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
    //    SchedulingManager.Instance.AddEntry(dueDate, LeaveWorld, this);
    //}
    public void SetTargetStructure(LocationStructure structure) {
        targetStructure = structure;
    }
    public void SetVillageTargetStructure() {
        targetStructure = gridTileLocation.GetNearestVillageStructureFromThisWithResidents(this);
    }
    public void SetPlayerTargetStructure() {
        targetStructure = PlayerManager.Instance.player.playerSettlement.GetRandomStructure();
    }
    public void ResetTargetStructure() {
        targetStructure = null;
    }

    //private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
    //    if (character != this && combatComponent.isInCombat && homeStructure != null) {
    //        if (structure != homeStructure) {
    //            combatComponent.RemoveHostileInRange(character);
    //        }
    //    }
    //}

    //private void OnCharacterExitedHexTile(Character character, HexTile tile) {
    //    if (character != this && combatComponent.isInCombat) {
    //        if (HasTerritory()) {
    //            if (IsTerritory(tile)) {
    //                bool isCharacterInStillInTerritory = character.gridTileLocation.collectionOwner.isPartOfParentRegionMap && IsTerritory(character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner);
    //                if (!isCharacterInStillInTerritory) {
    //                    combatComponent.RemoveHostileInRange(character);
    //                }
    //            }
    //        }
    //    }
    //}
}