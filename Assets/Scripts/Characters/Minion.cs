using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Object_Pools;
using Traits;
using UnityEngine.Profiling;

public class Minion {
    public const int MAX_INTERVENTION_ABILITY_SLOT = 5;

    public Character character { get; private set; }
    //public int exp { get; private set; }
    //public CombatAbility combatAbility { get; private set; }
    //public List<string> traitsToAdd { get; private set; }
    //public Region assignedRegion { get; private set; } //the landmark that this minion is currently invading. NOTE: This is set on both npcSettlement and non npcSettlement landmarks
    //public int spellExtractionCount { get; private set; } //the number of times a spell was extracted from this minion.
    public bool isSummoned { get; private set; }
    public PLAYER_SKILL_TYPE minionPlayerSkillType { get; private set; }

    #region getters
    public DeadlySin deadlySin => CharacterManager.Instance.GetDeadlySin(character.characterClass.className);
    #endregion

    public Minion(Character character, bool keepData) {
        this.character = character;
        //this.exp = 0;
        //traitsToAdd = new List<string>();
        character.SetMinion(this);
        //character.StartingLevel();
        if (!keepData) {
            character.SetFirstAndLastName(character.characterClass.className, string.Empty);
        }
        // RemoveInvalidPlayerActions();
        // character.needsComponent.SetFullnessForcedTick(0);
        // character.needsComponent.SetTirednessForcedTick(0);
        // character.needsComponent.SetHappinessForcedTick(0);
        if (character.behaviourComponent.defaultBehaviourSetName == CharacterManager.Default_Resident_Behaviour) {
            //only change default behaviour set of minion if it is currently using the default resident behaviour.
            character.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Minion_Behaviour);    
        }
        character.visuals.UpdateAllVisuals(character);
    }
    public Minion(Character character, SaveDataMinion data) {
        this.character = character;
        isSummoned = data.isSummoned;
        minionPlayerSkillType = data.minionPlayerSkillType;
        if (isSummoned) {
            SubscribeListeners();
        }
    }
    public void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFillerStruct[] deathLogFillers = null) {
        if (!character.isDead) {
            Region deathLocation = character.currentRegion;
            LocationStructure deathStructure = character.currentStructure;
            LocationGridTile deathTile = character.gridTileLocation;

            //Unseize first before processing death
            if (character.isBeingSeized) {
                PlayerManager.Instance.player.seizeComponent.UnseizePOIOnDeath();
            }
            character.SetDeathLocation(character.gridTileLocation);
            
            character.SetIsDead(true);
            character.SetPOIState(POI_STATE.INACTIVE);

            //Remove disguise first before processing death
            character.reactionComponent.SetDisguisedCharacter(null);

            if (character.currentRegion == null) {
                throw new Exception(
                    $"Specific location of {character.name} is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
            }
            if (character.stateComponent.currentState != null) {
                character.stateComponent.ExitCurrentState();
            }
            //if (character.currentSettlement != null && character.isHoldingItem) {
            //    character.DropAllItems(deathTile);
            //}
            character.DropAllItems(deathTile);
            character.UnownOrTransferOwnershipOfAllItems();

            character.reactionComponent.SetIsHidden(false);
            //clear traits that need to be removed
            character.traitsNeededToBeRemoved.Clear();

            character.UncarryPOI();
            Character carrier = character.isBeingCarriedBy;
            if (carrier != null) {
                carrier.UncarryPOI(character);
            }

            if (character.partyComponent.hasParty) {
                character.partyComponent.currentParty.RemoveMember(character);
            }
            //character.ownParty.PartyDeath();

            //No longer remove from region list even if character died to prevent inconsistency in data because if a dead character is picked up and dropped, he will be added in the structure location list again but wont be in region list
            //https://trello.com/c/WTiGxjrK/1786-inconsistent-characters-at-location-list-in-region-with-characters-at-structure
            //character.currentRegion?.RemoveCharacterFromLocation(character);
            //character.SetRegionLocation(deathLocation); //set the specific location of this party, to the location it died at
            //character.SetCurrentStructureLocation(deathStructure, false);

            // character.role?.OnDeath(character);
            character.traitContainer.RemoveAllTraitsAndStatusesByName(character, "Criminal"); //remove all criminal type traits

            List<Trait> traitOverrideFunctions = character.traitContainer.GetTraitOverrideFunctions(TraitManager.Death_Trait);
            if (traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
                    if (trait.OnDeath(character)) {
                        i--;
                    }
                }
            }
            //for (int i = 0; i < character.traitContainer.allTraitsAndStatuses.Count; i++) {
            //    if (character.traitContainer.allTraitsAndStatuses[i].OnDeath(character)) {
            //        i--;
            //    }
            //}

            character.traitContainer.RemoveAllNonPersistentTraitAndStatuses(character);
            character.marker?.OnDeath(deathTile);
            
            // Dead dead = new Dead();
            // dead.AddCharacterResponsibleForTrait(responsibleCharacter);
            // character.traitContainer.AddTrait(character, dead, gainedFromDoing: deathFromAction);
            // PlayerManager.Instance.player.RemoveMinion(this);
            Messenger.Broadcast(CharacterSignals.CHARACTER_DEATH, character);
            character.eventDispatcher.ExecuteCharacterDied(character);

            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, character as IPointOfInterest, "target is already dead");
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, character as IPointOfInterest, "target is already dead");
            character.behaviourComponent.OnDeath();
            character.jobQueue.CancelAllJobs();
            // StopInvasionProtocol(PlayerManager.Instance.player.currentNpcSettlementBeingInvaded);
            
            if (_deathLog == null) {
                Log deathLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", $"death_{cause}", providedTags: LOG_TAG.Life_Changes);
                deathLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                if (responsibleCharacter != null) {
                    deathLog.AddToFillers(responsibleCharacter, responsibleCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                }
                if (deathLogFillers != null) {
                    for (int i = 0; i < deathLogFillers.Length; i++) {
                        deathLog.AddToFillers(deathLogFillers[i]);
                    }
                }
                //will only add death log to history if no death log is provided. NOTE: This assumes that if a death log is provided, it has already been added to this characters history.
                deathLog.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFrom(character, deathLog);
                character.SetDeathLog(deathLog);
                LogPool.Release(deathLog);
            } else {
                character.SetDeathLog(_deathLog);
            }
            List<Trait> afterDeathTraitOverrideFunctions = character.traitContainer.GetTraitOverrideFunctions(TraitManager.After_Death);
            if (afterDeathTraitOverrideFunctions != null) {
                for (int i = 0; i < afterDeathTraitOverrideFunctions.Count; i++) {
                    Trait trait = afterDeathTraitOverrideFunctions[i];
                    trait.AfterDeath(character);
                }
            }
            Unsummon();
            GameManager.Instance.CreateParticleEffectAt(deathTile, PARTICLE_EFFECT.Minion_Dissipate);
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, character as IPlayerActionTarget);
        }
    }

    //#region Combat Ability
    //public void SetCombatAbility(CombatAbility combatAbility, bool showNewAbilityUI = false) {
    //    if (this.combatAbility == null) {
    //        this.combatAbility = combatAbility;
    //        if (combatAbility != null && showNewAbilityUI) {
    //            PlayerUI.Instance.newAbilityUI.ShowNewAbilityUI(this, combatAbility);
    //        }
    //        Messenger.Broadcast(Signals.MINION_CHANGED_COMBAT_ABILITY, this);
    //    } else {
    //        PlayerUI.Instance.replaceUI.ShowReplaceUI(new List<CombatAbility>() { this.combatAbility }, combatAbility, ReplaceCombatAbility, RejectCombatAbility);
    //    }
    //}
    //public void SetCombatAbility(COMBAT_ABILITY combatAbility, bool showNewAbilityUI = false) {
    //    SetCombatAbility(PlayerManager.Instance.CreateNewCombatAbility(combatAbility), showNewAbilityUI);
    //}
    //private void ReplaceCombatAbility(object objToReplace, object objToAdd) {
    //    CombatAbility newAbility = objToAdd as CombatAbility;
    //    this.combatAbility = newAbility;
    //}
    //private void RejectCombatAbility(object objToReplace) {

    //}
    //public void ResetCombatAbilityCD() {
    //    combatAbility.StopCooldown();
    //}
    //#endregion

    #region Invasion
    private void OnTickEnded() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"Minion On Tick Ended");
#endif
        if (character.isDead) { return; }
        character.interruptComponent.OnTickEnded();
        character.stateComponent.OnTickEnded();
        character.ProcessTraitsOnTickEnded();
        character.TryProcessTraitsOnTickEndedWhileStationaryOrUnoccupied();
        character.EndTickPerformJobs();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    private void OnTickStarted() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"Minion On Tick Started");
#endif
        if (character.isDead) { return; }
        character.ProcessTraitsOnTickStarted();
        if (character.CanPlanGoap()) {
            character.PerStartTickActionPlanning();
        }
        // character.AdjustHP(-5, ELEMENTAL_TYPE.Normal, triggerDeath: true, showHPBar: true, source: character);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
#endregion

#region Utilities
    public void SetMinionPlayerSkillType(PLAYER_SKILL_TYPE skillType) {
        minionPlayerSkillType = skillType;
    }
    public string GetMinionClassName(PLAYER_SKILL_TYPE skillType) {
        switch (skillType) {
            case PLAYER_SKILL_TYPE.DEMON_WRATH:
                return "Wrath";
            case PLAYER_SKILL_TYPE.DEMON_ENVY:
                return "Envy";
            case PLAYER_SKILL_TYPE.DEMON_GLUTTONY:
                return "Gluttony";
            case PLAYER_SKILL_TYPE.DEMON_GREED:
                return "Greed";
            case PLAYER_SKILL_TYPE.DEMON_LUST:
                return "Lust";
            case PLAYER_SKILL_TYPE.DEMON_PRIDE:
                return "Pride";
            case PLAYER_SKILL_TYPE.DEMON_SLOTH:
                return "Sloth";
            default:
                return "Wrath";
        }
    }
#endregion

#region Summoning
    public void Summon(LocationGridTile tile) {
        character.CreateMarker();
        character.marker.visionCollider.VoteToUnFilterVision();

        character.ConstructInitialGoapAdvertisementActions();
        character.marker.InitialPlaceMarkerAt(tile);
        character.SetIsDead(false);
        character.behaviourComponent.OnSummon(tile);

        SubscribeListeners();
        SetIsSummoned(true);
        Messenger.Broadcast(PlayerSkillSignals.SUMMON_MINION, this);
    }
    private void Unsummon() {
        //if(!character.HasHealth()) {
        //    character.SetHP(0);
        //}
        //Messenger.AddListener(Signals.TICK_STARTED, UnsummonedCooldown);

        UnSubscribeListeners();
        SetIsSummoned(false);

        //If a minion is unsummoned remove it from the region/structure list of characters
        //Region deathLocation = character.currentRegion;
        //LocationStructure deathStructure = character.currentStructure;
        character.currentRegion?.RemoveCharacterFromLocation(character);
        character.MigrateHomeStructureTo(null);
        character.homeRegion?.RemoveResident(character);
        //character.SetRegionLocation(deathLocation);
        //character.SetCurrentStructureLocation(deathStructure, false);
        character.jobQueue.CancelAllJobs();
        character.interruptComponent.ForceEndNonSimultaneousInterrupt();
        character.combatComponent.ClearAvoidInRange(false);
        character.combatComponent.ClearHostilesInRange(false);

        SkillData spellData = PlayerSkillManager.Instance.GetMinionPlayerSkillData(minionPlayerSkillType);

        //int missingHealth = character.maxHP - character.currentHP;
        int cooldown = GameManager.Instance.GetTicksBasedOnHour(12); //Mathf.CeilToInt((float) missingHealth / 7);
        spellData.SetCooldown(cooldown);
        //spellData.SetCurrentCooldownTick(0);
        spellData.StartCooldown();
        //SkillData spellData = PlayerSkillManager.Instance.GetMinionPlayerSkillData(minionPlayerSkillType);
        //spellData.SetCooldown(-1);
        //spellData.AdjustCharges(1);

        //Messenger.Broadcast(SpellSignals.SPELL_COOLDOWN_STARTED, spellData);
        Messenger.Broadcast(PlayerSkillSignals.UNSUMMON_MINION, this);
    }
    private void UnsummonedCooldown() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"Minion Unsummoned HP Recovery");
#endif
        this.character.AdjustHP(7, ELEMENTAL_TYPE.Normal);
        SkillData spellData = PlayerSkillManager.Instance.GetMinionPlayerSkillData(minionPlayerSkillType);
        spellData.SetCurrentCooldownTick(spellData.currentCooldownTick + 1);
        if (character.IsHealthFull()) {
            //minion can be summoned again
            spellData.SetCooldown(-1);
            spellData.AdjustCharges(1);
            Messenger.Broadcast(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, spellData);
            Messenger.RemoveListener(Signals.TICK_STARTED, UnsummonedCooldown);
        }
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    public void OnSeize() {
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStarted);
    }
    public void OnUnseize() {
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted);
    }
    public void SetIsSummoned(bool state) {
        isSummoned = state;
    }
#endregion

    #region Listeners
    private void SubscribeListeners() {
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted);

        //For explanation why this part is commented out, please see Character.cs SubscribeToSignals
        //Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        //Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
        Messenger.AddListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, character.ForceCancelAllJobsTargetingPOI);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_PERFORM_AGAIN, OnCharacterCanPerformAgain);
        character.religionComponent.SubscribeListeners();
    }
    private void UnSubscribeListeners() {
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStarted);
        //Messenger.RemoveListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        //Messenger.RemoveListener<Character, CharacterState>(CharacterSignals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
        Messenger.RemoveListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, character.ForceCancelAllJobsTargetingPOI);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_PERFORM_AGAIN, OnCharacterCanPerformAgain);
        character.religionComponent.UnsubscribeListeners();
    }
    //private void OnCharacterStartedState(Character characterThatStartedState, CharacterState state) {
    //    if (characterThatStartedState == character) {
    //        //character.marker.UpdateActionIcon();
    //        if (state.characterState.IsCombatState()) {
    //            character.marker.visionCollider.TransferAllDifferentStructureCharacters();
    //        }
    //    }
    //}
    private void OnCharacterCanPerformAgain(Character character) {
        if (character == this.character) {
            
            //Add all in vision poi to process again
            for (int i = 0; i < character.marker.inVisionPOIs.Count; i++) {
                IPointOfInterest inVision = character.marker.inVisionPOIs[i];
                character.marker.AddUnprocessedPOI(inVision);
            }
        }
    }
    //private void OnCharacterEndedState(Character character, CharacterState state) {
    //    if (character == this.character) {
    //        if (state is CombatState && character.marker) {
    //            character.marker.visionCollider.ReCategorizeVision();
    //        }
    //    }
    //}
    #endregion

    #region Clean Up
    public void CleanUp() {
        character = null;
    }
    #endregion
}

[System.Serializable]
public struct UnsummonedMinionData {
    public string minionName;
    public string className;
    public COMBAT_ABILITY combatAbility;
    public List<PLAYER_SKILL_TYPE> interventionAbilitiesToResearch;

    public override bool Equals(object obj) {
        if (obj is UnsummonedMinionData) {
            return Equals((UnsummonedMinionData)obj);
        }
        return base.Equals(obj);
    }
    public override int GetHashCode() {
        return base.GetHashCode();
    }
    private bool Equals(UnsummonedMinionData data) {
        return this.minionName == data.minionName && this.className == data.className && this.combatAbility == data.combatAbility && this.interventionAbilitiesToResearch.Equals(data.interventionAbilitiesToResearch);
    }
}