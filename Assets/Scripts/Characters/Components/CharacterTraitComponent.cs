using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class CharacterTraitComponent : CharacterComponent {

    public bool hasAgoraphobicReactedThisTick { get; private set; }

    public CharacterTraitComponent() {
    }
    public CharacterTraitComponent(SaveDataCharacterTraitComponent data) {
        hasAgoraphobicReactedThisTick = data.hasAgoraphobicReactedThisTick;
    }

    #region Agoraphobia
    public void SubscribeToAgoraphobiaLevelUpSignal() {
        Messenger.AddListener<SkillData>("AgoraphobiaLevelUp", OnAgoraphobiaLevelUp);
    }
    public void UnsubscribeToAgoraphobiaLevelUpSignal() {
        Messenger.RemoveListener<SkillData>("AgoraphobiaLevelUp", OnAgoraphobiaLevelUp);
    }
    public void OnAgoraphobiaLevelUp(SkillData p_skill) {
        if (p_skill.currentLevel >= 3) {
            if (owner.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.AGORAPHOBIA)) {
                if (owner.partyComponent.hasParty) {
                    owner.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Party, owner, "Agoraphobic");
                }
            }
        }
    }
    public void SetHasAgoraphobicReactedThisTick(bool p_state) {
        if(hasAgoraphobicReactedThisTick != p_state) {
            hasAgoraphobicReactedThisTick = p_state;
            if (hasAgoraphobicReactedThisTick) {
                GameDate date = GameManager.Instance.Today();
                date.AddTicks(1);
                SchedulingManager.Instance.AddEntry(date, () => SetHasAgoraphobicReactedThisTick(false), this);
            }
        }
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataCharacterTraitComponent data) {
    }
    #endregion
}

[System.Serializable]
public class SaveDataCharacterTraitComponent : SaveData<CharacterTraitComponent> {
    public bool hasAgoraphobicReactedThisTick;

    #region Overrides
    public override void Save(CharacterTraitComponent data) {
        hasAgoraphobicReactedThisTick = data.hasAgoraphobicReactedThisTick;
    }

    public override CharacterTraitComponent Load() {
        CharacterTraitComponent component = new CharacterTraitComponent(this);
        return component;
    }
    #endregion
}