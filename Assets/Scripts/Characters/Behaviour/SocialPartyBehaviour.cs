using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SocialPartyBehaviour : CharacterBehaviourComponent {
    public SocialPartyBehaviour() {
        priority = 450;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-Character is partying";
        Party socialParty = character.partyComponent.currentParty;
        if (!socialParty.isWaitTimeOver) {
            if (character.currentStructure == socialParty.target) {
                log += $"\n-Character is already in target structure, will do party jobs";
                if(character.previousCurrentActionNode != null && character.previousCurrentActionNode.associatedJobType == JOB_TYPE.PARTY_GO_TO) {
                    character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                } else {
                    int roll = UnityEngine.Random.Range(0, 100);
                    if (roll < 15) {
                        if (character.jobComponent.TriggerSingJob(out producedJob)) {
                            return true;
                        }
                    } else if (roll >= 15 && roll < 30) {
                        if (character.jobComponent.TriggerDanceJob(out producedJob)) {
                            return true;
                        }
                    } else if (roll >= 30 && roll < 40) {
                        TileObject tileObject = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.TABLE);
                        if (tileObject != null) {
                            if (character.jobComponent.TriggerDrinkJob(tileObject as Table, out producedJob)) {
                                return true;
                            }
                        }
                    } else if (roll >= 40 && roll < 50) {
                        TileObject tileObject = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.TABLE);
                        if (tileObject != null) {
                            if (character.jobComponent.TriggerPartyEatJob(tileObject as Table, out producedJob)) {
                                return true;
                            }
                        }
                    } else if (roll >= 50 && roll < 70) {
                        Character chosenCharacter = character.currentStructure.GetRandomCharacterThatMeetCriteria(x => !x.combatComponent.isInCombat && x.canPerform && x.canWitness && !x.isDead);
                        if (chosenCharacter != null) {
                            if (character.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, chosenCharacter)) {
                                return true;
                            }
                        }
                    } else if (roll >= 70 && roll < 85) {
                        TileObject tileObject = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK);
                        if (tileObject != null) {
                            if (character.jobComponent.TriggerPlayCardsJob(tileObject as Desk, out producedJob)) {
                                return true;
                            }
                        }
                    }
                    character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                }
            } else {
                log += $"\n-Character is not in target structure, go to it";
                if (socialParty.target is LocationStructure targetStructure) {
                    LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
                    character.jobComponent.CreatePartyGoToJob(targetTile, out producedJob);
                }
            }
        }
        return true;
    }
}
