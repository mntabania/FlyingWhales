using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

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
            AssaultDemonicStructure(out List<Character> attackingCharacters);
            Messenger.Broadcast(Signals.THREAT_MAXED_OUT, attackingCharacters);
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

    private void AssaultDemonicStructure(out List<Character> attackingCharacters) {
        string debugLog = string.Empty;
        LocationStructure targetDemonicStructure = null;
        if (InnerMapManager.Instance.HasExistingWorldKnownDemonicStructure()) {
            targetDemonicStructure = InnerMapManager.Instance.worldKnownDemonicStructures[UnityEngine.Random.Range(0, InnerMapManager.Instance.worldKnownDemonicStructures.Count)];
        } else {
            targetDemonicStructure = PlayerManager.Instance.player.playerSettlement.GetRandomStructure();
        }
        debugLog += "TARGET: " + targetDemonicStructure.name;
        List<Character> characters = new List<Character>();
        int count = 0;
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if(character.canPerform && character.canMove && character.canWitness && character.faction.isMajorNonPlayerFriendlyNeutral
                && (character.race == RACE.HUMANS || character.race == RACE.ELVES) && !character.isInCombat
                && !(character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE)
                && character.traitContainer.HasTrait("Combatant")) {
                count++;
                debugLog += "RETALIATOR: " + character.name;
                characters.Add(character);
                character.behaviourComponent.SetIsAttackingDemonicStructure(true, targetDemonicStructure as DemonicStructure);
                if(count >= 5) {
                    break;
                }
            }
        }
        if (characters.Count > 0) {
            Character chosenCharacter = CollectionUtilities.GetRandomElement(characters);
            UIManager.Instance.ShowYesNoConfirmation("Threat Response", 
                "Your threat level has reached maximum. The people will now retaliate!", 
                onClickNoAction: chosenCharacter.CenterOnCharacter, yesBtnText: "OK", noBtnText: "Jump to an attacker", 
                showCover:true, pauseAndResume: true);    
        }
        attackingCharacters = characters;
        // PlayerUI.Instance.ShowGeneralConfirmation("Threat Response", "Your threat level has reached maximum. The people will now retaliate!");
        Debug.Log(debugLog);
    }
}
