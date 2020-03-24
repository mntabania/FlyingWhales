using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class ThreatComponent {
    public Player player { get; private set; }

    public const int MAX_THREAT = 100;
    public int threat;

    public ThreatComponent(Player player) {
        this.player = player;
        Messenger.AddListener(Signals.TICK_STARTED, PerTick);
    }

    public void AdjustThreat(int amount) {
        int supposedThreat = threat + amount;
        bool hasReachedMax = false;
        if (threat != MAX_THREAT) {
            hasReachedMax = supposedThreat >= MAX_THREAT;
        }
        threat = supposedThreat;
        threat = Mathf.Clamp(threat, 0, 100);
        Messenger.Broadcast(Signals.THREAT_UPDATED);
        if (hasReachedMax) {
            AssaultDemonicStructure();
            ResetThreat();
        }
        //TODO: Threat Response - Assault Demonic Structure
    }
    public void ResetThreat() {
        threat = 0;
        Messenger.Broadcast(Signals.THREAT_UPDATED);
    }

    private void PerTick() {

    }

    private void AssaultDemonicStructure() {
        PlayerUI.Instance.ShowGeneralConfirmation("Threat Response", "Your threat level has reached maximum. The people will now retaliate!");
        string debugLog = string.Empty;
        LocationStructure targetDemonicStructure = null;
        if (InnerMapManager.Instance.HasExistingWorldKnownDemonicStructure()) {
            targetDemonicStructure = InnerMapManager.Instance.worldKnownDemonicStructures[UnityEngine.Random.Range(0, InnerMapManager.Instance.worldKnownDemonicStructures.Count)];
        } else {
            targetDemonicStructure = PlayerManager.Instance.player.playerSettlement.GetRandomStructure();
        }
        debugLog += "TARGET: " + targetDemonicStructure.name;
        int count = 0;
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if(character.canPerform && character.canMove && character.canWitness && character.faction.isMajorNonPlayerFriendlyNeutral
                && (character.race == RACE.HUMANS || character.race == RACE.ELVES) && !character.isInCombat
                && !(character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE)) {
                count++;
                debugLog += "RETALIATOR: " + character.name;
                character.behaviourComponent.SetIsAttackingDemonicStructure(true, targetDemonicStructure as DemonicStructure);
                if(count >= 5) {
                    break;
                }
            }
        }
        Debug.Log(debugLog);
    }
}
