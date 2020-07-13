using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class StateAwarenessComponent {
    public Character owner { get; private set; }

    public bool startMissingTimer { get; private set; }
    public bool startPresumedDeadTimer { get; private set; }

    public int currentMissingTicks { get; private set; }
    public int currentPresumedDeadTicks { get; private set; }

    public StateAwarenessComponent(Character owner) {
        this.owner = owner;
    }
    public void SubscribeSignals() {
        Messenger.AddListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.AddListener<Character>(Signals.CHARACTER_PRESUMED_DEAD, OnCharacterPresumedDead);
        Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
    }
    public void UnsubscribeSignals() {
        Messenger.RemoveListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_PRESUMED_DEAD, OnCharacterPresumedDead);
        Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
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
    private void OnCharacterEnteredHexTile(Character character, HexTile hex) {
        if(character == owner) {
            if(owner.homeSettlement != null) {
                if(hex.settlementOnTile != owner.homeSettlement) {
                    StartMissingTimer();
                } else {
                    StopMissingTimer();
                }
            }
        }
    }
    #endregion

    public void OnCharacterWasSeenBy(Character characterThatSaw) {
        if (characterThatSaw.isNormalCharacter) {
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
                Messenger.Broadcast(Signals.CHARACTER_MISSING, owner);
                SetStartMissingTimer(false);
                SetStartPresumedDeadTimer(true);
            } else {
                currentMissingTicks++;
            }
        } else if (startPresumedDeadTimer) {
            if (currentPresumedDeadTicks >= CharacterManager.Instance.CHARACTER_PRESUMED_DEAD_THRESHOLD) {
                Messenger.Broadcast(Signals.CHARACTER_PRESUMED_DEAD, owner);
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
            if (!startPresumedDeadTimer) {
                ResetPresumedDeadTimer();
            }
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
            }
        } else if (state == AWARENESS_STATE.Missing) {
            if (!owner.traitContainer.HasTrait("Psychopath")) {
                if (owner.relationshipContainer.GetOpinionLabel(target) == RelationshipManager.Close_Friend
                    || (owner.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.CHILD, RELATIONSHIP_TYPE.RELATIVE, RELATIONSHIP_TYPE.PARENT, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.SIBLING) && !owner.relationshipContainer.IsEnemiesWith(target))) {
                    owner.interruptComponent.TriggerInterrupt(INTERRUPT.Worried, target);
                }
            }
        }
    }
}
