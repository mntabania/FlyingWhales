using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class CharacterTraitComponent : CharacterComponent {

    public CharacterTraitComponent() {
    }

    #region Agoraphobia
    public void SubscribeToAgoraphobiaLevelUpSignal() {
        Messenger.AddListener<SkillData>("AgoraphobiaLevelUp", OnAgoraphobiaLevelUp);
    }
    public void UnsubscribeToAgoraphobiaLevelUpSignal() {
        Messenger.RemoveListener<SkillData>("AgoraphobiaLevelUp", OnAgoraphobiaLevelUp);
    }
    private void OnAgoraphobiaLevelUp(SkillData p_skill) {
        if (p_skill.currentLevel >= 3) {
            if (owner.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.AGORAPHOBIA)) {
                if (owner.partyComponent.hasParty) {
                    owner.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Party, owner, "Agoraphobic");
                }
            }
        }
    }
    #endregion
}