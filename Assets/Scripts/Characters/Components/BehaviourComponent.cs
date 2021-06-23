using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Components;
using Inner_Maps;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Pathfinding;
using UnityEngine.Profiling;
using UtilityScripts;
using Traits;
using UnityEngine.Assertions;

public class BehaviourComponent : CharacterComponent, CharacterEventDispatcher.ILocationListener {
    public List<CharacterBehaviourComponent> currentBehaviourComponents { get; private set; }
    public NPCSettlement attackVillageTarget { get; private set; }
    public Area attackAreaTarget { get; private set; }
    public DemonicStructure attackDemonicStructureTarget { get; private set; }
    public bool isAttackingDemonicStructure { get; private set; }
    public bool hasLayedAnEgg { get; private set; }
    public bool isAgitated { get; private set; }
    public bool subterraneanJustExitedCombat { get; private set; }
    public string defaultBehaviourSetName { get; private set; }
    
    //douse fire
    public NPCSettlement dousingFireForSettlement { get; private set; }
    //cleanse tiles
    public NPCSettlement cleansingTilesForSettlement { get; private set; }
    //cleanse tiles
    public NPCSettlement dryingTilesForSettlement { get; private set; }
    //mining
    //public LocationGridTile targetMiningTile { get; private set; }
    
    //Abduct
    //public ABPath currentAbductDigPath { get; private set; }
    public Character currentAbductTarget { get; private set; }
    
    //De-Mood
    public bool canDeMood => currentDeMoodCooldown >= _deMoodCooldownPeriod;
    public int currentDeMoodCooldown { get; private set; }
    private readonly int _deMoodCooldownPeriod;
    public List<Area> deMoodVillageTarget { get; private set; }
    
    //invade
    public List<Area> invadeVillageTarget { get; private set; }
    public int followerCount { get; private set; }
    
    //disabler
    public bool canDisable => currentDisableCooldown >= _disableCooldownPeriod;
    public int currentDisableCooldown { get; private set; }
    private readonly int _disableCooldownPeriod;
    public Character invaderToFollow { get; private set; }
    
    //abductor
    public LocationGridTile nest { get; private set; }
    public bool hasEatenInTheMorning { get; private set; }
    public bool hasEatenInTheNight { get; private set; }
    
    //arsonist
    public bool canArson => currentArsonCooldown >= _arsonCooldownPeriod;
    public int currentArsonCooldown { get; private set; }
    private readonly int _arsonCooldownPeriod;
    public List<Area> arsonVillageTarget { get; private set; }
    
    //Abomination
    public Area abominationTarget { get; private set; }
    
    //snatcher
    public bool isCurrentlySnatching;

    //pest
    public BaseSettlement pestSettlementTarget { get; private set; }
    public bool pestHasFailedEat { get; private set; }

    //private COMBAT_MODE combatModeBeforeHarassRaidInvade;
    public COMBAT_MODE combatModeBeforeAttackingDemonicStructure { get; private set; }
    
    //socializing
    public LocationStructure targetSocializeStructure { get; private set; }
    public GameDate socializingEndTime { get; private set; }
    
    //visit village
    public NPCSettlement targetVisitVillage { get; private set; }
    public LocationStructure targetVisitVillageStructure { get; private set; }
    public GameDate visitVillageEndTime { get; private set; }
    public VISIT_VILLAGE_INTENT visitVillageIntent { get; private set; }

    public BehaviourComponent () {
        deMoodVillageTarget = new List<Area>();
        invadeVillageTarget = new List<Area>();
        arsonVillageTarget = new List<Area>();

        defaultBehaviourSetName = string.Empty;
        currentBehaviourComponents = new List<CharacterBehaviourComponent>();
        
        //De-Mood
        _deMoodCooldownPeriod = GameManager.ticksPerHour * 2; //2 hours
        currentDeMoodCooldown = _deMoodCooldownPeriod;
        
        //Disabler
        _disableCooldownPeriod = GameManager.ticksPerHour * 2; //2 hours
        currentDisableCooldown = _disableCooldownPeriod;
        
        //Arson
        _arsonCooldownPeriod = GameManager.ticksPerHour * 2; //2 hours
        currentArsonCooldown = _arsonCooldownPeriod;
        
        PopulateInitialBehaviourComponents();
    }
    public BehaviourComponent(SaveDataBehaviourComponent data) {
        deMoodVillageTarget = new List<Area>();
        invadeVillageTarget = new List<Area>();
        arsonVillageTarget = new List<Area>();

        _deMoodCooldownPeriod = GameManager.ticksPerHour * 2; //2 hours
        _disableCooldownPeriod = GameManager.ticksPerHour * 2; //2 hours
        _arsonCooldownPeriod = GameManager.ticksPerHour * 2; //2 hours

        currentBehaviourComponents = new List<CharacterBehaviourComponent>();
        for (int i = 0; i < data.currentBehaviourComponents.Count; i++) {
            CharacterBehaviourComponent component = CharacterManager.Instance.GetCharacterBehaviourComponent(System.Type.GetType(data.currentBehaviourComponents[i]));
            AddBehaviourComponentFromSave(component);
        }
        isAttackingDemonicStructure = data.isAttackingDemonicStructure;
        hasLayedAnEgg = data.hasLayedAnEgg;
        isAgitated = data.isAgitated;
        subterraneanJustExitedCombat = data.subterraneanJustExitedCombat;
        defaultBehaviourSetName = data.defaultBehaviourSetName;
        currentDeMoodCooldown = data.currentDeMoodCooldown;
        followerCount = data.followerCount;
        currentDisableCooldown = data.currentDisableCooldown;
        currentArsonCooldown = data.currentArsonCooldown;
        hasEatenInTheMorning = data.hasEatenInTheMorning;
        hasEatenInTheNight = data.hasEatenInTheNight;
        isCurrentlySnatching = data.isCurrentlySnatching;
        combatModeBeforeAttackingDemonicStructure = data.combatModeBeforeAttackingDemonicStructure;
        pestHasFailedEat = data.pestHasFailedEat;
    }

    #region General
    public void PopulateInitialBehaviourComponents() {
        ChangeDefaultBehaviourSet(CharacterManager.Default_Resident_Behaviour);
    }
    private bool AddBehaviourComponent(CharacterBehaviourComponent component) {
        if(component == null) {
            throw new System.Exception(
                $"{GameManager.Instance.TodayLogString()}{owner.name} is trying to add a new behaviour component but it is null!");
        }
        if (HasBehaviour(component.GetType())) {
            return false;
        }
        bool behaviourAdded = AddBehaviourComponentInOrder(component);
        if (behaviourAdded) {
            component.OnAddBehaviourToCharacter(owner);
        }
        return behaviourAdded;
    }
    private bool AddBehaviourComponentFromSave(CharacterBehaviourComponent component) {
        if (component == null) {
            throw new System.Exception(
                $"{GameManager.Instance.TodayLogString()}{owner.name} is trying to add a new behaviour component but it is null!");
        }
        if (HasBehaviour(component.GetType())) {
            return false;
        }
        bool behaviourAdded = AddBehaviourComponentInOrder(component);
        return behaviourAdded;
    }
    public bool AddBehaviourComponent(System.Type componentType) {
        return AddBehaviourComponent(CharacterManager.Instance.GetCharacterBehaviourComponent(componentType));
    }
    private bool RemoveBehaviourComponent(CharacterBehaviourComponent component) {
        bool wasRemoved = currentBehaviourComponents.Remove(component);
        if (wasRemoved) {
#if DEBUG_LOG
            Debug.Log($"{owner.name} removed character behaviour {component}");
#endif
            component.OnRemoveBehaviourFromCharacter(owner);
            Messenger.Broadcast(CharacterSignals.CHARACTER_REMOVED_BEHAVIOUR, owner, component);
        }
        return wasRemoved;
    }
    public bool RemoveBehaviourComponent(System.Type componentType) {
        return RemoveBehaviourComponent(CharacterManager.Instance.GetCharacterBehaviourComponent(componentType));
    }
    public bool ReplaceBehaviourComponent(CharacterBehaviourComponent componentToBeReplaced, CharacterBehaviourComponent componentToReplace) {
        if (RemoveBehaviourComponent(componentToBeReplaced)) {
            return AddBehaviourComponent(componentToReplace);
        }
        return false;
    }
    public bool ReplaceBehaviourComponent(System.Type componentToBeReplaced, System.Type componentToReplace) {
        if (RemoveBehaviourComponent(componentToBeReplaced)) {
            return AddBehaviourComponent(componentToReplace);
        }
        return false;
    }
    // public bool ReplaceBehaviourComponent(List<CharacterBehaviourComponent> newComponents) {
    //     currentBehaviourComponents.Clear();
    //     for (int i = 0; i < newComponents.Count; i++) {
    //         AddBehaviourComponent(newComponents[i]);
    //     }
    //     return true;
    // }
    private bool AddBehaviourComponentInOrder(CharacterBehaviourComponent component) {
        if (currentBehaviourComponents.Count > 0) {
            for (int i = 0; i < currentBehaviourComponents.Count; i++) {
                if (component.priority > currentBehaviourComponents[i].priority) {
                    currentBehaviourComponents.Insert(i, component);
                    return true;
                }
            }
        }
        currentBehaviourComponents.Add(component);
        return true;
    }
    //private void OnNoLongerAbleResidentsInsideSettlement(NPCSettlement npcSettlement) {
    //    if(assignedTargetSettlement == npcSettlement) {
    //        SetIsInvading(false, null);
    //    }
    //}
    public void ChangeDefaultBehaviourSet(string setName) {
        if(defaultBehaviourSetName != setName) {
            RemoveDefaultBehaviourSet(defaultBehaviourSetName);
            AddDefaultBehaviourSet(setName);
            defaultBehaviourSetName = setName;
        }
    }
    private void AddDefaultBehaviourSet(string setName) {
        System.Type[] defaultBehaviours = CharacterManager.Instance.GetDefaultBehaviourSet(setName);
        if(defaultBehaviours != null) {
            for (int i = 0; i < defaultBehaviours.Length; i++) {
                AddBehaviourComponent(defaultBehaviours[i]);
            }
        }
    }
    private void RemoveDefaultBehaviourSet(string setName) {
        System.Type[] defaultBehaviours = CharacterManager.Instance.GetDefaultBehaviourSet(setName);
        if (defaultBehaviours != null) {
            for (int i = 0; i < defaultBehaviours.Length; i++) {
                RemoveBehaviourComponent(defaultBehaviours[i]);
            }
        }
    }
    public void UpdateDefaultBehaviourSet() {
        if ((owner.isNormalCharacter && !owner.isConsideredRatman) || owner.characterClass.IsZombie()) {
            if(owner.homeSettlement != null) {
                owner.SetIsWanderer(false);
            } else {
                owner.SetIsWanderer(true);
            }
        } else {
            if (owner.minion != null) {
                if (owner.faction != null && owner.faction.isMajorNonPlayer) {
                    ChangeDefaultBehaviourSet(CharacterManager.Default_Resident_Behaviour);
                } else {
                    ChangeDefaultBehaviourSet(CharacterManager.Default_Minion_Behaviour);    
                }
            } else if (owner.race == RACE.ANGEL) {
                ChangeDefaultBehaviourSet(CharacterManager.Default_Angel_Behaviour);
            } else {
                string behaviourSetName = owner.characterClass.className + " Behaviour";
                if (CharacterManager.Instance.HasDefaultBehaviourSet(behaviourSetName)) {
                    ChangeDefaultBehaviourSet(behaviourSetName);
                } else {
                    ChangeDefaultBehaviourSet(CharacterManager.Default_Monster_Behaviour);
                }
            }
        }
    }
    public void OnDeath() {
        //update following invader count. NOTE: Note this is for disablers only
        invaderToFollow?.behaviourComponent.RemoveFollower();
    }
    #endregion

    #region Utilities
    public void PopulateVillageTargetsByPriority(List<Area> areas) {
        //get settlements in region that have normal characters living there.
        List<BaseSettlement> settlementsInRegion = RuinarchListPool<BaseSettlement>.Claim();
        owner.currentRegion?.PopulateSettlementsInRegionForGettingGeneralVillageTargets(settlementsInRegion);
        if (settlementsInRegion.Count > 0) {
            // List<BaseSettlement> villageChoices = settlementsInRegion.Where(x => x.locationType == LOCATION_TYPE.VILLAGE && (x.owner == null || !x.owner.IsFriendlyWith(PlayerManager.Instance.player.playerFaction))).ToList();
            List<BaseSettlement> villageChoices = RuinarchListPool<BaseSettlement>.Claim();
            settlementsInRegion.PopulateSettlementsThatAreUnownedOrHostileWithFaction(villageChoices, LOCATION_TYPE.VILLAGE, PlayerManager.Instance.player.playerFaction);
            if (villageChoices != null) {
                //a random village occupied by Villagers within current region
                BaseSettlement chosenVillage = CollectionUtilities.GetRandomElement(villageChoices);
                areas.AddRange(chosenVillage.areas);
            } else {
                //a random special structure occupied by Villagers within current region
                List<BaseSettlement> specialStructureChoices = RuinarchListPool<BaseSettlement>.Claim();
                settlementsInRegion.PopulateSettlementsThatAreUnownedOrHostileWithFaction(specialStructureChoices, LOCATION_TYPE.DUNGEON, PlayerManager.Instance.player.playerFaction);
                if (specialStructureChoices != null) {
                    BaseSettlement chosenSpecialStructure = CollectionUtilities.GetRandomElement(specialStructureChoices);
                    areas.AddRange(chosenSpecialStructure.areas);
                }
                RuinarchListPool<BaseSettlement>.Release(specialStructureChoices);
            }
            RuinarchListPool<BaseSettlement>.Release(villageChoices);
        }
        RuinarchListPool<BaseSettlement>.Release(settlementsInRegion);

        //no settlements in region.
        //a random area occupied by Villagers within current region
        if (areas.Count <= 0) {
            List<Area> occupiedAreas = RuinarchListPool<Area>.Claim();
            owner.currentRegion?.PopulateAreasOccupiedByVillagers(occupiedAreas);
            if (occupiedAreas != null) {
                Area randomArea = CollectionUtilities.GetRandomElement(occupiedAreas);
                areas.Add(randomArea);
            }
            RuinarchListPool<Area>.Release(occupiedAreas);
        }
    }
    #endregion

    #region Processes
    public string RunBehaviour() {
        string log = string.Empty;
#if DEBUG_LOG
        log = $"{GameManager.Instance.TodayLogString()}{owner.name} Idle Plan Decision Making:";
#endif
        List<CharacterBehaviourComponent> behavioursToProcess = RuinarchListPool<CharacterBehaviourComponent>.Claim();
        behavioursToProcess.AddRange(currentBehaviourComponents); //cached list so that removing a behaviour while in a behaviour will not cause problems
        for (int i = 0; i < behavioursToProcess.Count; i++) {
            CharacterBehaviourComponent component = behavioursToProcess[i];
            if (!currentBehaviourComponents.Contains(component)) {
#if DEBUG_LOG
                log += $"\nBehaviour Component: {component.ToString()} has been removed from {owner.name}'s list of behaviours, skipping it...";
#endif
                continue; //skip component
            }
            if (component.IsDisabledFor(owner)) {
#if DEBUG_LOG
                log += $"\nBehaviour Component: {component.ToString()} is disabled for {owner.name} skipping it...";
#endif
                continue; //skip component
            }
            if (!component.CanDoBehaviour(owner)) {
#if DEBUG_LOG
                log += $"\nBehaviour Component: {component.ToString()} cannot be done by {owner.name} skipping it...";
#endif
                continue; //skip component
            }
#if DEBUG_PROFILER
            Profiler.BeginSample($"{component} - Try Do Behaviour");
#endif
            bool behaviourSuccess = component.TryDoBehaviour(owner, ref log, out JobQueueItem producedJob);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
            if (behaviourSuccess) {
                bool isProducedJobValid = IsProducedJobValid(producedJob, owner);
                if (producedJob == null || isProducedJobValid) {
                    if (producedJob != null) {
                        owner.jobQueue.AddJobInQueue(producedJob);
                    }
                    component.PostProcessAfterSuccessfulDoBehaviour(owner);
                    if (producedJob != null) {
                        //if a job was produced, always stop behaviour loop, regardless if behaviour says it should not skip processing
                        break;
                    } else {
                        //no job was produced, check if current behaviour stops processing
                        if (!component.WillContinueProcess()) { break; }    
                    }
                }
                if (!isProducedJobValid && producedJob != null) { //if produced valid is not valid and produced job is not null
                    //add character to blacklist if job is owned by a settlement
                    if (producedJob.originalOwner != null && producedJob.originalOwner.ownerType != JOB_OWNER.CHARACTER) {
                        producedJob.AddBlacklistedCharacter(owner);
                    }
                }

            }
            if (component.StopsBehaviourLoop()) { break; }
        }
        RuinarchListPool<CharacterBehaviourComponent>.Release(behavioursToProcess);
        return log;
    }
    private bool IsProducedJobValid(JobQueueItem job, Character character) {
        if (job is CharacterStateJob) {
            return true;
        } else if (job is GoapPlanJob goapPlanJob) {
            if (character is Dragon) {
                //if character is dragon then do not check path possibility
                return true;
            }
            if (goapPlanJob.jobType == JOB_TYPE.IDLE_RETURN_HOME) {
                if (character.homeStructure != null) {
                    return character.movementComponent.HasPathToEvenIfDiffRegion(character.homeStructure.GetRandomUnoccupiedTile());
                } else if (character.HasTerritory()) {
                    Area randomTerritory = character.territory;
                    return character.movementComponent.HasPathToEvenIfDiffRegion(CollectionUtilities.GetRandomElement(randomTerritory.gridTileComponent.gridTiles));
                }
            } else if (goapPlanJob.jobType == JOB_TYPE.RESCUE || goapPlanJob.jobType == JOB_TYPE.EXTERMINATE ||
                       goapPlanJob.jobType == JOB_TYPE.EXPLORE || goapPlanJob.jobType == JOB_TYPE.COUNTERATTACK || goapPlanJob.jobType == JOB_TYPE.MONSTER_INVADE) {
                //if job allows digging do not check pathfinding, always allow it.
                //TODO: Add some way to unify this checking instead of using JOB_TYPEs
                return true;
            } else if (goapPlanJob.jobType == JOB_TYPE.PRODUCE_FOOD || goapPlanJob.jobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP) {
                //Do not check path towards produce food target, since target isn't really used for the job,
                //and can produce problems if that object has been destroyed.
                return true;
            }else if (character.behaviourComponent.HasBehaviour(typeof(PangatLooVillageInvaderBehaviour)) && job.jobType == JOB_TYPE.GO_TO) {
                return true; //do not check path for go to job of pangat loo village invader
            }
            
            if (character == goapPlanJob.targetPOI || goapPlanJob.targetPOI == null || (goapPlanJob.targetPOI is TileObject tileObject && tileObject.mapObjectState == MAP_OBJECT_STATE.UNBUILT)) {
                //if target is self, target is null or target is an unbuilt tile object, job is valid.
                return true;
            }
            if (goapPlanJob.targetPOI.gridTileLocation != null) {
                return character.movementComponent.HasPathToEvenIfDiffRegion(goapPlanJob.targetPOI.gridTileLocation, goapPlanJob.targetPOI.gridTileLocation.parentMap.onlyUnwalkableGraph);    
            } else {
                return false;
            }
            
        }
        return false;
    }
    public void OnCharacterFinishedJob(JobQueueItem job) {
        if (job.jobType == JOB_TYPE.DECREASE_MOOD) {
            if (HasBehaviour(typeof(DeMooderBehaviour))) {
                //character finished decrease mood job, start cooldown.
                StartDeMoodCooldown();
                ResetDeMoodVillageTarget();
            }
        } else if (job.jobType == JOB_TYPE.DISABLE) {
            if (HasBehaviour(typeof(DisablerBehaviour))) {
                //character finished disable job, start cooldown.
                StartDisablerCooldown();
            }
        } else if (job.jobType == JOB_TYPE.MONSTER_EAT) {
            if (HasBehaviour(typeof(AbductorBehaviour))) {
                TIME_IN_WORDS currentTimeInWords = GameManager.Instance.GetCurrentTimeInWordsOfTick();
                switch (currentTimeInWords) {
                    case TIME_IN_WORDS.MORNING:
                    case TIME_IN_WORDS.AFTERNOON:
                    case TIME_IN_WORDS.LUNCH_TIME:
                        SetHasEatenInTheMorning(true);
                        break;
                    case TIME_IN_WORDS.EARLY_NIGHT:
                    case TIME_IN_WORDS.LATE_NIGHT:
                        SetHasEatenInTheNight(true);
                        break;
                }
            }
        } else if (job.jobType == JOB_TYPE.MONSTER_ABDUCT) {
            if (job is GoapPlanJob goapJob && goapJob.targetPOI is Character targetCharacter) {
                if (HasBehaviour(typeof(AbductorBehaviour))) {
                    targetCharacter.defaultCharacterTrait.SetHasBeenAbductedByPlayerMonster(true);
                }
                if (owner is GiantSpider) {
                    targetCharacter.traitContainer.AddTrait(targetCharacter, "Webbed", owner);
                    targetCharacter.defaultCharacterTrait.SetHasBeenAbductedByWildMonster(true);
                }
            }
        } else if (job.jobType == JOB_TYPE.ARSON) {
            if (HasBehaviour(typeof(ArsonistBehaviour))) {
                //clear target village data after 3 hours after first successful arson job.
                GameDate expiry = GameManager.Instance.Today();
                expiry.AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
                SchedulingManager.Instance.AddEntry(expiry, ResetArsonistVillageTarget, owner);
                SchedulingManager.Instance.AddEntry(expiry, StartArsonistCooldown, owner);
            }
        }
    }
    #endregion

    #region Inquiry
    public bool HasBehaviour(System.Type type) {
        for (int i = 0; i < currentBehaviourComponents.Count; i++) {
            CharacterBehaviourComponent cbc = currentBehaviourComponents[i];
            if (cbc.GetType() == type) {
                return true;
            }
        }
        return false;
    }
#endregion

    #region Attack Demonic Structure
    public void SetIsAttackingDemonicStructure(bool state, DemonicStructure target) {
        if (isAttackingDemonicStructure != state) {
            isAttackingDemonicStructure = state;
            SetDemonicStructureTarget(target);
            owner.jobQueue.CancelAllJobs();
            if (isAttackingDemonicStructure) {
                combatModeBeforeAttackingDemonicStructure = owner.combatComponent.combatMode;
                owner.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
                owner.movementComponent.SetEnableDigging(true);
                AddBehaviourComponent(typeof(AttackDemonicStructureBehaviour));
                owner.traitContainer.AddTrait(owner, "Fervor");
                StartCheckingIfShouldStopAttackingDemonicStructure();
            } else {
                owner.combatComponent.SetCombatMode(combatModeBeforeAttackingDemonicStructure);
                RemoveBehaviourComponent(typeof(AttackDemonicStructureBehaviour));
                owner.traitContainer.RemoveTrait(owner, "Fervor");
                owner.movementComponent.SetEnableDigging(false);
                StopCheckingIfShouldStopAttackingDemonicStructure();
            }
        }
    }
    public void SetDemonicStructureTarget(DemonicStructure target) {
        attackDemonicStructureTarget = target;
    }
    private void StartCheckingIfShouldStopAttackingDemonicStructure() {
        // Messenger.AddListener<Character, ActualGoapNode>(Signals.CHARACTER_DOING_ACTION, OnActionStartedWhileAttackingDemonicStructure);
        // Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
        //Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
    }
    private void StopCheckingIfShouldStopAttackingDemonicStructure() {
        // Messenger.RemoveListener<Character, ActualGoapNode>(Signals.CHARACTER_DOING_ACTION, OnActionStartedWhileAttackingDemonicStructure);
        // Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
    }
    //private void OnActionStartedWhileAttackingDemonicStructure(Character actor, ActualGoapNode action) {
    //    if (actor == owner) {
    //        if (action.action.goapType != INTERACTION_TYPE.ATTACK_DEMONIC_STRUCTURE) {
    //            SetIsAttackingDemonicStructure(false, null);
    //        }
    //    }
    //}
    //private void OnCharacterCanNoLongerPerform(Character character) {
    //    if (character == owner) {
    //        if(owner is Summon summon && (summon.summonType == SUMMON_TYPE.Magical_Angel || summon.summonType == SUMMON_TYPE.Warrior_Angel)) {
    //            //Angels should not remove attack demonic structure
    //        } else {
    //            SetIsAttackingDemonicStructure(false, null);
    //        }
    //    }
    //}
    private void OnJobRemovedFromCharacter(JobQueueItem job, Character character) {
        if (character == owner) {
            //if (job is GoapPlanJob goapJob) {
            //    if (job.jobType == JOB_TYPE.ASSAULT_DEMONIC_STRUCTURE && goapJob.finishedSuccessfully == false) {
            //        SetIsAttackingDemonicStructure(false, null);
            //    }    
            //} else if (job is CharacterStateJob stateJob) {
            //    if (stateJob.assignedState is CombatState combatState && combatState.endedInternally == false) {
            //        SetIsAttackingDemonicStructure(false, null);
            //    }
            //}    
        }
    }
    #endregion

    #region Priority
    public int GetHighestBehaviourPriority() {
        if (currentBehaviourComponents.Count > 0) {
            return currentBehaviourComponents[0].priority;
        }
        return -1;
    }
    #endregion

    #region Douse Fire
    public void SetDouseFireSettlement(NPCSettlement settlement) {
        if (settlement == null) {
            //remove douser from previous settlements list, if any.
            dousingFireForSettlement?.settlementJobTriggerComponent.RemoveDouser(owner);
        } else {
            settlement.settlementJobTriggerComponent.AddDouser(owner);
        }
        dousingFireForSettlement = settlement;
    }
    #endregion

    #region Cleanse Tiles
    public void SetCleansingTilesForSettlement(NPCSettlement settlement) {
        if (settlement == null) {
            //remove poison cleanser from previous settlements list, if any.
            cleansingTilesForSettlement?.settlementJobTriggerComponent.RemovePoisonCleanser(owner);
        } else {
            settlement.settlementJobTriggerComponent.AddPoisonCleanser(owner);
        }
        cleansingTilesForSettlement = settlement;
        
    }
    #endregion
    
    // #region Dry Tiles
    // public void SetDryingTilesForSettlement(NPCSettlement settlement) {
    //     if (settlement == null) {
    //         //remove tile dryer from previous settlements list, if any.
    //         dryingTilesForSettlement?.settlementJobTriggerComponent.RemoveTileDryer(owner);
    //     } else {
    //         settlement.settlementJobTriggerComponent.AddTileDryer(owner);
    //     }
    //     dryingTilesForSettlement = settlement;
    //     
    // }
    // #endregion

    //#region Mining
    //public void SetTargetMiningTile(LocationGridTile tile) {
    //    targetMiningTile = tile;
    //}
    //#endregion

    #region Attack Village
    public void SetAttackVillageTarget(NPCSettlement npcSettlement) {
        attackVillageTarget = npcSettlement;
    }
    public void SetAttackAreaTarget(Area p_area) {
        attackAreaTarget = p_area;
    }
    public void ClearAttackVillageData() {
        SetAttackAreaTarget(null);
        SetAttackVillageTarget(null);
        if (HasBehaviour(typeof(AttackVillageBehaviour))) {
            RemoveBehaviourComponent(typeof(AttackVillageBehaviour));
        }
    }
    #endregion

    #region Abduction
    //public void SetDigForAbductionPath(ABPath path) {
    //    currentAbductDigPath = path;
    //}
    public void SetAbductionTarget(Character character) {
        currentAbductTarget = character;
    }
    #endregion

    #region Summon Specific
    public void OnSummon(LocationGridTile tile) {
        if (HasBehaviour(typeof(AbductorBehaviour))) {
            //if character is an abductor, set its nest to where it was summoned
            SetNest(tile);
        }
    }
    #endregion
    
    #region De-Mood
    private void StartDeMoodCooldown() {
        currentDeMoodCooldown = 0;
        Messenger.AddListener(Signals.TICK_ENDED, PerTickDeMoodCooldown);
    }
    private void PerTickDeMoodCooldown() {
        if (currentDeMoodCooldown >= _deMoodCooldownPeriod) {
            Messenger.RemoveListener(Signals.TICK_ENDED, PerTickDeMoodCooldown);    
        }
        currentDeMoodCooldown++;
    }
    public void ResetDeMoodVillageTarget() {
        deMoodVillageTarget.Clear();
    }
    #endregion

    #region Invade
    public void ResetInvadeVillageTarget() {
        invadeVillageTarget.Clear();
    }
    public void AddFollower() {
        followerCount++;
    }
    public void RemoveFollower() {
        followerCount--;
    }
    #endregion

    #region Disabler
    private void StartDisablerCooldown() {
        currentDisableCooldown = 0;
        Messenger.AddListener(Signals.TICK_ENDED, PerTickDisablerCooldown);
    }
    private void PerTickDisablerCooldown() {
        if (currentDisableCooldown >= _disableCooldownPeriod) {
            Messenger.RemoveListener(Signals.TICK_ENDED, PerTickDisablerCooldown);    
        }
        currentDisableCooldown++;
    }
    public void SetInvaderToFollow(Character characterToFollow) {
        Character previousInvaderToFollow = invaderToFollow;
        invaderToFollow = characterToFollow;
        previousInvaderToFollow?.behaviourComponent.RemoveFollower();
        if (invaderToFollow != null) {
            invaderToFollow.behaviourComponent.AddFollower();
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckIfInvaderToFollowDied);
            Messenger.AddListener<Character>(CharacterSignals.START_FLEE, OnCharacterStartedFleeing);
        } else {
            Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckIfInvaderToFollowDied);
            Messenger.RemoveListener<Character>(CharacterSignals.START_FLEE, OnCharacterStartedFleeing);
        }
    }
    private void OnCharacterStartedFleeing(Character character) {
        if (character == owner) {
            //if this character started fleeing, stop following target invader.
            SetInvaderToFollow(null);   
        }
    }
    private void CheckIfInvaderToFollowDied(Character character) {
        if (character == invaderToFollow) {
            SetInvaderToFollow(null);
        }
    }
    #endregion

    #region Infestor
    public void SetHasLayedAnEgg(bool state) {
        if(hasLayedAnEgg != state) {
            hasLayedAnEgg = state;
            if (hasLayedAnEgg) {
                GameDate dueDate = GameManager.Instance.Today();
                dueDate.AddDays(2);
                SchedulingManager.Instance.AddEntry(dueDate, () => SetHasLayedAnEgg(false), owner);
            }
        }
    }
    #endregion

    #region Abductor
    private void SetNest(LocationGridTile tile) {
        nest = tile;
    }
    public void OnBecomeAbductor() {
        Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnAbductorAddedJobToQueue);
        Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnAbductorRemovedJobFromQueue);
    }
    public void OnNoLongerAbductor() {
        Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnAbductorAddedJobToQueue);
        Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnAbductorRemovedJobFromQueue);
    }
    private void OnAbductorRemovedJobFromQueue(JobQueueItem job, Character character) {
        if (character == owner && job.jobType == JOB_TYPE.MONSTER_ABDUCT) {
            if (character is Summon summon) {
                character.combatComponent.SetCombatMode(summon.defaultCombatMode);    
            } else {
                character.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
            }
            
        }
    }
    private void OnAbductorAddedJobToQueue(JobQueueItem job, Character character) {
        if (character == owner && job.jobType == JOB_TYPE.MONSTER_ABDUCT && character.combatComponent.combatMode != COMBAT_MODE.Defend) {
            character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
        }
    }
    public bool AlreadyHasAbductedVictimAtNest(out Character target) {
        for (int i = 0; i < nest.charactersHere.Count; i++) {
            Character character = nest.charactersHere[i];
            if (character.limiterComponent.canMove == false) {
                target = character;
                return true;
            }
        }
        target = null;
        return false;
    }
    public bool IsNestBlocked(out IPointOfInterest blocker) {
        if (nest.tileObjectComponent.objHere != null) {
            blocker = nest.tileObjectComponent.objHere;
            return true;
        }
        blocker = null;
        return false;
    }
    private void SetHasEatenInTheMorning(bool state) {
        hasEatenInTheMorning = state;
        if (state) {
            //reset value the next day
            GameDate resetDate = GameManager.Instance.Today();
            resetDate.AddDays(1);
            resetDate.SetTicks(1);
            SchedulingManager.Instance.AddEntry(resetDate, () => SetHasEatenInTheMorning(false), owner);
        }
    }
    private void SetHasEatenInTheNight(bool state) {
        hasEatenInTheNight = state;
        if (state) {
            //reset value the next day
            GameDate resetDate = GameManager.Instance.Today();
            resetDate.AddDays(1);
            resetDate.SetTicks(1);
            SchedulingManager.Instance.AddEntry(resetDate, () => SetHasEatenInTheNight(false), owner);
        }
    }
    #endregion

    #region Arsonist
    public void ResetArsonistVillageTarget() {
        arsonVillageTarget.Clear();
    }
    private void StartArsonistCooldown() {
        currentArsonCooldown = 0;
        Messenger.AddListener(Signals.TICK_ENDED, PerTickArsonistCooldown);
    }
    private void PerTickArsonistCooldown() {
        if (currentArsonCooldown >= _arsonCooldownPeriod) {
            Messenger.RemoveListener(Signals.TICK_ENDED, PerTickArsonistCooldown);    
        }
        currentArsonCooldown++;
    }
    public void OnBecomeArsonist() {
        Messenger.AddListener<Character>(CharacterSignals.START_FLEE, OnArsonistStartedFleeing);
    }
    public void OnNoLongerArsonist() {
        Messenger.RemoveListener<Character>(CharacterSignals.START_FLEE, OnArsonistStartedFleeing);
    }
    private void OnArsonistStartedFleeing(Character character) {
        if (character == owner) {
            //once arson starts fleeing, clear target village and start cooldown.
            ResetArsonistVillageTarget();
            StartArsonistCooldown();
        }
    }
    #endregion

    #region Abomination
    public void SetAbominationTarget(Area p_area) {
        abominationTarget = p_area;
        if (abominationTarget != null) {
            //schedule it to be cleared after 5 hours
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(5));
            SchedulingManager.Instance.AddEntry(dueDate, () => SetAbominationTarget(null), owner);
        }
    }
    #endregion

    #region Agitate
    public void SetIsAgitated(bool state) {
        if(isAgitated != state) {
            isAgitated = state;
            if (isAgitated) {
                owner.traitContainer.AddTrait(owner, "Agitated");
            } else {
                owner.traitContainer.RemoveTrait(owner, "Agitated");
            }
        }
    }
    #endregion

    #region Subterranean
    public void SetSubterraneanJustExitedCombat(bool state) {
        subterraneanJustExitedCombat = state;
    }
    #endregion

    #region Snatcher
    public void SetIsSnatching(bool state) {
        isCurrentlySnatching = state;
    }
    public void OnBecomeSnatcher() {
        //Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnSnatcherAddedJobToQueue);
        //Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnSnatchJobRemoved);   
    }
    public void OnNoLongerSnatcher() {
        //Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnSnatcherAddedJobToQueue);
        //Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnSnatchJobRemoved);   
    }
    private void OnSnatcherAddedJobToQueue(JobQueueItem job, Character character) {
        if (character == owner && job.jobType == JOB_TYPE.SNATCH && character.combatComponent.combatMode != COMBAT_MODE.Defend) {
            character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
        }
    }
    private void OnSnatchJobRemoved(JobQueueItem job, Character character) {
        if (character == owner && job.jobType == JOB_TYPE.SNATCH) {
            SetIsSnatching(false);
            if (character is Summon summon) {
                character.combatComponent.SetCombatMode(summon.defaultCombatMode);
            } else {
                character.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
            }
        }
    }
    #endregion

    #region Cultist
    public void OnBecomeCultist() {
        Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnCultistSnatchAddedJobToQueue);
        Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnCultistSnatchJobRemoved);   
    }
    public void OnNoLongerCultist() {
        Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnCultistSnatchAddedJobToQueue);
        Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnCultistSnatchJobRemoved);   
    }
    private void OnCultistSnatchAddedJobToQueue(JobQueueItem job, Character character) {
        if (character == owner && job.jobType == JOB_TYPE.SNATCH && character.combatComponent.combatMode != COMBAT_MODE.Defend) {
            character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
        }
    }
    private void OnCultistSnatchJobRemoved(JobQueueItem job, Character character) {
        if (character == owner && job.jobType == JOB_TYPE.SNATCH) {
            SetIsSnatching(false);
            //since characters are Aggressive by default
            character.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
        }
    }
    #endregion

    #region Dazed
    public void OnBecomeDazed() {
        Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, OnCharacterEnteredArea);
        Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnDazedCharacterArrivedAtStructure);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnDazedCharacterCanNoLongerPerform);
        Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnDazedCharacterStartedState);
    }
    private void OnCharacterEnteredArea(Character character, Area p_area) {
        if (character == owner) {
            if (character.homeSettlement != null && character.homeSettlement.areas.Contains(p_area)) {
                character.traitContainer.RemoveTrait(character, "Dazed");
            } else if (character.IsTerritory(p_area)) {
                character.traitContainer.RemoveTrait(character, "Dazed");    
            }
        }
    }

    private void OnDazedCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        if (character == owner && character.homeStructure != null && character.homeStructure == structure) {
            character.traitContainer.RemoveTrait(character, "Dazed");
        }
    }
    private void OnDazedCharacterCanNoLongerPerform(Character character) {
        if (character == owner) {
            character.traitContainer.RemoveTrait(character, "Dazed");
        }
    }
    private void OnDazedCharacterStartedState(Character character, CharacterState state) {
        if (character == owner && state is CombatState) {
            character.traitContainer.RemoveTrait(character, "Dazed");
        }
    }
    public void OnNoLongerDazed() {
        Messenger.RemoveListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, OnCharacterEnteredArea);
        Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnDazedCharacterArrivedAtStructure);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnDazedCharacterCanNoLongerPerform);
        Messenger.RemoveListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnDazedCharacterStartedState);
    }
    #endregion

    #region Pest
    public void SetPestSettlementTarget(BaseSettlement p_settlement) {
        pestSettlementTarget = p_settlement;
    }
    public void SetPestHasFailedEat(bool p_state) {
        pestHasFailedEat = p_state;
    }
    #endregion

    #region Work
    public bool PlanSettlementOrFactionWorkActions(out JobQueueItem producedJob) {
        //Stationary characters like Wurm cannot take work jobs
        if (owner.limiterComponent.canTakeJobs && !owner.movementComponent.isStationary) {
            //NOTE: ONLY ADDED FACTION CHECKING BECAUSE OF BUG THAT VAGRANTS ARE STILL PART OF A VILLAGE
            if (owner.isAtHomeRegion && owner.homeSettlement != null && owner.homeSettlement.owner == owner.faction) {
                //check npcSettlement job queue, if it has any jobs that target an object that is in view of the owner
                JobQueueItem jobToAssign = owner.homeSettlement.GetFirstJobBasedOnVision(owner);
                if (jobToAssign != null) {
                    producedJob = jobToAssign;
                    //took job based from vision
                    return true;
                } else {
                    //if none of the jobs targets can be seen by the owner, try and get a job from the npcSettlement or faction
                    //regardless of vision instead.
                    if (owner.homeSettlement.HasPathTowardsTileInSettlement(owner, 2)) {
                        if (owner.faction != null) {
                            jobToAssign = owner.faction.GetFirstUnassignedJobToCharacterJob(owner);
                        }

                        //Characters should only take non-vision settlement jobs if they have a path towards the settlement
                        //Reference: https://trello.com/c/SSYDok6x/1106-owners-should-only-take-non-vision-settlement-jobs-if-they-have-a-path-towards-the-settlement
                        if (jobToAssign == null) {
                            jobToAssign = owner.homeSettlement.GetFirstUnassignedJobToCharacterJob(owner);
                        }
                    }

                    if (jobToAssign != null) {
                        producedJob = jobToAssign;
                        return true;
                    }
                }
            }
            if (owner.faction != null) {
                JobQueueItem jobToAssign = owner.faction.GetFirstUnassignedJobToCharacterJob(owner);
                if (jobToAssign != null) {
                    producedJob = jobToAssign;
                    return true;
                }
            }
        }
        producedJob = null;
        return false;
    }
    #endregion

    #region Recruit
    public bool CanCharacterBeRecruitedBy(Character recruiter) {
        if (recruiter.faction == null || owner.faction == recruiter.faction
            || owner.race == RACE.TRITON) {
            //Tritons cannot be tamed/recruited
            return false;
        }
        // if (targetCharacter.faction?.factionType.type == FACTION_TYPE.Undead) {
        //     return false;
        // }
        if (!owner.traitContainer.HasTrait("Restrained")) {
            return false;
        }
        if (owner.HasJobTargetingThis(JOB_TYPE.RECRUIT)) {
            return false;
        }
        if (!recruiter.faction.ideologyComponent.DoesCharacterFitCurrentIdeologies(owner)) {
            //Cannot recruit characters that does not fit faction ideologies
            return false;
        }
        if (recruiter.faction.IsCharacterBannedFromJoining(owner)) {
            //Cannot recruit banned characters
            return false;
        }
        Prisoner prisoner = owner.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
        if (prisoner == null || !prisoner.IsFactionPrisonerOf(recruiter.faction)) {
            //Only recruit characters that are prisoners of the recruiters faction.
            //This was added because sometimes vampire lords will recruit their imprisoned blood sources
            return false;
        }
        return true;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataBehaviourComponent data) {
        if (!string.IsNullOrEmpty(data.attackVillageTarget)) {
            attackVillageTarget = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.attackVillageTarget) as NPCSettlement;
        }
        if (!string.IsNullOrEmpty(data.attackAreaTarget)) {
            attackAreaTarget = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.attackAreaTarget);
        }
        if (!string.IsNullOrEmpty(data.attackDemonicStructureTarget)) {
            attackDemonicStructureTarget = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.attackDemonicStructureTarget) as DemonicStructure;
        }
        if (!string.IsNullOrEmpty(data.dousingFireForSettlement)) {
            dousingFireForSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.dousingFireForSettlement) as NPCSettlement;
        }
        if (!string.IsNullOrEmpty(data.cleansingTilesForSettlement)) {
            cleansingTilesForSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.cleansingTilesForSettlement) as NPCSettlement;
        }
        // if (!string.IsNullOrEmpty(data.dryingTilesForSettlement)) {
        //     dryingTilesForSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.dryingTilesForSettlement) as NPCSettlement;
        // }
        //if (data.targetMiningTile.hasValue) {
        //    targetMiningTile = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(data.targetMiningTile);
        //}
        if (!string.IsNullOrEmpty(data.currentAbductTarget)) {
            currentAbductTarget = CharacterManager.Instance.GetCharacterByPersistentID(data.currentAbductTarget);
        }
        if (!string.IsNullOrEmpty(data.invaderToFollow)) {
            invaderToFollow = CharacterManager.Instance.GetCharacterByPersistentID(data.invaderToFollow);
        }
        if (data.nest.hasValue) {
            nest = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(data.nest);
        }
        if (!string.IsNullOrEmpty(data.abominationTarget)) {
            abominationTarget = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.abominationTarget);
        }
        if (data.deMoodVillageTarget != null) {
            for (int i = 0; i < data.deMoodVillageTarget.Count; i++) {
                Area area = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.deMoodVillageTarget[i]);
                deMoodVillageTarget.Add(area);
            }
        }
        if (data.invadeVillageTarget != null) {
            for (int i = 0; i < data.invadeVillageTarget.Count; i++) {
                Area area = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.invadeVillageTarget[i]);
                invadeVillageTarget.Add(area);
            }
        }
        if (data.arsonVillageTarget != null) {
            for (int i = 0; i < data.arsonVillageTarget.Count; i++) {
                Area area = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.arsonVillageTarget[i]);
                arsonVillageTarget.Add(area);
            }
        }
        if (data.pestSettlementTarget != null) {
            if (!string.IsNullOrEmpty(data.pestSettlementTarget)) {
                pestSettlementTarget = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.pestSettlementTarget);
            }
            //pestSettlementTarget = new List<HexTile>();
            //for (int i = 0; i < data.pestSettlementTarget.Count; i++) {
            //    HexTile hex = DatabaseManager.Instance.hexTileDatabase.GetHextileByPersistentID(data.pestSettlementTarget[i]);
            //    pestSettlementTarget.Add(hex);
            //}
        }
        if (!string.IsNullOrEmpty(data.targetSocializeStructure)) {
            targetSocializeStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.targetSocializeStructure);
            owner.eventDispatcher.SubscribeToCharacterArrivedAtStructure(this);
        }
        if (data.socializingEndTime.hasValue) {
            ScheduleSocializeEnd(data.socializingEndTime);
            owner.eventDispatcher.UnsubscribeToCharacterArrivedAtStructure(this);
        }
        if (!string.IsNullOrEmpty(data.targetVisitVillage)) {
            targetVisitVillage = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.targetVisitVillage) as NPCSettlement;
            owner.eventDispatcher.SubscribeToCharacterArrivedAtSettlement(this);
        }
        if (!string.IsNullOrEmpty(data.targetVisitVillageStructure)) {
            targetVisitVillageStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.targetVisitVillageStructure);
            //unsubscribe from this signal, since we expect that if the character has a
            //target village structure, that he/she is already at his/her target village
            owner.eventDispatcher.UnsubscribeToCharacterArrivedAtSettlement(this); 
        }
        if (data.visitVillageEndTime.hasValue) {
            ScheduleVisitVillageEnd(data.visitVillageEndTime);
        }
        visitVillageIntent = data.visitVillageIntent;
        if (data.currentBehaviourComponents != null) {
            for (int i = 0; i < data.currentBehaviourComponents.Count; i++) {
                string behaviourStr = data.currentBehaviourComponents[i];
                CharacterBehaviourComponent component = CharacterManager.Instance.GetCharacterBehaviourComponent(System.Type.GetType(behaviourStr));
                component.OnLoadBehaviourToCharacter(owner);
            }    
        }
    }
    #endregion

    #region Demonic Defender
    public void OnBecomeDemonicDefender() {
        Messenger.AddListener<Character, DemonicStructure>(CharacterSignals.CHARACTER_HIT_DEMONIC_STRUCTURE, OnCharacterHitDemonicStructure);
        Messenger.AddListener<Character, Character>(CharacterSignals.CHARACTER_WAS_HIT, OnCharacterAttacked);
    }
    public void OnNoLongerDemonicDefender() {
        Messenger.RemoveListener<Character, DemonicStructure>(CharacterSignals.CHARACTER_HIT_DEMONIC_STRUCTURE, OnCharacterHitDemonicStructure);
        Messenger.RemoveListener<Character, Character>(CharacterSignals.CHARACTER_WAS_HIT, OnCharacterAttacked);
    }
    private void OnCharacterHitDemonicStructure(Character p_attacker, DemonicStructure p_demonicStructure) {
        if (!owner.combatComponent.isInCombat && owner.limiterComponent.canMove && owner.limiterComponent.canPerform && 
            (owner.currentActionNode == null || owner.currentActionNode.action.goapType != INTERACTION_TYPE.ASSAULT)) {
            //if defender is not currently in combat and a demonic structure was hit, attack the character that hit it.
            owner.combatComponent.Fight(p_attacker, CombatManager.Defending_Home, null, true);
        }
    }
    private void OnCharacterAttacked(Character p_hitCharacter, Character p_attacker) {
        if (!owner.combatComponent.isInCombat && owner.limiterComponent.canMove && owner.limiterComponent.canPerform && p_hitCharacter != owner && 
            p_hitCharacter.gridTileLocation != null && p_hitCharacter.gridTileLocation.corruptionComponent.isCorrupted &&
            p_hitCharacter.partyComponent.currentParty != null && owner.partyComponent.currentParty != null &&
            p_hitCharacter.partyComponent.currentParty == owner.partyComponent.currentParty && 
            (owner.currentActionNode == null || owner.currentActionNode.action.goapType != INTERACTION_TYPE.ASSAULT)) {
            //if defender is not currently in combat and a party member was hit, attack the character that hit it.
            owner.combatComponent.Fight(p_attacker, CombatManager.Defending_Home, null, true);
        }
    }
    #endregion

    #region Socializing
    public void GoSocializing(Character p_character, LocationStructure p_targetStructure) {
        targetSocializeStructure = p_targetStructure;
        Assert.IsNotNull(targetSocializeStructure);
        p_character.behaviourComponent.AddBehaviourComponent(typeof(SocializingBehaviour));
        //listen for when the character arrives at the target structure.
        p_character.eventDispatcher.SubscribeToCharacterArrivedAtStructure(this);
        Debug.Log($"{p_character.name} will socialize at {targetSocializeStructure.name}");
    }
    public void OnCharacterLeftStructure(Character p_character, LocationStructure p_leftStructure) { }
    public void OnCharacterArrivedAtStructure(Character p_character, LocationStructure p_arrivedStructure) {
        if (p_arrivedStructure == targetSocializeStructure) {
            OnCharacterArrivedAtTargetSocializingStructure(p_character, p_arrivedStructure);
        }
    }
    private void OnCharacterArrivedAtTargetSocializingStructure(Character p_character, LocationStructure p_structure) {
        Assert.IsTrue(p_structure == targetSocializeStructure);
        p_character.eventDispatcher.UnsubscribeToCharacterArrivedAtStructure(this);
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
        ScheduleSocializeEnd(dueDate);
    }
    private void ScheduleSocializeEnd(GameDate p_date) {
        socializingEndTime = p_date;
        SchedulingManager.Instance.AddEntry(p_date, EndSocializeOnSchedule, this);
        Debug.Log($"{GameManager.Instance.TodayLogString()}Scheduled socialize end of {owner.name} to {socializingEndTime.ToString()}");
    }
    private void EndSocializeOnSchedule() {
        ClearOutSocializingBehaviour();
    }
    public void ClearOutSocializingBehaviour() {
        owner.behaviourComponent.RemoveBehaviourComponent(typeof(SocializingBehaviour));
        socializingEndTime = default;
        targetSocializeStructure = null;
    }
    #endregion

    #region Visit Village
    public void VisitVillage(Character p_character, NPCSettlement p_settlement) {
        targetVisitVillage = p_settlement;
        Assert.IsNotNull(targetVisitVillage);
        p_character.behaviourComponent.AddBehaviourComponent(typeof(VisitVillageBehaviour));
        //listen for when the character arrives at the target structure.
        p_character.eventDispatcher.SubscribeToCharacterArrivedAtSettlement(this);
        Debug.Log($"{GameManager.Instance.TodayLogString()}{p_character.name} will visit village at {targetVisitVillage.name}");
    }
    public void OnCharacterArrivedAtSettlement(Character p_character, NPCSettlement p_settlement) {
        if (p_settlement == targetVisitVillage) {
            OnCharacterArrivedAtTargetVillageSettlement(p_character, p_settlement);
        }
    }
    private void OnCharacterArrivedAtTargetVillageSettlement(Character p_character, NPCSettlement p_settlement) {
        Assert.IsTrue(p_settlement == targetVisitVillage);
        p_character.eventDispatcher.UnsubscribeToCharacterArrivedAtSettlement(this);
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
        ScheduleVisitVillageEnd(dueDate);
        SetVisitVillageIntent(VISIT_VILLAGE_INTENT.Socialize);
        if (p_settlement.HasStructure(STRUCTURE_TYPE.TAVERN)) {
            SetTargetVisitVillageStructure(GameUtilities.RollChance(50) ? p_settlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN) : p_settlement.cityCenter);
        } else {
            SetTargetVisitVillageStructure(p_settlement.cityCenter);
        }
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}Set target visit village structure to {targetVisitVillageStructure.name}");
#endif        
    }
    public void SetVisitVillageIntent(VISIT_VILLAGE_INTENT p_intent) {
        visitVillageIntent = p_intent;
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}Set visit village intent of {owner.name} to {visitVillageIntent}");
#endif
    }

    public void SetTargetVisitVillageStructure(LocationStructure p_structure) {
        targetVisitVillageStructure = p_structure;
    }
    private void ScheduleVisitVillageEnd(GameDate p_date) {
        visitVillageEndTime = p_date;
        SchedulingManager.Instance.AddEntry(p_date, EndVisitVillageOnSchedule, this);
        Debug.Log($"{GameManager.Instance.TodayLogString()}Scheduled visit village end of {owner.name} to {visitVillageEndTime.ToString()}");
    }
    private void EndVisitVillageOnSchedule() {
        ClearOutVisitVillageBehaviour();
    }
    public void ClearOutVisitVillageBehaviour() {
        owner.behaviourComponent.RemoveBehaviourComponent(typeof(VisitVillageBehaviour));
        visitVillageEndTime = default;
        targetVisitVillage = null;
        targetVisitVillageStructure = null;
        visitVillageIntent = VISIT_VILLAGE_INTENT.Socialize;
    }
    #endregion
}

[System.Serializable]
public class SaveDataBehaviourComponent : SaveData<BehaviourComponent> {
    public List<string> currentBehaviourComponents;
    public string attackVillageTarget;
    public string attackAreaTarget;
    public string attackDemonicStructureTarget; //Must be demonic structure when loaded
    public bool isAttackingDemonicStructure;
    public bool hasLayedAnEgg;
    public bool isAgitated;
    public bool subterraneanJustExitedCombat;
    public string defaultBehaviourSetName;

    //douse fire
    public string dousingFireForSettlement;
    //cleanse tiles
    public string cleansingTilesForSettlement;
    //cleanse tiles
    // public string dryingTilesForSettlement;
    ////mining
    //public TileLocationSave targetMiningTile;

    //Abduct
    public string currentAbductTarget;

    //De-Mood
    public int currentDeMoodCooldown;
    public List<string> deMoodVillageTarget;

    //invade
    public List<string> invadeVillageTarget;
    public int followerCount;

    //disabler
    public int currentDisableCooldown;
    public string invaderToFollow;

    //abductor
    public TileLocationSave nest;
    public bool hasEatenInTheMorning;
    public bool hasEatenInTheNight;

    //arsonist
    public int currentArsonCooldown;
    public List<string> arsonVillageTarget;

    //Abomination
    public string abominationTarget;

    //snatcher
    public bool isCurrentlySnatching;

    //pest
    public string pestSettlementTarget;
    public bool pestHasFailedEat;

    public COMBAT_MODE combatModeBeforeAttackingDemonicStructure;
    
    //socializing
    public string targetSocializeStructure;
    public GameDate socializingEndTime;
    
    //visit village
    public string targetVisitVillage;
    public string targetVisitVillageStructure;
    public GameDate visitVillageEndTime;
    public VISIT_VILLAGE_INTENT visitVillageIntent;

    #region Overrides
    public override void Save(BehaviourComponent data) {
        currentBehaviourComponents = new List<string>();
        for (int i = 0; i < data.currentBehaviourComponents.Count; i++) {
            currentBehaviourComponents.Add(data.currentBehaviourComponents[i].ToString());
        }
        isAttackingDemonicStructure = data.isAttackingDemonicStructure;
        hasLayedAnEgg = data.hasLayedAnEgg;
        isAgitated = data.isAgitated;
        subterraneanJustExitedCombat = data.subterraneanJustExitedCombat;
        defaultBehaviourSetName = data.defaultBehaviourSetName;
        currentDeMoodCooldown = data.currentDeMoodCooldown;
        followerCount = data.followerCount;
        currentDisableCooldown = data.currentDisableCooldown;
        currentArsonCooldown = data.currentArsonCooldown;
        hasEatenInTheMorning = data.hasEatenInTheMorning;
        hasEatenInTheNight = data.hasEatenInTheNight;
        isCurrentlySnatching = data.isCurrentlySnatching;
        combatModeBeforeAttackingDemonicStructure = data.combatModeBeforeAttackingDemonicStructure;
        pestHasFailedEat = data.pestHasFailedEat;

        if (data.attackVillageTarget != null) {
            attackVillageTarget = data.attackVillageTarget.persistentID;
        }
        if (data.attackAreaTarget != null) {
            attackAreaTarget = data.attackAreaTarget.persistentID;
        }
        if (data.attackDemonicStructureTarget != null) {
            attackDemonicStructureTarget = data.attackDemonicStructureTarget.persistentID;
        }
        if (data.dousingFireForSettlement != null) {
            dousingFireForSettlement = data.dousingFireForSettlement.persistentID;
        }
        if (data.cleansingTilesForSettlement != null) {
            cleansingTilesForSettlement = data.cleansingTilesForSettlement.persistentID;
        }
        // if (data.dryingTilesForSettlement != null) {
        //     dryingTilesForSettlement = data.dryingTilesForSettlement.persistentID;
        // }
        //if (data.targetMiningTile != null) {
        //    targetMiningTile = new TileLocationSave(data.targetMiningTile);
        //}
        if (data.currentAbductTarget != null) {
            currentAbductTarget = data.currentAbductTarget.persistentID;
        }
        if (data.invaderToFollow != null) {
            invaderToFollow = data.invaderToFollow.persistentID;
        }
        if (data.nest != null) {
            nest = new TileLocationSave(data.nest);
        }
        if (data.abominationTarget != null) {
            abominationTarget = data.abominationTarget.persistentID;
        }
        if (data.deMoodVillageTarget != null) {
            deMoodVillageTarget = new List<string>();
            for (int i = 0; i < data.deMoodVillageTarget.Count; i++) {
                deMoodVillageTarget.Add(data.deMoodVillageTarget[i].persistentID);
            }
        }
        if (data.invadeVillageTarget != null) {
            invadeVillageTarget = new List<string>();
            for (int i = 0; i < data.invadeVillageTarget.Count; i++) {
                invadeVillageTarget.Add(data.invadeVillageTarget[i].persistentID);
            }
        }
        if (data.arsonVillageTarget != null) {
            arsonVillageTarget = new List<string>();
            for (int i = 0; i < data.arsonVillageTarget.Count; i++) {
                arsonVillageTarget.Add(data.arsonVillageTarget[i].persistentID);
            }
        }
        if (data.pestSettlementTarget != null) {
            pestSettlementTarget = data.pestSettlementTarget.persistentID;
            //pestSettlementTarget = new List<string>();
            //for (int i = 0; i < data.pestSettlementTarget.Count; i++) {
            //    pestSettlementTarget.Add(data.pestSettlementTarget[i].persistentID);
            //}
        }
        if (data.targetSocializeStructure != null) {
            targetSocializeStructure = data.targetSocializeStructure.persistentID;
        }
        if (data.socializingEndTime.hasValue) {
            socializingEndTime = data.socializingEndTime;
        }
        if (data.targetVisitVillage != null) {
            targetVisitVillage = data.targetVisitVillage.persistentID;
        }
        if (data.targetVisitVillageStructure != null) {
            targetVisitVillageStructure = data.targetVisitVillageStructure.persistentID;
        }
        if (data.visitVillageEndTime.hasValue) {
            visitVillageEndTime = data.visitVillageEndTime;
        }
        visitVillageIntent = data.visitVillageIntent;
    }

    public override BehaviourComponent Load() {
        BehaviourComponent component = new BehaviourComponent(this);
        return component;
    }
    #endregion
}