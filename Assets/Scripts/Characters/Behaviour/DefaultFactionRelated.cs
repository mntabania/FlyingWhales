using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class DefaultFactionRelated : CharacterBehaviourComponent {
    public DefaultFactionRelated() {
        priority = 26;
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.DO_NOT_SKIP_PROCESSING/*, BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY*/ };
    }

    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        if(UnityEngine.Random.Range(0, 100) < 15) {
            if (character.isFriendlyFactionless) {
                log += $"\n-{character.name} is factionless, 15% chance to join faction";
                Faction chosenFaction = character.JoinFactionProcessing();
                if (chosenFaction != null) {
                    log += $"\n-Chosen faction to join: {chosenFaction.name}";
                } else {
                    log += "\n-No available faction that the character fits the ideology";
                }
            }
            return true;
        } else if (UnityEngine.Random.Range(0, 100) < 10) {
            if (character.isFriendlyFactionless) {
                log += $"\n-{character.name} is factionless, 10% chance to create faction";
                if (character.traitContainer.HasTrait("Inspiring", "Ambitious")) {
                    log += $"\n-{character.name} is Ambitious or Inspiring, creating new faction...";
                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character);
                }
            }
            return true;
        }

        return false;
    }
}
