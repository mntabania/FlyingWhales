using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class BehaviourComponent {

	public Character owner { get; private set; }
    public List<CharacterBehaviourComponent> currentBehaviourComponents { get; private set; }
    public NPCSettlement assignedTargetSettlement { get; private set; }
    public HexTile assignedTargetHex { get; private set; }
    public DemonicStructure attackDemonicStructureTarget { get; private set; }
    public bool isHarassing { get; private set; }
    public bool isDefending { get; private set; }
    public bool isInvading { get; private set; }
    public bool isAttackingDemonicStructure { get; private set; }

    private COMBAT_MODE combatModeBeforeHarassRaidInvade;
    private COMBAT_MODE combatModeBeforeAttackingDemonicStructure;

    public BehaviourComponent (Character owner) {
        this.owner = owner;
        currentBehaviourComponents = new List<CharacterBehaviourComponent>();
    }

    #region General
    public void PopulateInitialBehaviourComponents() {
        System.Type[] classBehaviourComponents = CharacterManager.Instance.GetClassBehaviourComponents(owner.characterClass.className);
        for (int i = 0; i < classBehaviourComponents.Length; i++) {
            CharacterBehaviourComponent behaviourComponent = CharacterManager.Instance.GetCharacterBehaviourComponent(classBehaviourComponents[i]);
            AddBehaviourComponent(behaviourComponent);
        }
    }
    public void OnChangeClass(CharacterClass newClass, CharacterClass oldClass) {
        if(oldClass == newClass) {
            return;
        }
        if(oldClass != null && newClass != null) {
            string oldClassBehaviourComponentKey = CharacterManager.Instance.GetClassBehaviourComponentKey(oldClass.className);
            string newClassBehaviourComponentKey = CharacterManager.Instance.GetClassBehaviourComponentKey(newClass.className);
            if (oldClassBehaviourComponentKey == newClassBehaviourComponentKey) {
                return;
            }
        }
        if (oldClass != null) {
            System.Type[] classBehaviourComponents = CharacterManager.Instance.GetClassBehaviourComponents(oldClass.className);
            for (int i = 0; i < classBehaviourComponents.Length; i++) {
                RemoveBehaviourComponent(CharacterManager.Instance.GetCharacterBehaviourComponent(classBehaviourComponents[i]));
            }
        }
        if(newClass != null) {
            System.Type[] classBehaviourComponents = CharacterManager.Instance.GetClassBehaviourComponents(newClass.className);
            for (int i = 0; i < classBehaviourComponents.Length; i++) {
                AddBehaviourComponent(CharacterManager.Instance.GetCharacterBehaviourComponent(classBehaviourComponents[i]));
            }
        }
    }
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
    public bool RemoveBehaviourComponent(CharacterBehaviourComponent component) {
        return currentBehaviourComponents.Remove(component);
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
                if (component.priority <= currentBehaviourComponents[i].priority) {
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
                Messenger.AddListener<NPCSettlement>(Signals.NO_ABLE_CHARACTER_INSIDE_SETTLEMENT, OnNoLongerAbleResidentsInsideSettlement);
            } else {
                previousTarget.DecreaseIsBeingInvadedCount();
                owner.combatComponent.SetCombatMode(combatModeBeforeHarassRaidInvade);
                RemoveBehaviourComponent(typeof(InvadeBehaviour));
                //owner.RemovePlayerAction(PlayerDB.End_Invade_Action);
                //owner.RemovePlayerAction(SPELL_TYPE.END_INVADE);
                Messenger.RemoveListener<NPCSettlement>(Signals.NO_ABLE_CHARACTER_INSIDE_SETTLEMENT, OnNoLongerAbleResidentsInsideSettlement);
            }
        }
    }
    public void SetIsAttackingDemonicStructure(bool state, DemonicStructure target) {
        if (isAttackingDemonicStructure != state) {
            isAttackingDemonicStructure = state;
            attackDemonicStructureTarget = target;
            owner.CancelAllJobs();
            if (isAttackingDemonicStructure) {
                combatModeBeforeAttackingDemonicStructure = owner.combatComponent.combatMode;
                owner.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
                AddBehaviourComponent(typeof(AttackDemonicStructureBehaviour));
                owner.traitContainer.AddTrait(owner, "Fervor");
            } else {
                owner.combatComponent.SetCombatMode(combatModeBeforeAttackingDemonicStructure);
                RemoveBehaviourComponent(typeof(AttackDemonicStructureBehaviour));
                owner.traitContainer.RemoveTrait(owner, "Fervor");
            }
        }
    }
    private void OnNoLongerAbleResidentsInsideSettlement(NPCSettlement npcSettlement) {
        if(assignedTargetSettlement == npcSettlement) {
            SetIsInvading(false, null);
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
            if (component.TryDoBehaviour(owner, ref log)) {
                component.PostProcessAfterSucessfulDoBehaviour(owner);
                if (!component.WillContinueProcess()) { break; }
            }
        }
        return log;
    }
    #endregion
}
