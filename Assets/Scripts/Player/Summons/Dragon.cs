using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class Dragon : SkinnableAnimal {
    public override string raceClassName => "Dragon";
    public override System.Type serializedData => typeof(SaveDataDragon);

    public bool isAwakened { get; private set; }
    public bool isAttackingPlayer { get; private set; }
    public bool willLeaveWorld { get; private set; }
    public LocationStructure targetStructure { get; private set; }
    public int leaveWorldCounter { get; private set; }
    public List<Character> charactersThatAreWary { get; private set; }
    private readonly int _leaveWorldTimer;

    public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.BEAR_HIDE;

    public override bool defaultDigMode => true;

    public Dragon() : base(SUMMON_TYPE.Dragon, "Dragon", RACE.DRAGON, UtilityScripts.Utilities.GetRandomGender()) {
        //SetMaxHPMod(1000);
        traitContainer.AddTrait(this, "Immune");
        traitContainer.AddTrait(this, "Hibernating");
        traitContainer.AddTrait(this, "Fire Resistant");
        traitContainer.AddTrait(this, "Sturdy");
        //traitContainer.AddTrait(this, "Indestructible");
        _leaveWorldTimer = GameManager.Instance.GetTicksBasedOnHour(8);
        charactersThatAreWary = new List<Character>();
        //movementComponent.SetTagAsTraversable(InnerMapManager.Obstacle_Tag); //dragons can traverse the obstacles tag
        //set all penalties to 0, so that dragon can freely move around
        for (int i = InnerMapManager.Starting_Tag_Index; i < 32; i++) {
            movementComponent.SetPenaltyForTag(i, 0);
        }
    }
    public Dragon(string className) : base(SUMMON_TYPE.Dragon, className, RACE.DRAGON, UtilityScripts.Utilities.GetRandomGender()) {
        //SetMaxHPMod(1000);
        traitContainer.AddTrait(this, "Immune");
        traitContainer.AddTrait(this, "Hibernating");
        traitContainer.AddTrait(this, "Fire Resistant");
        traitContainer.AddTrait(this, "Sturdy");
        //traitContainer.AddTrait(this, "Indestructible");
        _leaveWorldTimer = GameManager.Instance.GetTicksBasedOnHour(8);
        charactersThatAreWary = new List<Character>();
    }
    public Dragon(SaveDataDragon data) : base(data) {
        charactersThatAreWary = new List<Character>();
        _leaveWorldTimer = GameManager.Instance.GetTicksBasedOnHour(8);

        isAwakened = data.isAwakened;
        isAttackingPlayer = data.isAttackingPlayer;
        willLeaveWorld = data.willLeaveWorld;
        leaveWorldCounter = data.leaveWorldCounter;
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        movementComponent.SetToFlying();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Dragon_Behaviour);
    }
    public override void SubscribeToSignals() {
        if (hasSubscribedToSignals) {
            return;
        }
        base.SubscribeToSignals();
        Messenger.AddListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        Messenger.AddListener(Signals.TICK_STARTED, TryLeaveWorld);
    }
    public override void UnsubscribeSignals() {
        if (!hasSubscribedToSignals) {
            return;
        }
        base.UnsubscribeSignals();
        Messenger.RemoveListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        Messenger.RemoveListener(Signals.TICK_STARTED, TryLeaveWorld);
    }
    private void OnCharacterClassChange(Character character, CharacterClass previousClass, CharacterClass newClass) {
        if (character == this && newClass.IsZombie()) {
            //when dragon becomes a zombie it should no longer try to leave the world, because if it does it will become permanently passive.
            Messenger.RemoveListener(Signals.TICK_STARTED, TryLeaveWorld);
        }
    }
    private void TryLeaveWorld() {
        if (isAwakened && !willLeaveWorld) {
            CheckLeaveWorld();
        }
    }
    public override void ConstructDefaultActions() {
        base.ConstructDefaultActions();
        // RemovePlayerAction(PLAYER_SKILL_TYPE.SNATCH);
    }
    public override void LoadReferences(SaveDataCharacter data) {
        if(data is SaveDataDragon savedData) {
            if(!string.IsNullOrEmpty(savedData.targetStructure)) {
                targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(savedData.targetStructure);
            }
            if(savedData.charactersThatAreWary != null) {
                for (int i = 0; i < savedData.charactersThatAreWary.Count; i++) {
                    Character character = CharacterManager.Instance.GetCharacterByPersistentID(savedData.charactersThatAreWary[i]);
                    if (character != null) {
                        charactersThatAreWary.Add(character);
                    }
                }
            }
        }
        base.LoadReferences(data);
    }
    #endregion

    public void Awaken() {
        if (!isAwakened) {
            isAwakened = true;
            traitContainer.RemoveTrait(this, "Immune");
            traitContainer.RemoveTrait(this, "Hibernating");
            //traitContainer.RemoveTrait(this, "Indestructible");
            //StartLeaveWorldTimer();
            Messenger.Broadcast(MonsterSignals.AWAKEN_DRAGON, this as Character);
        }
    }

    public void SetIsAttackingPlayer(bool state) {
        isAttackingPlayer = state;
    }

    private void CheckLeaveWorld() {
        leaveWorldCounter++;
        if(leaveWorldCounter >= _leaveWorldTimer) {
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
    public void AddCharacterThatWary(Character character) {
        charactersThatAreWary.Add(character);
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
    //                bool isCharacterInStillInTerritory = character.gridTileLocation.collectionOwner.isPartOfParentRegionMap && IsTerritory(character.gridTileLocation.hexTileOwner);
    //                if (!isCharacterInStillInTerritory) {
    //                    combatComponent.RemoveHostileInRange(character);
    //                }
    //            }
    //        }
    //    }
    //}
}

[System.Serializable]
public class SaveDataDragon : SaveDataSkinnableAnimal {
    public bool isAwakened;
    public bool isAttackingPlayer;
    public bool willLeaveWorld;
    public int leaveWorldCounter;
    public List<string> charactersThatAreWary;

    public string targetStructure;

    public override void Save(Character data) {
        base.Save(data);
        if (data is Dragon summon) {
            isAwakened = summon.isAwakened;
            isAttackingPlayer = summon.isAttackingPlayer;
            willLeaveWorld = summon.willLeaveWorld;
            leaveWorldCounter = summon.leaveWorldCounter;

            if(summon.targetStructure != null) {
                targetStructure = summon.targetStructure.persistentID;
            }
            if(summon.charactersThatAreWary.Count > 0) {
                charactersThatAreWary = new List<string>();
                for (int i = 0; i < summon.charactersThatAreWary.Count; i++) {
                    charactersThatAreWary.Add(summon.charactersThatAreWary[i].persistentID);
                }
            }
        }
    }
}