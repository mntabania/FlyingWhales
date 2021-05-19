using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class CharacterTraitComponent : CharacterComponent {

    public bool hasAgoraphobicReactedThisTick { get; private set; }
    public bool willProcessPlayerSourceChaosOrb { get; private set; }
    public bool isOtherTick { get; private set; }
    public CharacterTraitComponent() {
    }
    public CharacterTraitComponent(SaveDataCharacterTraitComponent data) {
        hasAgoraphobicReactedThisTick = data.hasAgoraphobicReactedThisTick;
        willProcessPlayerSourceChaosOrb = data.willProcessPlayerSourceChaosOrb;
        isOtherTick = data.isOtherTick;
    }

    #region General
    public void OnCharacterFinishedJob(JobQueueItem p_job) {
        if(p_job is GoapPlanJob job && job.targetInteractionType == INTERACTION_TYPE.EAT_CORPSE) {
            if (owner.traitContainer.HasTrait("Hunting")) {
                owner.traitContainer.RemoveTrait(owner, "Hunting");
            }
        }
    }
    #endregion

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

    #region Glutton
    public void SubscribeToGluttonLevelUpSignal() {
        Messenger.AddListener<SkillData>("GluttonyLevelUp", OnGluttonLeveledUp);
    }
    public void UnsubscribeToGluttonLevelUpSignal() {
        Messenger.RemoveListener<SkillData>("GluttonyLevelUp", OnGluttonLeveledUp);
    }
    private void OnGluttonLeveledUp(SkillData p_skillData) {
        Glutton glutton = owner.traitContainer.GetTraitOrStatus<Glutton>("Glutton");
        glutton?.OnGluttonLeveledUp();
    }
    #endregion

    #region Player Source Chaos Orb
    public void SetWillProcessPlayerSourceChaosOrb(bool p_state) {
        if (willProcessPlayerSourceChaosOrb != p_state) {
            willProcessPlayerSourceChaosOrb = p_state;
            if (willProcessPlayerSourceChaosOrb) {
                Messenger.AddListener(Signals.TICK_STARTED, TickStartedProcessPlayerSourceChaosOrb);
            } else {
                if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
                    Messenger.RemoveListener(Signals.TICK_STARTED, TickStartedProcessPlayerSourceChaosOrb);
                }
            }
        }
    }
    private void TickStartedProcessPlayerSourceChaosOrb() {
        if (!isOtherTick) {
            isOtherTick = true;
        } else {
            isOtherTick = false;
            ProcessPlayerSourceChaosOrb();
        }
    }
    private void ProcessPlayerSourceChaosOrb() {
        if (!owner.isDead && owner.isNormalAndNotAlliedWithPlayer) {
            if (GameUtilities.RollChance(20)) {
                LocationGridTile gridTile = owner.gridTileLocation;
                if (gridTile != null) {
#if DEBUG_LOG
                    Debug.Log("Chaos Orb Produced - [" + owner.name + "] - [Status Effect] - [" + 1 + "]");
#endif
                    Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, gridTile.centeredWorldLocation, 1, gridTile.parentMap);
                }
            }
        }
    }
#endregion

#region Loading
    public void LoadReferences(SaveDataCharacterTraitComponent data) {
        if (willProcessPlayerSourceChaosOrb) {
            Messenger.AddListener(Signals.TICK_STARTED, TickStartedProcessPlayerSourceChaosOrb);
        }
    }
#endregion
}

[System.Serializable]
public class SaveDataCharacterTraitComponent : SaveData<CharacterTraitComponent> {
    public bool hasAgoraphobicReactedThisTick;
    public bool willProcessPlayerSourceChaosOrb;
    public bool isOtherTick;

#region Overrides
    public override void Save(CharacterTraitComponent data) {
        hasAgoraphobicReactedThisTick = data.hasAgoraphobicReactedThisTick;
        willProcessPlayerSourceChaosOrb = data.willProcessPlayerSourceChaosOrb;
        isOtherTick = data.isOtherTick;
    }

    public override CharacterTraitComponent Load() {
        CharacterTraitComponent component = new CharacterTraitComponent(this);
        return component;
    }
#endregion
}