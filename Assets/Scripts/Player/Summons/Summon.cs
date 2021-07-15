using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using Interrupts;
using Object_Pools;
using UnityEngine.Profiling;
using UtilityScripts;
public class Summon : Character {

	public SUMMON_TYPE summonType { get; }
    private bool showNotificationOnDeath { get; set; }
    /// <summary>
    /// Is this monster unimportant? (i.e. skeleton carriers from Defiler/Prison)
    /// </summary>
    public bool isVolatileMonster { get; private set; }
    public virtual Faction defaultFaction => FactionManager.Instance.neutralFaction;
    public virtual int gainedKennelSummonCapacity => 3;

	#region shearing and skinning data
	public virtual TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.NONE;
	#endregion

	#region getters
	public virtual SUMMON_TYPE adultSummonType => SUMMON_TYPE.None;
    public virtual COMBAT_MODE defaultCombatMode => COMBAT_MODE.Aggressive;
    public virtual bool defaultDigMode => false;
    public virtual string bredBehaviour => characterClass.traitNameOnTamedByPlayer;
    public override Type serializedData => typeof(SaveDataSummon);
    public bool isUsingDefaultName => name == raceClassName;
    public bool isTamed => faction != null && (faction.isMajorNonPlayer || faction.factionType.type == FACTION_TYPE.Ratmen);
    #endregion

    protected Summon(SUMMON_TYPE summonType, string className, RACE race, GENDER gender) : base(className, race, gender) {
        this.summonType = summonType;
        showNotificationOnDeath = true;
        isVolatileMonster = false;
        isInfoUnlocked = true;
        isWildMonster = true;
    }
    protected Summon(SaveDataSummon data) : base(data) {
        summonType = data.summonType;
        showNotificationOnDeath = true;
        isVolatileMonster = data.isVolatileMonster;
        isInfoUnlocked = true;
        isWildMonster = true;
    }

    #region user defined functions
    void DropItem() {
        CharacterClassData cData = CharacterManager.Instance.GetOrCreateCharacterClassData(characterClass.className);

        if (cData.droppableItems.Count > 0) {
            TILE_OBJECT_TYPE itemToDrop = TryGetWeightedSpellChoicesList().PickRandomElementGivenWeights();
            if (itemToDrop == TILE_OBJECT_TYPE.NONE) {
                return;
            }
            TileObject drop = InnerMapManager.Instance.CreateNewTileObject<TileObject>(itemToDrop);
            LocationGridTile tileToSpawnPile = gridTileLocation;
            if (tileToSpawnPile != null && tileToSpawnPile.tileObjectComponent.objHere != null) {
                tileToSpawnPile = gridTileLocation.GetFirstNeighborThatIsPassableAndNoObject();
            }
            tileToSpawnPile?.structure.AddPOI(drop, tileToSpawnPile);
        }
    }

    private WeightedDictionary<TILE_OBJECT_TYPE> TryGetWeightedSpellChoicesList() {
        CharacterClassData cData = CharacterManager.Instance.GetOrCreateCharacterClassData(characterClass.className);
        WeightedDictionary<TILE_OBJECT_TYPE> weights = new WeightedDictionary<TILE_OBJECT_TYPE>();
        for (int x = 0; x < cData.droppableItems.Count; ++x) {
            weights.AddElement(cData.droppableItems[x].item, cData.droppableItems[x].weight);
        }
        return weights;
    }
    #endregion

    #region Overrides
    public override void Initialize() {
        combatComponent.SetCombatMode(defaultCombatMode);
        ConstructDefaultActions();
        OnUpdateRace();
        classComponent.OnUpdateCharacterClass();

        moodComponent.OnCharacterBecomeMinionOrSummon();
        moodComponent.SetMoodValue(50);
        
        needsComponent.Initialize();
        moneyComponent.Initialize();
        
        advertisedActions.Clear(); //This is so that any advertisements from OnUpdateRace will be negated. TODO: Make updating advertisements better.
        //TODO: Put this in a system
        AddAdvertisedAction(INTERACTION_TYPE.ABSORB_LIFE);
        AddAdvertisedAction(INTERACTION_TYPE.ABSORB_POWER);
        ConstructInitialGoapAdvertisementActions();
        // needsComponent.SetFullnessForcedTick(0);
        // needsComponent.SetTirednessForcedTick(0);
        // needsComponent.SetHappinessForcedTick(0);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Monster_Behaviour);
        // movementComponent.AvoidAllFactions();
    }
    public override void OnActionPerformed(ActualGoapNode node) { } //overridden OnActionStateSet so that summons cannot witness other events.
    public override void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = default, LogFillerStruct[] deathLogFillers = null,
        Interrupt interrupt = null, bool isPlayerSource = false) {
        if (!_isDead) {
            deathTilePosition = gridTileLocation;
            Region deathLocation = currentRegion;
            LocationStructure deathStructure = currentStructure;

            //Unseize first before processing death
            if (isBeingSeized) {
                PlayerManager.Instance.player.seizeComponent.UnseizePOIOnDeath();
            }
            SetDeathLocation(gridTileLocation);
        
            if (isLycanthrope) {
                //Added this so that human and lycan form can share the same death log and prevent duplicates
                Character humanForm = lycanData.originalForm;
                lycanData.LycanDies(this, cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers);
                _deathLog = humanForm.deathLog;
                _deathLog.AddInvolvedObjectManual(persistentID);
            }

			if (!isDead) {
                DropItem();
            }
            SetIsDead(true);
            
            if (isLimboCharacter && isInLimbo) {
                //If a limbo character dies while in limbo, that character should not process death, instead he/she will be removed from the list
                CharacterManager.Instance.RemoveLimboCharacter(this);
                return;
            }
            //Remove disguise first before processing death
            reactionComponent.SetDisguisedCharacter(null);

            if (responsibleCharacter != null) {
                //If a character killed another character, he should automatically be one of the ones who saw the dead body
                //This is so that the killer will not assume murder anymore because he already knows the dead body
                //https://trello.com/c/xMuWkixY/4997-killer-assumed-another-villager-killed-his-victim
                reactionComponent.AddCharacterThatSawThisDead(responsibleCharacter);
            }

            UnsubscribeSignals();
            if (stateComponent.currentState != null) {
                stateComponent.ExitCurrentState();
            }
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "target is already dead");
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, this as IPointOfInterest, "target is already dead");
            jobQueue.CancelAllJobs();
            DropAllItems(deathTilePosition);
            UnownOrTransferOwnershipOfAllItems();

            reactionComponent.SetIsHidden(false);
            //clear traits that need to be removed
            traitsNeededToBeRemoved.Clear();

            UncarryPOI();
            Character carrier = isBeingCarriedBy;
            carrier?.UncarryPOI(this);

            if (hasMarker) {
                marker.OnDeath(deathTilePosition);
            }
            if (destroyMarkerOnDeath) {
                //If death is destroy marker, this will leave no corpse, so remove it from the list of characters at location in region
                if (currentRegion != null) {
                    currentRegion.RemoveCharacterFromLocation(this);
                }
            }
            previousCharacterDataComponent.SetHomeSettlementOnDeath(homeSettlement);
            if (homeRegion != null) {
                Region home = homeRegion;
                homeRegion.RemoveResident(this);
                MigrateHomeStructureTo(null, addToRegionResidents: false);
                SetHomeRegion(home); //keep this data with character to prevent errors
                //SetHomeStructure(homeStructure); //keep this data with character to prevent errors
            }
            if (partyComponent.hasParty) {
                partyComponent.currentParty.RemoveMember(this);
            }

            traitContainer.RemoveAllTraitsAndStatusesByName(this, "Criminal"); //remove all criminal type traits

            List<Trait> traitOverrideFunctions = traitContainer.GetTraitOverrideFunctions(TraitManager.Death_Trait);
            if (traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
                    if (trait.OnDeath(this)) {
                        i--;
                    }
                }
            }

            if (interruptComponent.isInterrupted && interruptComponent.currentInterrupt.interrupt != interrupt) {
                interruptComponent.ForceEndNonSimultaneousInterrupt();
            }
            traitContainer.AddTrait(this, "Dead", responsibleCharacter);
            if (deathFromAction != null) {
                traitContainer.GetTraitOrStatus<Trait>("Dead")?.SetGainedFromDoingAction(deathFromAction.action.goapType, deathFromAction.isStealth);
            }

            if (cause == "attacked" && responsibleCharacter != null && responsibleCharacter.isInWerewolfForm) {
                traitContainer.AddTrait(this, "Mangled", responsibleCharacter);
                if (deathFromAction != null) {
                    traitContainer.GetTraitOrStatus<Trait>("Mangled")?.SetGainedFromDoingAction(deathFromAction.action.goapType, deathFromAction.isStealth);
                }
            }
            Messenger.Broadcast(CharacterSignals.CHARACTER_DEATH, this as Character);
            eventDispatcher.ExecuteCharacterDied(this);
            behaviourComponent.OnDeath();
            jobQueue.CancelAllJobs();

            
            if (_deathLog == null) {
                Log localDeathLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", $"death_{cause}", null, LOG_TAG.Life_Changes);
                localDeathLog.AddToFillers(this, name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                if (responsibleCharacter != null) {
                    localDeathLog.AddToFillers(responsibleCharacter, responsibleCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                }
                if (deathLogFillers != null) {
                    for (int i = 0; i < deathLogFillers.Length; i++) {
                        localDeathLog.AddToFillers(deathLogFillers[i]);
                    }
                }
                //will only add death log to history if no death log is provided. NOTE: This assumes that if a death log is provided, it has already been added to this characters history.
                localDeathLog.AddLogToDatabase();
                if (showNotificationOnDeath) {
                    PlayerManager.Instance.player.ShowNotificationFrom(this, localDeathLog);    
                }
                SetDeathLog(localDeathLog);
                LogPool.Release(localDeathLog);
            } else {
                SetDeathLog(_deathLog);
            }
            
            AfterDeath(deathTilePosition);
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
    }
    protected override void OnTickStarted() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} OnTickStarted");
#endif
        needsComponent.PerTickSummon();
        ProcessTraitsOnTickStarted();
        StartTickGoapPlanGeneration();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    public override void OnUnseizePOI(LocationGridTile tileLocation) {
        base.OnUnseizePOI(tileLocation);
        //Reference: https://trello.com/c/Flr78mJy/3037-summon-types-should-no-longer-relocate-home-when-unseized-if-they-belong-to-a-major-faction
        if (faction != null && !faction.isMajorFaction) {
            //If you drop a monster at a structure that is not yet full and not occupied by villagers, they will set their home to that place.
            if(tileLocation.structure != null) {
                if(ShouldRelocateHomeToStructureOnUnseize(tileLocation.structure)) {
                    ClearTerritoryAndMigrateHomeStructureTo(tileLocation.structure);
                }
            }    
        }
    }
    private bool ShouldRelocateHomeToStructureOnUnseize(LocationStructure p_structure) {
        if (p_structure is Wilderness || p_structure is DemonicStructure) {
            return false;
        }
        //Reference: https://trello.com/c/NoWry5Tk/3041-monster-sets-home-to-settlement-structure-with-no-residents-if-unseized-there-even-though-settlement-is-still-owned-by-a-village
        if (p_structure.settlementLocation?.owner != null) {
            return false;
        }
        if (p_structure.HasReachedMaxResidentCapacity()) {
            return false;
        }
        if (p_structure.HasVillagerResident()) {
            return false;
        }
        return true;
    }
    public override void LoadReferences(SaveDataCharacter data) {
        base.LoadReferences(data);
        //Messenger.RemoveListener(Signals.HOUR_STARTED, () => needsComponent.DecreaseNeeds()); //do not make summons decrease needs
        movementComponent.UpdateSpeed();
    }
#endregion

    #region Virtuals
    /// <summary>
    /// What should a summon do when it is placed.
    /// </summary>
    /// <param name="tile">The tile the summon was placed on.</param>
    public virtual void OnPlaceSummon(LocationGridTile tile) {
        SubscribeToSignals();
        //Removed this Remove Listener, instead, do not add DecreaseNeeds to listener if character is a monster/summon
        //Messenger.RemoveListener(Signals.HOUR_STARTED, () => needsComponent.DecreaseNeeds()); //do not make summons decrease needs
        movementComponent.UpdateSpeed();
        behaviourComponent.OnSummon(tile);
    }
    protected virtual void AfterDeath(LocationGridTile deathTileLocation) {
        if (marker == null && destroyMarkerOnDeath) {
            if (race == RACE.TRITON) {
                GameManager.Instance.CreateParticleEffectAt(deathTileLocation, PARTICLE_EFFECT.Water_Bomb);
            } else {
                GameManager.Instance.CreateParticleEffectAt(deathTileLocation, PARTICLE_EFFECT.Minion_Dissipate);
            }
            
        }
        List<Trait> afterDeathTraitOverrideFunctions = traitContainer.GetTraitOverrideFunctions(TraitManager.After_Death);
        if (afterDeathTraitOverrideFunctions != null) {
            for (int i = 0; i < afterDeathTraitOverrideFunctions.Count; i++) {
                Trait trait = afterDeathTraitOverrideFunctions[i];
                trait.AfterDeath(this);
            }
        }
    }
    public virtual void OnSummonAsPlayerMonster() {
        combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
    }
    #endregion

    #region Player Action Target
    public override void ConstructDefaultActions() {
        if (actions == null) {
            actions = new List<PLAYER_SKILL_TYPE>();
        } else {
            actions.Clear();
        }
        AddPlayerAction(PLAYER_SKILL_TYPE.SEIZE_MONSTER);
        // AddPlayerAction(PLAYER_SKILL_TYPE.BREED_MONSTER);
        AddPlayerAction(PLAYER_SKILL_TYPE.AGITATE);
        // AddPlayerAction(PLAYER_SKILL_TYPE.SNATCH);
        AddPlayerAction(PLAYER_SKILL_TYPE.SACRIFICE);
        AddPlayerAction(PLAYER_SKILL_TYPE.RELEASE);
        AddPlayerAction(PLAYER_SKILL_TYPE.HEAL);
        AddPlayerAction(PLAYER_SKILL_TYPE.EXPEL);
        AddPlayerAction(PLAYER_SKILL_TYPE.DRAIN_SPIRIT);
        AddPlayerAction(PLAYER_SKILL_TYPE.LET_GO);
        AddPlayerAction(PLAYER_SKILL_TYPE.FULL_HEAL);
    }
    #endregion

    #region Selecatble
    public override bool IsCurrentlySelected() {
        Character characterToSelect = this;
        if (isLycanthrope) {
            characterToSelect = lycanData.activeForm;
        }
        return UIManager.Instance.monsterInfoUI.isShowing &&
               UIManager.Instance.monsterInfoUI.activeMonster == characterToSelect;
    }
    #endregion

    #region Utilities
    public void SetShowNotificationOnDeath(bool showNotificationOnDeath) {
        this.showNotificationOnDeath = showNotificationOnDeath;
    }
    public void SetIsVolatile(bool isVolatile) {
        isVolatileMonster = isVolatile;
    }
    #endregion
}

[System.Serializable]
public class SaveDataSummon : SaveDataCharacter {
    public SUMMON_TYPE summonType;
    public bool isVolatileMonster;

    public override void Save(Character data) {
        base.Save(data);
        if(data is Summon summon) {
            summonType = summon.summonType;
            isVolatileMonster = summon.isVolatileMonster;
        }
    }
    public override Character Load() {
        Summon summon = CharacterManager.Instance.CreateNewSummon(this);
        return summon;
    }
}