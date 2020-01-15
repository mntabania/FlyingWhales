﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultBaseStructure : CharacterBehaviourComponent {
    public DefaultBaseStructure() {
        priority = 50;
        //attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        if (character.trapStructure.structure != null && character.currentStructure == character.trapStructure.structure) {
            log += "\n-" + character.name + "'s Base Structure is not empty and current structure is the Base Structure";
            log += "\n-15% chance to trigger a Chat conversation if there is anyone chat-compatible in range";
            int chance = UnityEngine.Random.Range(0, 100);
            log += "\n  -RNG roll: " + chance;
            if (chance < 15) {
                if (character.marker.inVisionCharacters.Count > 0) {
                    bool hasForcedChat = false;
                    for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
                        Character targetCharacter = character.marker.inVisionCharacters[i];
                        //if (character.nonActionEventsComponent.ForceChatCharacter(targetCharacter)) {
                        if (character.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, targetCharacter)) {
                            log += "\n  -Chat with: " + targetCharacter.name;
                            hasForcedChat = true;
                            break;
                        }
                    }
                    if (hasForcedChat) {
                        return true;
                    } else {
                        log += "\n  -Could not chat with anyone in vision";
                    }
                } else {
                    log += "\n  -No characters in vision";
                }
            }
            log += "\n-Sit if there is still an unoccupied Table or Desk";
            TileObject deskOrTable = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK, TILE_OBJECT_TYPE.TABLE);
            if (deskOrTable != null) {
                log += "\n  -" + character.name + " will do action Sit on " + deskOrTable.ToString();
                character.PlanIdle(INTERACTION_TYPE.SIT, deskOrTable);
                return true;
            } else {
                log += "\n  -No unoccupied Table or Desk";
            }
            log += "\n-Otherwise, stand idle";
            log += "\n  -" + character.name + " will do action Stand";
            character.PlanIdle(INTERACTION_TYPE.STAND, character);
            return true;
        }
        return false;
    }
}
