using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;

public class Minion {
    public const int MAX_INTERVENTION_ABILITY_SLOT = 5;

    public Character character { get; private set; }
    public int exp { get; private set; }
    public int indexDefaultSort { get; private set; }
    public CombatAbility combatAbility { get; private set; }
    public List<string> traitsToAdd { get; private set; }
    public Region assignedRegion { get; private set; } //the landmark that this minion is currently invading. NOTE: This is set on both npcSettlement and non npcSettlement landmarks
    public DeadlySin deadlySin => CharacterManager.Instance.GetDeadlySin(_assignedDeadlySinName);
    public bool isAssigned => assignedRegion != null; //true if minion is already assigned somewhere else, maybe in construction or research spells
    public int spellExtractionCount { get; private set; } //the number of times a spell was extracted from this minion.
    public bool isSummoned { get; private set; }
    public SPELL_TYPE minionPlayerSkillType { get; private set; }

    private string _assignedDeadlySinName;
    
    public Minion(Character character, bool keepData) {
        this.character = character;
        this.exp = 0;
        traitsToAdd = new List<string>();
        character.SetMinion(this);
        //character.StartingLevel();
        SetAssignedDeadlySinName(character.characterClass.className);
        character.ownParty.icon.SetVisualState(true);
        if (!keepData) {
            character.SetName(RandomNameGenerator.GenerateMinionName());
        }
        // RemoveInvalidPlayerActions();
        character.needsComponent.SetFullnessForcedTick(0);
        character.needsComponent.SetTirednessForcedTick(0);
        if (character.behaviourComponent.defaultBehaviourSetName == CharacterManager.Default_Resident_Behaviour) {
            //only change default behaviour set of minion if it is currently using the default resident behaviour.
            character.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Minion_Behaviour);    
        }
        // if (character.combatComponent.combatMode == COMBAT_MODE.Aggressive) {
        //     //only change combat mode of minions that haven't already changed their combat mode
        //     character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
        // }
        character.visuals.UpdateAllVisuals(character);
    }
    public Minion(SaveDataMinion data) {
        this.character = CharacterManager.Instance.GetCharacterByID(data.characterID);
        this.exp = data.exp;
        traitsToAdd = data.traitsToAdd;
        SetIndexDefaultSort(data.indexDefaultSort);
        character.SetMinion(this);
        character.ownParty.icon.SetVisualState(true);
        SetAssignedDeadlySinName(character.characterClass.className);
        spellExtractionCount = data.spellExtractionCount;
        character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
        // RemoveInvalidPlayerActions();
    }
    public void SetAssignedDeadlySinName(string name) {
        _assignedDeadlySinName = name;
    }
    public void SetPlayerCharacterItem(PlayerCharacterItem item) {
        //character.SetPlayerCharacterItem(item);
    }
    public void SetIndexDefaultSort(int index) {
        indexDefaultSort = index;
    }
    public void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, 
        Log _deathLog = null, LogFiller[] deathLogFillers = null) {
        if (!character.isDead) {
            Region deathLocation = character.currentRegion;
            LocationStructure deathStructure = character.currentStructure;
            LocationGridTile deathTile = character.gridTileLocation;

            character.SetIsDead(true);
            character.SetPOIState(POI_STATE.INACTIVE);

            if (character.currentRegion == null) {
                throw new Exception(
                    $"Specific location of {character.name} is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
            }
            if (character.stateComponent.currentState != null) {
                character.stateComponent.ExitCurrentState();
            }
            if (character.currentSettlement != null && character.isHoldingItem) {
                character.DropAllItems(deathTile);
            }

            //clear traits that need to be removed
            character.traitsNeededToBeRemoved.Clear();

            Character carrier = character.isBeingCarriedBy;
            if (carrier != null) {
                carrier.UncarryPOI(character);
            }
            character.ownParty.PartyDeath();
            character.currentRegion?.RemoveCharacterFromLocation(character);
            character.SetRegionLocation(deathLocation); //set the specific location of this party, to the location it died at
            character.SetCurrentStructureLocation(deathStructure, false);

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
            Messenger.Broadcast(Signals.CHARACTER_DEATH, character);

            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, character as IPointOfInterest, "target is already dead");
            character.behaviourComponent.OnDeath();
            character.CancelAllJobs();
            // StopInvasionProtocol(PlayerManager.Instance.player.currentNpcSettlementBeingInvaded);

            Log deathLog;
            if (_deathLog == null) {
                deathLog = new Log(GameManager.Instance.Today(), "Character", "Generic", $"death_{cause}");
                deathLog.AddToFillers(this, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                if (responsibleCharacter != null) {
                    deathLog.AddToFillers(responsibleCharacter, responsibleCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                }
                if (deathLogFillers != null) {
                    for (int i = 0; i < deathLogFillers.Length; i++) {
                        deathLog.AddToFillers(deathLogFillers[i]);
                    }
                }
                //will only add death log to history if no death log is provided. NOTE: This assumes that if a death log is provided, it has already been added to this characters history.
                character.logComponent.AddHistory(deathLog);
                PlayerManager.Instance.player.ShowNotificationFrom(character, deathLog);
            } else {
                deathLog = _deathLog;
            }
            character.SetDeathLog(deathLog);
            Unsummon();
            GameManager.Instance.CreateParticleEffectAt(deathTile, PARTICLE_EFFECT.Minion_Dissipate);
        }
    }

    #region Intervention Abilities
    //public void SetUnlockedInterventionSlots(int amount) {
    //    unlockedInterventionSlots = amount;
    //    unlockedInterventionSlots = Mathf.Clamp(unlockedInterventionSlots, 0, MAX_INTERVENTION_ABILITY_SLOT);
    //}
    //public void AdjustUnlockedInterventionSlots(int amount) {
    //    unlockedInterventionSlots += amount;
    //    unlockedInterventionSlots = Mathf.Clamp(unlockedInterventionSlots, 0, MAX_INTERVENTION_ABILITY_SLOT);
    //}
    //public void GainNewInterventionAbility(PlayerJobAction ability, bool showNewAbilityUI = false) {
    //    int currentInterventionAbilityCount = GetCurrentInterventionAbilityCount();
    //    if(currentInterventionAbilityCount < unlockedInterventionSlots) {
    //        for (int i = 0; i < interventionAbilities.Length; i++) {
    //            if (interventionAbilities[i] == null) {
    //                interventionAbilities[i] = ability;
    //                ability.SetMinion(this);
    //                Messenger.Broadcast(Signals.MINION_LEARNED_INTERVENE_ABILITY, this, ability);
    //                if (showNewAbilityUI) {
    //                    PlayerUI.Instance.newAbilityUI.ShowNewAbilityUI(this, ability);
    //                }
    //                break;
    //            }
    //        }
    //    } else {
    //        //Broadcast intervention ability is full, must open UI whether player wants to replace ability or discard it
    //        PlayerUI.Instance.replaceUI.ShowReplaceUI(GeAllInterventionAbilities(), ability, ReplaceAbility, RejectAbility);
    //    }
    //}
    //private void ReplaceAbility(object objToReplace, object objToAdd) {
    //    PlayerJobAction replace = objToReplace as PlayerJobAction;
    //    PlayerJobAction add = objToAdd as PlayerJobAction;
    //    for (int i = 0; i < interventionAbilities.Length; i++) {
    //        if (interventionAbilities[i] == replace) {
    //            interventionAbilities[i] = add;
    //            add.SetMinion(this);
    //            replace.SetMinion(null);
    //            Messenger.Broadcast(Signals.MINION_LEARNED_INTERVENE_ABILITY, this, add);
    //            break;
    //        }
    //    }
    //}
    //private void RejectAbility(object rejectedObj) { }
    //public void AddInterventionAbility(INTERVENTION_ABILITY ability, bool showNewAbilityUI = false) {
    //    GainNewInterventionAbility(PlayerManager.Instance.CreateNewInterventionAbility(ability), showNewAbilityUI);
    //}
    //public int GetCurrentInterventionAbilityCount() {
    //    int count = 0;
    //    for (int i = 0; i < interventionAbilities.Length; i++) {
    //        if (interventionAbilities[i] != null) {
    //            count++;
    //        }
    //    }
    //    return count;
    //}
    //public List<PlayerJobAction> GeAllInterventionAbilities() {
    //    List<PlayerJobAction> all = new List<PlayerJobAction>();
    //    for (int i = 0; i < interventionAbilities.Length; i++) {
    //        if (interventionAbilities[i] != null) {
    //            all.Add(interventionAbilities[i]);
    //        }
    //    }
    //    return all;
    //}
    //public void ResetInterventionAbilitiesCD() {
    //    for (int i = 0; i < interventionAbilities.Length; i++) {
    //        if(interventionAbilities[i] != null) {
    //            interventionAbilities[i].InstantCooldown();
    //        }
    //    }
    //}
    #endregion

    #region Combat Ability
    public void SetCombatAbility(CombatAbility combatAbility, bool showNewAbilityUI = false) {
        if (this.combatAbility == null) {
            this.combatAbility = combatAbility;
            if (combatAbility != null && showNewAbilityUI) {
                PlayerUI.Instance.newAbilityUI.ShowNewAbilityUI(this, combatAbility);
            }
            Messenger.Broadcast(Signals.MINION_CHANGED_COMBAT_ABILITY, this);
        } else {
            PlayerUI.Instance.replaceUI.ShowReplaceUI(new List<CombatAbility>() { this.combatAbility }, combatAbility, ReplaceCombatAbility, RejectCombatAbility);
        }
    }
    public void SetCombatAbility(COMBAT_ABILITY combatAbility, bool showNewAbilityUI = false) {
        SetCombatAbility(PlayerManager.Instance.CreateNewCombatAbility(combatAbility), showNewAbilityUI);
    }
    private void ReplaceCombatAbility(object objToReplace, object objToAdd) {
        CombatAbility newAbility = objToAdd as CombatAbility;
        this.combatAbility = newAbility;
    }
    private void RejectCombatAbility(object objToReplace) {

    }
    public void ResetCombatAbilityCD() {
        combatAbility.StopCooldown();
    }
    #endregion

    #region Invasion
    public void StartInvasionProtocol(NPCSettlement npcSettlement) {
        //TODO:
        // AddPendingTraits();
        // Messenger.AddListener(Signals.TICK_STARTED, PerTickInvasion);
        // Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        // Messenger.AddListener<NPCSettlement>(Signals.SUCCESS_INVASION_AREA, OnSucceedInvadeArea);
        // Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, character.OnOtherCharacterDied);
        // Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, character.OnCharacterEndedState);
        // SetAssignedRegion(npcSettlement.region);
    }
    public void StopInvasionProtocol(NPCSettlement npcSettlement) {
        //TODO:
        // if(npcSettlement != null && assignedRegion != null && assignedRegion.npcSettlement == npcSettlement) {
        //     Messenger.RemoveListener(Signals.TICK_STARTED, PerTickInvasion);
        //     Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        //     Messenger.RemoveListener<NPCSettlement>(Signals.SUCCESS_INVASION_AREA, OnSucceedInvadeArea);
        //     Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, character.OnOtherCharacterDied);
        //     Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, character.OnCharacterEndedState);
        //     SetAssignedRegion(null);
        // }
    }
    private void OnTickEnded() {
        if (character.isDead) { return; }
        character.interruptComponent.OnTickEnded();
        character.stateComponent.OnTickEnded();
        character.ProcessTraitsOnTickEnded();
        character.EndTickPerformJobs();
    }
    private void OnTickStarted() {
        if (character.isDead) { return; }
        character.ProcessTraitsOnTickStarted();
        if (character.CanPlanGoap()) {
            character.PerStartTickActionPlanning();
        }
        character.AdjustHP(-5, ELEMENTAL_TYPE.Normal, triggerDeath: true, showHPBar: true, source: character);
    }
    public void SetAssignedRegion(Region region) {
        assignedRegion = region;
        Messenger.Broadcast(Signals.MINION_CHANGED_ASSIGNED_REGION, this, assignedRegion);
    }
    #endregion

    #region Traits
    /// <summary>
    /// Add trait function for minions. Added handling for when a minion gains a trait while outside of an npcSettlement map. All traits are stored and will be added once the minion is placed at an npcSettlement map.
    /// </summary>
    public bool AddTrait(string traitName, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null) {
        if (InnerMapManager.Instance.isAnInnerMapShowing) {
            return character.traitContainer.AddTrait(character, traitName, characterResponsible, gainedFromDoing);
        } else {
            traitsToAdd.Add(traitName);
            return true;
        }
    }
    private void AddPendingTraits() {
        for (int i = 0; i < traitsToAdd.Count; i++) {
            character.traitContainer.AddTrait(character, traitsToAdd[i]);
        }
        traitsToAdd.Clear();
    }
    #endregion

    #region Utilities
    public void AdjustSpellExtractionCount(int amount) {
        spellExtractionCount += amount;
    }
    public void SetMinionPlayerSkillType(SPELL_TYPE skillType) {
        minionPlayerSkillType = skillType;
    }
    #endregion

    #region Summoning
    public void Summon(Inner_Maps.Location_Structures.ThePortal portalStructure) {
        character.CreateMarker();
        character.marker.visionCollider.VoteToUnFilterVision();
        int minX = portalStructure.tiles.Min(t => t.localPlace.x);
        int maxX = portalStructure.tiles.Max(t => t.localPlace.x);
        int minY = portalStructure.tiles.Min(t => t.localPlace.y);
        int maxY = portalStructure.tiles.Max(t => t.localPlace.y);

        int differenceX = (maxX - minX) + 1;
        int differenceY = (maxY - minY) + 1;

        int centerX = minX + (differenceX / 2);
        int centerY = minY + (differenceY / 2);

        LocationGridTile centerTile = portalStructure.location.innerMap.map[centerX, centerY];
        // Vector3 pos = centerTile.worldLocation;

        Summon(centerTile);
    }
    public void Summon(LocationGridTile tile) {
        character.CreateMarker();
        character.marker.visionCollider.VoteToUnFilterVision();

        character.ConstructInitialGoapAdvertisementActions();
        character.marker.InitialPlaceMarkerAt(tile);
        character.SetIsDead(false);
        character.behaviourComponent.OnSummon(tile);

        SubscribeListeners();
        SetIsSummoned(true);
        Messenger.Broadcast(Signals.SUMMON_MINION, this);
    }
    private void Unsummon() {
        if(character.currentHP < 0) {
            character.SetHP(0);
        }
        Messenger.AddListener(Signals.TICK_ENDED, UnsummonedHPRecovery);
        UnSubscribeListeners();
        SetIsSummoned(false);
        character.behaviourComponent.SetIsHarassing(false, null);
        character.behaviourComponent.SetIsDefending(false, null);
        character.behaviourComponent.SetIsInvading(false, null);
        character.CancelAllJobs();
        character.interruptComponent.ForceEndNonSimultaneousInterrupt();
        character.combatComponent.ClearAvoidInRange(false);
        character.combatComponent.ClearHostilesInRange(false);
        //PlayerSkillManager.Instance.GetMinionPlayerSkillData(minionPlayerSkillType).StartCooldown();
        Messenger.Broadcast(Signals.UNSUMMON_MINION, this);
    }
    private void UnsummonedHPRecovery() {
        this.character.AdjustHP(25, ELEMENTAL_TYPE.Normal);
        if (character.currentHP >= character.maxHP) {
            //minion can be summoned again
            PlayerSkillManager.Instance.GetMinionPlayerSkillData(minionPlayerSkillType).SetCharges(1);
            Messenger.RemoveListener(Signals.TICK_ENDED, UnsummonedHPRecovery);
        }
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
        Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
        Messenger.AddListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, character.ForceCancelAllJobsTargetingPOI);
    }
    private void UnSubscribeListeners() {
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener(Signals.TICK_STARTED, OnTickStarted);
        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
        Messenger.RemoveListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, character.ForceCancelAllJobsTargetingPOI);
    }
    private void OnCharacterStartedState(Character characterThatStartedState, CharacterState state) {
        if (characterThatStartedState == character) {
            character.marker.UpdateActionIcon();
            if (state.characterState.IsCombatState()) {
                character.marker.visionCollider.TransferAllDifferentStructureCharacters();
            }
        }
    }
    private void OnCharacterEndedState(Character character, CharacterState state) {
        if (character == this.character) {
            if (state is CombatState && character.marker) {
                character.marker.visionCollider.ReCategorizeVision();
            }
        }
    }
    #endregion
}

[System.Serializable]
public struct UnsummonedMinionData {
    public string minionName;
    public string className;
    public COMBAT_ABILITY combatAbility;
    public List<SPELL_TYPE> interventionAbilitiesToResearch;

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