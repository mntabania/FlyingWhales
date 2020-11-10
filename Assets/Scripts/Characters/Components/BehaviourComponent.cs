using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Pathfinding;
using UtilityScripts;

public class BehaviourComponent : CharacterComponent {
    public List<CharacterBehaviourComponent> currentBehaviourComponents { get; private set; }
    //public NPCSettlement assignedTargetSettlement { get; private set; }
    public NPCSettlement attackVillageTarget { get; private set; }
    public HexTile attackHexTarget { get; private set; }
    //public HexTile assignedTargetHex { get; private set; }
    public DemonicStructure attackDemonicStructureTarget { get; private set; }
    //public bool isHarassing { get; private set; }
    //public bool isDefending { get; private set; }
    //public bool isInvading { get; private set; }
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
    public LocationGridTile targetMiningTile { get; private set; }
    
    //Abduct
    //public ABPath currentAbductDigPath { get; private set; }
    public Character currentAbductTarget { get; private set; }
    
    //De-Mood
    public bool canDeMood => currentDeMoodCooldown >= _deMoodCooldownPeriod;
    public int currentDeMoodCooldown { get; private set; }
    private readonly int _deMoodCooldownPeriod;
    public List<HexTile> deMoodVillageTarget { get; private set; }
    
    //invade
    public List<HexTile> invadeVillageTarget { get; private set; }
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
    public List<HexTile> arsonVillageTarget { get; private set; }
    
    //Abomination
    public HexTile abominationTarget { get; private set; }
    
    //snatcher
    public bool isCurrentlySnatching;
    
    //private COMBAT_MODE combatModeBeforeHarassRaidInvade;
    public COMBAT_MODE combatModeBeforeAttackingDemonicStructure { get; private set; }

    public BehaviourComponent () {
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
            // Debug.Log($"{owner.name} removed character behaviour {component}");
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
    public void SetIsHarassing(bool state, HexTile target) {
        //if(isHarassing != state) {
        //    isHarassing = state;
        //    NPCSettlement previousTarget = assignedTargetSettlement;
        //    assignedTargetHex = target;
        //    if (assignedTargetHex != null) {
        //        assignedTargetSettlement = assignedTargetHex.settlementOnTile as NPCSettlement;
        //    } else {
        //        assignedTargetSettlement = null;
        //    }
        //    owner.jobQueue.CancelAllJobs();
        //    if (isHarassing) {
        //        assignedTargetSettlement.IncreaseIsBeingHarassedCount();
        //        combatModeBeforeHarassRaidInvade = owner.combatComponent.combatMode;
        //        owner.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
        //        AddBehaviourComponent(typeof(HarassBehaviour));
        //        //TODO: Optimize this to not always create new instance if playeraction, or if it can't be helped, do object pool
        //        //owner.AddPlayerAction(new PlayerAction(PlayerDB.End_Harass_Action, () => true, null, () => SetIsHarassing(false, null)));
        //        //owner.AddPlayerAction(SPELL_TYPE.END_HARASS);
        //    } else {
        //        previousTarget.DecreaseIsBeingHarassedCount();
        //        owner.combatComponent.SetCombatMode(combatModeBeforeHarassRaidInvade);
        //        RemoveBehaviourComponent(typeof(HarassBehaviour));
        //        //owner.RemovePlayerAction(PlayerDB.End_Harass_Action);
        //        //owner.RemovePlayerAction(SPELL_TYPE.END_HARASS);
        //    }
        //}
    }
    public void SetIsDefending(bool state, HexTile target) {
        //if (isDefending != state) {
        //    isDefending = state;
        //    HexTile previousTarget = assignedTargetHex;
        //    assignedTargetHex = target;
        //    //if (assignedTargetHex != null) {
        //    //    assignedTargetSettlement = assignedTargetHex.settlementOnTile as NPCSettlement;
        //    //} else {
        //    //    assignedTargetSettlement = null;
        //    //}
        //    owner.jobQueue.CancelAllJobs();
        //    if (isDefending) {
        //        assignedTargetHex.IncreaseIsBeingDefendedCount();
        //        combatModeBeforeHarassRaidInvade = owner.combatComponent.combatMode;
        //        owner.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
        //        AddBehaviourComponent(typeof(DefendBehaviour));
        //        //TODO: Optimize this to not always create new instance if playeraction, or if it can't be helped, do object pool
        //        //owner.AddPlayerAction(new PlayerAction(PlayerDB.End_Raid_Action, () => true, null, () => SetIsRaiding(false, null)));
        //        //owner.AddPlayerAction(SPELL_TYPE.END_RAID);
        //    } else {
        //        previousTarget.DecreaseIsBeingDefendedCount();
        //        owner.combatComponent.SetCombatMode(combatModeBeforeHarassRaidInvade);
        //        RemoveBehaviourComponent(typeof(DefendBehaviour));
        //        //owner.RemovePlayerAction(PlayerDB.End_Raid_Action);
        //        //owner.RemovePlayerAction(SPELL_TYPE.END_RAID);
        //    }
        //}
    }
    public void SetIsInvading(bool state, HexTile target) {
        //if (isInvading != state) {
        //    isInvading = state;
        //    NPCSettlement previousTarget = assignedTargetSettlement;
        //    assignedTargetHex = target;
        //    if (assignedTargetHex != null) {
        //        assignedTargetSettlement = assignedTargetHex.settlementOnTile as NPCSettlement;
        //    } else {
        //        assignedTargetSettlement = null;
        //    }
        //    owner.jobQueue.CancelAllJobs();
        //    if (isInvading) {
        //        assignedTargetSettlement.IncreaseIsBeingInvadedCount();
        //        combatModeBeforeHarassRaidInvade = owner.combatComponent.combatMode;
        //        owner.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
        //        AddBehaviourComponent(typeof(InvadeBehaviour));
        //        //TODO: Optimize this to not always create new instance if playeraction, or if it can't be helped, do object pool
        //        //owner.AddPlayerAction(new PlayerAction(PlayerDB.End_Invade_Action, () => true, null, () => SetIsInvading(false, null)));
        //        //owner.AddPlayerAction(SPELL_TYPE.END_INVADE);
        //        //Messenger.AddListener<NPCSettlement>(Signals.NO_ABLE_CHARACTER_INSIDE_SETTLEMENT, OnNoLongerAbleResidentsInsideSettlement);
        //    } else {
        //        previousTarget.DecreaseIsBeingInvadedCount();
        //        owner.combatComponent.SetCombatMode(combatModeBeforeHarassRaidInvade);
        //        RemoveBehaviourComponent(typeof(InvadeBehaviour));
        //        //owner.RemovePlayerAction(PlayerDB.End_Invade_Action);
        //        //owner.RemovePlayerAction(SPELL_TYPE.END_INVADE);
        //        //Messenger.RemoveListener<NPCSettlement>(Signals.NO_ABLE_CHARACTER_INSIDE_SETTLEMENT, OnNoLongerAbleResidentsInsideSettlement);
        //    }
        //}
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
        if (owner.isNormalCharacter || owner.characterClass.className == "Zombie") {
            if(owner.homeSettlement != null) {
                owner.SetIsWanderer(false);
            } else {
                owner.SetIsWanderer(true);
            }
        } else {
            
            if (owner.minion != null) {
                ChangeDefaultBehaviourSet(CharacterManager.Default_Minion_Behaviour);
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
    public List<HexTile> GetVillageTargetsByPriority() {
        //get settlements in region that have normal characters living there.
        List<BaseSettlement> settlementsInRegion = owner.currentRegion?.GetSettlementsInRegion(
            settlement => settlement.residents.Count(c => c.isNormalCharacter && c.IsAble()) > 0);
        if (settlementsInRegion != null) {
            List<BaseSettlement> villageChoices = settlementsInRegion.Where(x =>
                    x.locationType == LOCATION_TYPE.VILLAGE)
                .ToList();
            if (villageChoices.Count > 0) {
                //a random village occupied by Villagers within current region
                BaseSettlement chosenVillage = CollectionUtilities.GetRandomElement(villageChoices);
                return new List<HexTile>(chosenVillage.tiles);
            } else {
                //a random special structure occupied by Villagers within current region
                List<BaseSettlement> specialStructureChoices = settlementsInRegion.Where(x =>
                        x.locationType == LOCATION_TYPE.DUNGEON).ToList();
                if (specialStructureChoices.Count > 0) {
                    BaseSettlement chosenSpecialStructure = CollectionUtilities.GetRandomElement(specialStructureChoices);
                    return new List<HexTile>(chosenSpecialStructure.tiles);
                }
            }
        } 
        //no settlements in region.
        //a random area occupied by Villagers within current region
        List<HexTile> occupiedAreas = owner.currentRegion?.GetAreasOccupiedByVillagers();
        if (occupiedAreas != null) {
            HexTile randomArea = CollectionUtilities.GetRandomElement(occupiedAreas);
            return new List<HexTile>() { randomArea };
        }
        return null;
    }
    #endregion

    #region Processes
    public string RunBehaviour() {
        string log = $"{GameManager.Instance.TodayLogString()}{owner.name} Idle Plan Decision Making:";
        for (int i = 0; i < currentBehaviourComponents.Count; i++) {
            CharacterBehaviourComponent component = currentBehaviourComponents[i];
            if (component.IsDisabledFor(owner)) {
                log += $"\nBehaviour Component: {component.ToString()} is disabled for {owner.name} skipping it...";
                continue; //skip component
            }
            if (!component.CanDoBehaviour(owner)) {
                log += $"\nBehaviour Component: {component.ToString()} cannot be done by {owner.name} skipping it...";
                continue; //skip component
            }
            if (component.TryDoBehaviour(owner, ref log, out JobQueueItem producedJob)) {
                bool isProducedJobValid = IsProducedJobValid(producedJob, owner);
                if (producedJob == null || isProducedJobValid) {
                    if (producedJob != null) {
                        owner.jobQueue.AddJobInQueue(producedJob);
                    }
                    component.PostProcessAfterSuccessfulDoBehaviour(owner);
                    if (!component.WillContinueProcess()) { break; }    
                }
                if (isProducedJobValid == false && producedJob != null) { //if produced valid is not valid and produced job is not null
                    //add character to blacklist if job is owned by a settlement
                    if (producedJob.originalOwner != null && producedJob.originalOwner.ownerType != JOB_OWNER.CHARACTER) {
                        producedJob.AddBlacklistedCharacter(owner);
                    }
                }

            }
        }
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
                } else if (character.territories != null && character.territories.Count > 0) {
                    HexTile randomTerritory = CollectionUtilities.GetRandomElement(character.territories);
                    return character.movementComponent.HasPathToEvenIfDiffRegion(CollectionUtilities.GetRandomElement(randomTerritory.locationGridTiles));
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
    
    #region Dry Tiles
    public void SetDryingTilesForSettlement(NPCSettlement settlement) {
        if (settlement == null) {
            //remove tile dryer from previous settlements list, if any.
            dryingTilesForSettlement?.settlementJobTriggerComponent.RemoveTileDryer(owner);
        } else {
            settlement.settlementJobTriggerComponent.AddTileDryer(owner);
        }
        dryingTilesForSettlement = settlement;
        
    }
    #endregion

    #region Mining
    public void SetTargetMiningTile(LocationGridTile tile) {
        targetMiningTile = tile;
    }
    #endregion

    #region Attack Village
    public void SetAttackVillageTarget(NPCSettlement npcSettlement) {
        attackVillageTarget = npcSettlement;
    }
    public void SetAttackHexTarget(HexTile hex) {
        attackHexTarget = hex;
    }
    public void ClearAttackVillageData() {
        SetAttackHexTarget(null);
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
    public void OnBecomeDeMooder() {
        Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfDeMoodJobFinished);
    }
    public void OnNoLongerDeMooder() {
        Messenger.RemoveListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfDeMoodJobFinished);
    }
    private void CheckIfDeMoodJobFinished(Character character, GoapPlanJob job) {
        if (character == owner && job.jobType == JOB_TYPE.DECREASE_MOOD) {
            //character finished decrease mood job, start cooldown.
            StartDeMoodCooldown();
            SetDeMoodVillageTarget(null);
        }
    }
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
    public void SetDeMoodVillageTarget(List<HexTile> targets) {
        deMoodVillageTarget = targets;
    }
    #endregion

    #region Invade
    public void SetInvadeVillageTarget(List<HexTile> targets) {
        invadeVillageTarget = targets;
    }
    public void AddFollower() {
        followerCount++;
    }
    public void RemoveFollower() {
        followerCount--;
    }
    #endregion

    #region Disabler
    public void OnBecomeDisabler() {
        Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfDisablerJobFinished);
    }
    public void OnNoLongerDisabler() {
        Messenger.RemoveListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfDisablerJobFinished);
    }
    private void CheckIfDisablerJobFinished(Character character, GoapPlanJob job) {
        if (character == owner && job.jobType == JOB_TYPE.DISABLE) {
            //character finished disable job, start cooldown.
            StartDisablerCooldown();
        }
    }
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
        Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfMonsterAte);
        Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnAbductorAddedJobToQueue);
        Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnAbductorRemovedJobFromQueue);
        Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnAbductorFinishedJobSuccessfully);
    }
    public void OnNoLongerAbductor() {
        Messenger.RemoveListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfMonsterAte);
        Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnAbductorAddedJobToQueue);
        Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnAbductorRemovedJobFromQueue);
        Messenger.RemoveListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnAbductorFinishedJobSuccessfully);
    }
    private void OnAbductorFinishedJobSuccessfully(Character character, GoapPlanJob job) {
        if (character == owner && job.jobType == JOB_TYPE.MONSTER_ABDUCT && job.targetPOI is Character targetCharacter) {
            targetCharacter.defaultCharacterTrait.SetHasBeenAbductedByPlayerMonster(true);
        }
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
    private void CheckIfMonsterAte(Character character, GoapPlanJob job) {
        if (character == owner && job.jobType == JOB_TYPE.MONSTER_EAT) {
            TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick();
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
        if (nest.objHere != null) {
            blocker = nest.objHere;
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
    public void SetArsonistVillageTarget(List<HexTile> target) {
        arsonVillageTarget = target;
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
        Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfArsonistDidBurn);
        Messenger.AddListener<Character>(CharacterSignals.START_FLEE, OnArsonistStartedFleeing);
    }
    public void OnNoLongerArsonist() {
        Messenger.RemoveListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, CheckIfArsonistDidBurn);
        Messenger.RemoveListener<Character>(CharacterSignals.START_FLEE, OnArsonistStartedFleeing);
    }
    private void CheckIfArsonistDidBurn(Character character, GoapPlanJob job) {
        if (character == owner && job.jobType == JOB_TYPE.ARSON) {
            //clear target village data after 3 hours after first successful arson job.
            GameDate expiry = GameManager.Instance.Today();
            expiry.AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
            SchedulingManager.Instance.AddEntry(expiry, () => SetArsonistVillageTarget(null), owner);
            SchedulingManager.Instance.AddEntry(expiry, StartArsonistCooldown, owner);
        }
    }
    private void OnArsonistStartedFleeing(Character character) {
        if (character == owner) {
            //once arson starts fleeing, clear target village and start cooldown.
            SetArsonistVillageTarget(null);
            StartArsonistCooldown();
        }
    }
    #endregion

    #region Abomination
    public void SetAbominationTarget(HexTile tile) {
        abominationTarget = tile;
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
        Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnSnatcherAddedJobToQueue);
        Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnSnatchJobRemoved);   
    }
    public void OnNoLongerSnatcher() {
        Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_ADDED_TO_QUEUE, OnSnatcherAddedJobToQueue);
        Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnSnatchJobRemoved);   
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
        Messenger.AddListener<Character, HexTile>(CharacterSignals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
        Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnDazedCharacterCanNoLongerPerform);
        Messenger.AddListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnDazedCharacterStartedState);
    }
    private void OnCharacterEnteredHexTile(Character character, HexTile tile) {
        if (character == owner) {
            if (character.homeSettlement != null && character.homeSettlement.tiles.Contains(tile)) {
                character.traitContainer.RemoveTrait(character, "Dazed");
            } else if (character.territories != null && character.territories.Contains(tile)) {
                character.traitContainer.RemoveTrait(character, "Dazed");    
            }
        }
    }
    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
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
        Messenger.RemoveListener<Character, HexTile>(CharacterSignals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
        Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnDazedCharacterCanNoLongerPerform);
        Messenger.RemoveListener<Character, CharacterState>(CharacterSignals.CHARACTER_STARTED_STATE, OnDazedCharacterStartedState);
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataBehaviourComponent data) {
        if (!string.IsNullOrEmpty(data.attackVillageTarget)) {
            attackVillageTarget = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.attackVillageTarget) as NPCSettlement;
        }
        if (!string.IsNullOrEmpty(data.attackHexTarget)) {
            attackHexTarget = DatabaseManager.Instance.hexTileDatabase.GetHextileByPersistentID(data.attackHexTarget);
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
        if (!string.IsNullOrEmpty(data.dryingTilesForSettlement)) {
            dryingTilesForSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.dryingTilesForSettlement) as NPCSettlement;
        }
        if (data.targetMiningTile.hasValue) {
            targetMiningTile = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(data.targetMiningTile);
        }
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
            abominationTarget = DatabaseManager.Instance.hexTileDatabase.GetHextileByPersistentID(data.abominationTarget);
        }
        if (data.deMoodVillageTarget != null) {
            deMoodVillageTarget = new List<HexTile>();
            for (int i = 0; i < data.deMoodVillageTarget.Count; i++) {
                HexTile hex = DatabaseManager.Instance.hexTileDatabase.GetHextileByPersistentID(data.deMoodVillageTarget[i]);
                deMoodVillageTarget.Add(hex);
            }
        }
        if (data.invadeVillageTarget != null) {
            invadeVillageTarget = new List<HexTile>();
            for (int i = 0; i < data.invadeVillageTarget.Count; i++) {
                HexTile hex = DatabaseManager.Instance.hexTileDatabase.GetHextileByPersistentID(data.invadeVillageTarget[i]);
                invadeVillageTarget.Add(hex);
            }
        }
        if (data.arsonVillageTarget != null) {
            arsonVillageTarget = new List<HexTile>();
            for (int i = 0; i < data.arsonVillageTarget.Count; i++) {
                HexTile hex = DatabaseManager.Instance.hexTileDatabase.GetHextileByPersistentID(data.arsonVillageTarget[i]);
                arsonVillageTarget.Add(hex);
            }
        }
        if (data.currentBehaviourComponents != null) {
            for (int i = 0; i < data.currentBehaviourComponents.Count; i++) {
                string behaviourStr = data.currentBehaviourComponents[i];
                CharacterBehaviourComponent component = CharacterManager.Instance.GetCharacterBehaviourComponent(System.Type.GetType(behaviourStr));
                component.OnLoadBehaviourToCharacter(owner);
            }    
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataBehaviourComponent : SaveData<BehaviourComponent> {
    public List<string> currentBehaviourComponents;
    public string attackVillageTarget;
    public string attackHexTarget;
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
    public string dryingTilesForSettlement;
    //mining
    public TileLocationSave targetMiningTile;

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


    public COMBAT_MODE combatModeBeforeAttackingDemonicStructure;

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

        if (data.attackVillageTarget != null) {
            attackVillageTarget = data.attackVillageTarget.persistentID;
        }
        if (data.attackHexTarget != null) {
            attackHexTarget = data.attackHexTarget.persistentID;
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
        if (data.dryingTilesForSettlement != null) {
            dryingTilesForSettlement = data.dryingTilesForSettlement.persistentID;
        }
        if (data.targetMiningTile != null) {
            targetMiningTile = new TileLocationSave(data.targetMiningTile);
        }
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
    }

    public override BehaviourComponent Load() {
        BehaviourComponent component = new BehaviourComponent(this);
        return component;
    }
    #endregion
}