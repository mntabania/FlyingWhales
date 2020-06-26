using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Pathfinding;
using UtilityScripts;

public class BehaviourComponent {

	public Character owner { get; private set; }
    public List<CharacterBehaviourComponent> currentBehaviourComponents { get; private set; }
    public List<Character> invadeCombatantTargetList { get; private set; }
    public List<Character> invadeNonCombatantTargetList { get; private set; }
    public NPCSettlement assignedTargetSettlement { get; private set; }
    public NPCSettlement attackVillageTarget { get; private set; }
    public HexTile assignedTargetHex { get; private set; }
    public DemonicStructure attackDemonicStructureTarget { get; private set; }
    public bool isHarassing { get; private set; }
    public bool isDefending { get; private set; }
    public bool isInvading { get; private set; }
    public bool isAttackingDemonicStructure { get; private set; }
    public string defaultBehaviourSetName { get; private set; }
    
    //douse fire
    public NPCSettlement dousingFireForSettlement { get; private set; }
    //cleanse tiles
    public NPCSettlement cleansingTilesForSettlement { get; private set; }
    //cleanse tiles
    public NPCSettlement dryingTilesForSettlement { get; private set; }
    //mining
    public LocationGridTile targetMiningTile { get; private set; }
    public ABPath currentMiningPath { get; private set; }
    
    //Abduct
    //public ABPath currentAbductDigPath { get; private set; }
    public Character currentAbductTarget { get; private set; }
    
    private COMBAT_MODE combatModeBeforeHarassRaidInvade;
    private COMBAT_MODE combatModeBeforeAttackingDemonicStructure;

    public BehaviourComponent (Character owner) {
        this.owner = owner;
        defaultBehaviourSetName = string.Empty;
        currentBehaviourComponents = new List<CharacterBehaviourComponent>();
        invadeCombatantTargetList = new List<Character>();
        invadeNonCombatantTargetList = new List<Character>();
        PopulateInitialBehaviourComponents();
    }

    #region General
    public void PopulateInitialBehaviourComponents() {
        ChangeDefaultBehaviourSet(CharacterManager.Default_Resident_Behaviour);
    }
    //public void OnChangeClass(CharacterClass newClass, CharacterClass oldClass) {
    //    if(oldClass == newClass) {
    //        return;
    //    }
    //    //if(oldClass != null && newClass != null) {
    //    //    string oldClassBehaviourComponentKey = CharacterManager.Instance.GetClassBehaviourComponentKey(oldClass.className);
    //    //    string newClassBehaviourComponentKey = CharacterManager.Instance.GetClassBehaviourComponentKey(newClass.className);
    //    //    if (oldClassBehaviourComponentKey == newClassBehaviourComponentKey) {
    //    //        return;
    //    //    }
    //    //}
    //    //if (oldClass != null) {
    //    //    System.Type[] classBehaviourComponents = CharacterManager.Instance.GetDefaultBehaviourSet(oldClass.className);
    //    //    for (int i = 0; i < classBehaviourComponents.Length; i++) {
    //    //        RemoveBehaviourComponent(CharacterManager.Instance.GetCharacterBehaviourComponent(classBehaviourComponents[i]));
    //    //    }
    //    //}
    //    //if(newClass != null) {
    //    //    System.Type[] classBehaviourComponents = CharacterManager.Instance.GetDefaultBehaviourSet(newClass.className);
    //    //    for (int i = 0; i < classBehaviourComponents.Length; i++) {
    //    //        AddBehaviourComponent(CharacterManager.Instance.GetCharacterBehaviourComponent(classBehaviourComponents[i]));
    //    //    }
    //    //}
    //}
    public bool AddBehaviourComponent(CharacterBehaviourComponent component) {
        if(component == null) {
            throw new System.Exception(
                $"{GameManager.Instance.TodayLogString()}{owner.name} is trying to add a new behaviour component but it is null!");
        }
        return AddBehaviourComponentInOrder(component);
    }
    public bool AddBehaviourComponent(System.Type componentType) {
        return AddBehaviourComponent(CharacterManager.Instance.GetCharacterBehaviourComponent(componentType));
    }
    private bool RemoveBehaviourComponent(CharacterBehaviourComponent component) {
        bool wasRemoved = currentBehaviourComponents.Remove(component);
        if (wasRemoved) {
            Debug.Log($"{owner.name} removed character behaviour {component}");
            Messenger.Broadcast(Signals.CHARACTER_REMOVED_BEHAVIOUR, owner, component);
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
        if(isHarassing != state) {
            isHarassing = state;
            NPCSettlement previousTarget = assignedTargetSettlement;
            assignedTargetHex = target;
            if (assignedTargetHex != null) {
                assignedTargetSettlement = assignedTargetHex.settlementOnTile as NPCSettlement;
            } else {
                assignedTargetSettlement = null;
            }
            owner.CancelAllJobs();
            if (isHarassing) {
                assignedTargetSettlement.IncreaseIsBeingHarassedCount();
                combatModeBeforeHarassRaidInvade = owner.combatComponent.combatMode;
                owner.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
                AddBehaviourComponent(typeof(HarassBehaviour));
                //TODO: Optimize this to not always create new instance if playeraction, or if it can't be helped, do object pool
                //owner.AddPlayerAction(new PlayerAction(PlayerDB.End_Harass_Action, () => true, null, () => SetIsHarassing(false, null)));
                //owner.AddPlayerAction(SPELL_TYPE.END_HARASS);
            } else {
                previousTarget.DecreaseIsBeingHarassedCount();
                owner.combatComponent.SetCombatMode(combatModeBeforeHarassRaidInvade);
                RemoveBehaviourComponent(typeof(HarassBehaviour));
                //owner.RemovePlayerAction(PlayerDB.End_Harass_Action);
                //owner.RemovePlayerAction(SPELL_TYPE.END_HARASS);
            }
        }
    }
    public void SetIsDefending(bool state, HexTile target) {
        if (isDefending != state) {
            isDefending = state;
            HexTile previousTarget = assignedTargetHex;
            assignedTargetHex = target;
            //if (assignedTargetHex != null) {
            //    assignedTargetSettlement = assignedTargetHex.settlementOnTile as NPCSettlement;
            //} else {
            //    assignedTargetSettlement = null;
            //}
            owner.CancelAllJobs();
            if (isDefending) {
                assignedTargetHex.IncreaseIsBeingDefendedCount();
                combatModeBeforeHarassRaidInvade = owner.combatComponent.combatMode;
                owner.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
                AddBehaviourComponent(typeof(DefendBehaviour));
                //TODO: Optimize this to not always create new instance if playeraction, or if it can't be helped, do object pool
                //owner.AddPlayerAction(new PlayerAction(PlayerDB.End_Raid_Action, () => true, null, () => SetIsRaiding(false, null)));
                //owner.AddPlayerAction(SPELL_TYPE.END_RAID);
            } else {
                previousTarget.DecreaseIsBeingDefendedCount();
                owner.combatComponent.SetCombatMode(combatModeBeforeHarassRaidInvade);
                RemoveBehaviourComponent(typeof(DefendBehaviour));
                //owner.RemovePlayerAction(PlayerDB.End_Raid_Action);
                //owner.RemovePlayerAction(SPELL_TYPE.END_RAID);
            }
        }
    }
    public void SetIsInvading(bool state, HexTile target) {
        if (isInvading != state) {
            isInvading = state;
            NPCSettlement previousTarget = assignedTargetSettlement;
            assignedTargetHex = target;
            if (assignedTargetHex != null) {
                assignedTargetSettlement = assignedTargetHex.settlementOnTile as NPCSettlement;
            } else {
                assignedTargetSettlement = null;
            }
            owner.CancelAllJobs();
            if (isInvading) {
                assignedTargetSettlement.IncreaseIsBeingInvadedCount();
                combatModeBeforeHarassRaidInvade = owner.combatComponent.combatMode;
                owner.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
                AddBehaviourComponent(typeof(InvadeBehaviour));
                //TODO: Optimize this to not always create new instance if playeraction, or if it can't be helped, do object pool
                //owner.AddPlayerAction(new PlayerAction(PlayerDB.End_Invade_Action, () => true, null, () => SetIsInvading(false, null)));
                //owner.AddPlayerAction(SPELL_TYPE.END_INVADE);
                //Messenger.AddListener<NPCSettlement>(Signals.NO_ABLE_CHARACTER_INSIDE_SETTLEMENT, OnNoLongerAbleResidentsInsideSettlement);
            } else {
                previousTarget.DecreaseIsBeingInvadedCount();
                owner.combatComponent.SetCombatMode(combatModeBeforeHarassRaidInvade);
                RemoveBehaviourComponent(typeof(InvadeBehaviour));
                //owner.RemovePlayerAction(PlayerDB.End_Invade_Action);
                //owner.RemovePlayerAction(SPELL_TYPE.END_INVADE);
                //Messenger.RemoveListener<NPCSettlement>(Signals.NO_ABLE_CHARACTER_INSIDE_SETTLEMENT, OnNoLongerAbleResidentsInsideSettlement);
            }
        }
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
            if (goapPlanJob.jobType == JOB_TYPE.IDLE_RETURN_HOME) {
                if (character.homeStructure != null) {
                    return character.movementComponent.HasPathToEvenIfDiffRegion(character.homeStructure.GetRandomUnoccupiedTile());
                } else if (character.territorries != null && character.territorries.Count > 0) {
                    HexTile randomTerritory = CollectionUtilities.GetRandomElement(character.territorries);
                    return character.movementComponent.HasPathToEvenIfDiffRegion(CollectionUtilities.GetRandomElement(randomTerritory.locationGridTiles));
                }
            }
            if (character == goapPlanJob.targetPOI || goapPlanJob.targetPOI == null) {
                return true;
            }
            return character.movementComponent.HasPathToEvenIfDiffRegion(goapPlanJob.targetPOI.gridTileLocation);
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
            owner.CancelAllJobs();
            if (isAttackingDemonicStructure) {
                combatModeBeforeAttackingDemonicStructure = owner.combatComponent.combatMode;
                owner.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
                AddBehaviourComponent(typeof(AttackDemonicStructureBehaviour));
                owner.traitContainer.AddTrait(owner, "Fervor");
                StartCheckingIfShouldStopAttackingDemonicStructure();
            } else {
                owner.combatComponent.SetCombatMode(combatModeBeforeAttackingDemonicStructure);
                RemoveBehaviourComponent(typeof(AttackDemonicStructureBehaviour));
                owner.traitContainer.RemoveTrait(owner, "Fervor");
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
        Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
    }
    private void StopCheckingIfShouldStopAttackingDemonicStructure() {
        // Messenger.RemoveListener<Character, ActualGoapNode>(Signals.CHARACTER_DOING_ACTION, OnActionStartedWhileAttackingDemonicStructure);
        // Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
    }
    private void OnActionStartedWhileAttackingDemonicStructure(Character actor, ActualGoapNode action) {
        if (actor == owner) {
            if (action.action.goapType != INTERACTION_TYPE.ATTACK_DEMONIC_STRUCTURE) {
                SetIsAttackingDemonicStructure(false, null);
            }
        }
    }
    private void OnCharacterCanNoLongerPerform(Character character) {
        if (character == owner) {
            if(owner is Summon summon && (summon.summonType == SUMMON_TYPE.Magical_Angel || summon.summonType == SUMMON_TYPE.Warrior_Angel)) {
                //Angels should not remove attack demonic structure
            } else {
                SetIsAttackingDemonicStructure(false, null);
            }
        }
    }
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
    public void SetCurrentMiningPath(ABPath path) {
        currentMiningPath = path;
    }
    #endregion

    #region Attack Village
    public void SetAttackVillageTarget(NPCSettlement npcSettlement) {
        attackVillageTarget = npcSettlement;
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

}
