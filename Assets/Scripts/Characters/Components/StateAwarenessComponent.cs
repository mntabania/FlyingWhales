using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Traits;

public class StateAwarenessComponent : CharacterComponent {

    public bool startMissingTimer { get; private set; }
    public bool startPresumedDeadTimer { get; private set; }

    public int currentMissingTicks { get; private set; }
    public int currentPresumedDeadTicks { get; private set; }

    public StateAwarenessComponent() {
    }

    public StateAwarenessComponent(SaveDataStateAwarenessComponent data) {
        startMissingTimer = data.startMissingTimer;
        startPresumedDeadTimer = data.startPresumedDeadTimer;
        currentMissingTicks = data.currentMissingTicks;
        currentPresumedDeadTicks = data.currentPresumedDeadTicks;
    }

    public void SubscribeSignals() {
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_PRESUMED_DEAD, OnCharacterPresumedDead);
        Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, OnCharacterEnteredArea);
    }
    public void UnsubscribeSignals() {
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_PRESUMED_DEAD, OnCharacterPresumedDead);
        Messenger.RemoveListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, OnCharacterEnteredArea);
    }

    #region Listeners
    private void OnCharacterMissing(Character missingCharacter) {
        if (missingCharacter != owner) {
            owner.relationshipContainer.SetAwarenessState(owner, missingCharacter, AWARENESS_STATE.Missing);
        }
    }
    private void OnCharacterPresumedDead(Character presumedDeadCharacter) {
        if (presumedDeadCharacter != owner) {
            owner.relationshipContainer.SetAwarenessState(owner, presumedDeadCharacter, AWARENESS_STATE.Presumed_Dead);
        }
    }
    private void OnCharacterEnteredArea(Character character, Area p_area) {
        if(character == owner) {
            if(owner.homeSettlement != null) {
                if(p_area.settlementOnArea != owner.homeSettlement) {
                    StartMissingTimer();
                } else {
                    StopMissingTimer();
                }
            }
        }
    }
    #endregion

    public void OnCharacterWasSeenBy(Character characterThatSaw) {
        //Should only be available when seen by its own factionmate
        if (characterThatSaw.isNormalCharacter && characterThatSaw.faction == owner.faction) {
            StopMissingTimer();
            if (owner.isDead) {
                characterThatSaw.relationshipContainer.SetAwarenessState(characterThatSaw, owner, AWARENESS_STATE.Presumed_Dead);
            } else {
                characterThatSaw.relationshipContainer.SetAwarenessState(characterThatSaw, owner, AWARENESS_STATE.Available);
            }
        }
    }

    public void PerTick() {
        if (startMissingTimer) {
            if(currentMissingTicks >= CharacterManager.Instance.CHARACTER_MISSING_THRESHOLD) {
                Messenger.Broadcast(CharacterSignals.CHARACTER_MISSING, owner);
                SetStartMissingTimer(false);
                SetStartPresumedDeadTimer(true);
            } else {
                currentMissingTicks++;
            }
        } else if (startPresumedDeadTimer) {
            if (currentPresumedDeadTicks >= CharacterManager.Instance.CHARACTER_PRESUMED_DEAD_THRESHOLD) {
                Messenger.Broadcast(CharacterSignals.CHARACTER_PRESUMED_DEAD, owner);
                SetStartMissingTimer(false);
            } else {
                currentPresumedDeadTicks++;
            }
        }
    }
    public void StartMissingTimer() {
        if (!startPresumedDeadTimer) {
            SetStartMissingTimer(true);
        }
    }
    public void StopMissingTimer() {
        SetStartMissingTimer(false);
    }
    private void SetStartMissingTimer(bool state) {
        if (startMissingTimer != state) {
            startMissingTimer = state;
            if (!startMissingTimer) {
                ResetMissingTimer();
            }
        }
        if(!state && startPresumedDeadTimer) {
            SetStartPresumedDeadTimer(false);
        }
    }
    private void SetStartPresumedDeadTimer(bool state) {
        if (startPresumedDeadTimer != state) {
            startPresumedDeadTimer = state;
            ResetPresumedDeadTimer();
        }
    }
    public void ResetMissingTimer() {
        currentMissingTicks = 0;
        SetStartPresumedDeadTimer(false);
    }
    public void ResetPresumedDeadTimer() {
        currentPresumedDeadTicks = 0;
    }
    public void OnSetAwarenessState(Character target, AWARENESS_STATE state) {
        if(state == AWARENESS_STATE.Presumed_Dead) {
            if (!owner.traitContainer.HasTrait("Psychopath")) {
                if(owner.relationshipContainer.GetOpinionLabel(target) == RelationshipManager.Close_Friend
                    || (owner.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.CHILD, RELATIONSHIP_TYPE.RELATIVE, RELATIONSHIP_TYPE.PARENT, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.SIBLING) && !owner.relationshipContainer.IsEnemiesWith(target))) {
                    owner.traitContainer.AddTrait(owner, "Griefstricken", target);
                }
            } else {
                Psychopath psychopath = owner.traitContainer.GetTraitOrStatus<Psychopath>("Psychopath");
                if(psychopath.targetVictim == target) {
                    owner.jobQueue.CancelAllJobs(JOB_TYPE.RITUAL_KILLING);
                    psychopath.SetTargetVictim(null);
                }
            }
        } else if (state == AWARENESS_STATE.Missing) {
            if (!owner.traitContainer.HasTrait("Psychopath")) {
                if (owner.relationshipContainer.GetOpinionLabel(target) == RelationshipManager.Close_Friend
                    || (owner.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.CHILD, RELATIONSHIP_TYPE.RELATIVE, RELATIONSHIP_TYPE.PARENT, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.SIBLING) && !owner.relationshipContainer.IsEnemiesWith(target))) {
                    owner.interruptComponent.TriggerInterrupt(INTERRUPT.Worried, target);
                }
            } else {
                Psychopath psychopath = owner.traitContainer.GetTraitOrStatus<Psychopath>("Psychopath");
                if (psychopath.targetVictim == target) {
                    owner.jobQueue.CancelAllJobs(JOB_TYPE.RITUAL_KILLING);
                    psychopath.SetTargetVictim(null);
                }
            }
        }
    }

    #region Loading
    public void LoadReferences(SaveDataStateAwarenessComponent data) {
        //Currently N/A
    }
    #endregion
}

[System.Serializable]
public class SaveDataStateAwarenessComponent : SaveData<StateAwarenessComponent> {
    public bool startMissingTimer;
    public bool startPresumedDeadTimer;

    public int currentMissingTicks;
    public int currentPresumedDeadTicks;

    #region Overrides
    public override void Save(StateAwarenessComponent data) {
        startMissingTimer = data.startMissingTimer;
        startPresumedDeadTimer = data.startPresumedDeadTimer;
        currentMissingTicks = data.currentMissingTicks;
        currentPresumedDeadTicks = data.currentPresumedDeadTicks;
    }

    public override StateAwarenessComponent Load() {
        StateAwarenessComponent component = new StateAwarenessComponent(this);
        return component;
    }
    #endregion
}