using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class CharacterCombatBehaviour {
    public CHARACTER_COMBAT_BEHAVIOUR behaviourType { get; private set; }
    public string name { get; private set; }
    public virtual string description => string.Empty;

    public CharacterCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR p_type) {
        behaviourType = p_type;
        name = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(behaviourType.ToString());
    }

    #region Virtuals
    public virtual void SetAsCombatBehaviourOf(Character p_character) { }
    public virtual void UnsetAsCombatBehaviourOf(Character p_character) { }
    public virtual void OnCharacterJoinedPartyQuest(Character p_character, PARTY_QUEST_TYPE p_questType) { }
    public virtual void OnCharacterLeftPartyQuest(Character p_character, PARTY_QUEST_TYPE p_questType) { }
    public virtual bool DetermineCombatBehaviour(Character p_character, CombatState p_combatState) { return false; } //returns if the character has done the combat behaviour
    #endregion
}

public class CharacterCombatBehaviourParent {
    public CharacterCombatBehaviour currentCombatBehaviour { get; private set; }
    public bool canDoTankBehaviour { get; private set; }
    public GameDate tankBehaviourActiveDate { get; private set; }

    public CharacterCombatBehaviourParent() {
        canDoTankBehaviour = true;
    }
    public CharacterCombatBehaviourParent(SaveDataCharacterCombatBehaviourParent data) {
        canDoTankBehaviour = data.canDoTankBehaviour;
        tankBehaviourActiveDate = data.tankBehaviourActiveDate;
    }

    #region Combat Behaviour
    private void SetCombatBehaviour(CharacterCombatBehaviour p_combatBehaviour, Character owner) {
        if (currentCombatBehaviour != p_combatBehaviour) {
            CharacterCombatBehaviour prevBehaviour = currentCombatBehaviour;
            currentCombatBehaviour = p_combatBehaviour;
            if (prevBehaviour != null) {
                prevBehaviour.UnsetAsCombatBehaviourOf(owner);
            }
            if (currentCombatBehaviour != null) {
                currentCombatBehaviour.SetAsCombatBehaviourOf(owner);
            }
        }
    }
    public void SetCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR p_behaviourType, Character owner) {
        CharacterCombatBehaviour behaviour = CombatManager.Instance.GetCombatBehaviour(p_behaviourType);
        SetCombatBehaviour(behaviour, owner);
    }
    public bool IsCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR p_behaviourType) {
        return currentCombatBehaviour?.behaviourType == p_behaviourType;
    }
    public bool TryDoCombatBehaviour(Character p_character, CombatState p_combatState) {
        if(currentCombatBehaviour != null) {
            return currentCombatBehaviour.DetermineCombatBehaviour(p_character, p_combatState);
        }
        return false;
    }
    #endregion

    #region Tank
    public void SetCanDoTankBehaviour(bool p_state) {
        if(canDoTankBehaviour != p_state) {
            canDoTankBehaviour = p_state;
            if (!canDoTankBehaviour) {
                tankBehaviourActiveDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(1));
                SchedulingManager.Instance.AddEntry(tankBehaviourActiveDate, () => SetCanDoTankBehaviour(true), null);
            }
        }
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataCharacterCombatBehaviourParent data) {
        currentCombatBehaviour = CombatManager.Instance.GetCombatBehaviour(data.currentCombatBehaviour);
        if (!canDoTankBehaviour) {
            SchedulingManager.Instance.AddEntry(tankBehaviourActiveDate, () => SetCanDoTankBehaviour(true), null);
        }
    }
    #endregion
}

public class SaveDataCharacterCombatBehaviourParent : SaveData<CharacterCombatBehaviourParent> {
    public CHARACTER_COMBAT_BEHAVIOUR currentCombatBehaviour { get; private set; }
    public bool canDoTankBehaviour;
    public GameDate tankBehaviourActiveDate;

    public override void Save(CharacterCombatBehaviourParent data) {
        base.Save(data);
        canDoTankBehaviour = data.canDoTankBehaviour;
        tankBehaviourActiveDate = data.tankBehaviourActiveDate;

        currentCombatBehaviour = CHARACTER_COMBAT_BEHAVIOUR.None;
        if (data.currentCombatBehaviour != null) {
            currentCombatBehaviour = data.currentCombatBehaviour.behaviourType;
        }
    }

    public override CharacterCombatBehaviourParent Load() {
        return new CharacterCombatBehaviourParent(this);
    }
}