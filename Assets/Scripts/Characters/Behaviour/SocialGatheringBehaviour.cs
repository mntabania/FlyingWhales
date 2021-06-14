using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SocialGatheringBehaviour : CharacterBehaviourComponent {
    public SocialGatheringBehaviour() {
        priority = 450;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;
#if DEBUG_LOG
        log += $"\n-Character is partying";
#endif
        //Party socialParty = character.partyComponent.currentParty;
        Gathering socialGathering = character.gatheringComponent.currentGathering;
        if (!socialGathering.isWaitTimeOver) {
            if (character.currentStructure == socialGathering.target) {
#if DEBUG_LOG
                log += $"\n-Character is already in target structure, will do party jobs";
#endif
                if (character.previousCharacterDataComponent.previousJobType == JOB_TYPE.PARTY_GO_TO) {
                    hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                } else {
                    int roll = UnityEngine.Random.Range(0, 100);
                    if (roll < 15) {
                        hasJob = character.jobComponent.TriggerSingJob(out producedJob);
                    } else if (roll >= 15 && roll < 30) {
                        hasJob = character.jobComponent.TriggerDanceJob(out producedJob);
                    } else if (roll >= 30 && roll < 40) {
                        TileObject tileObject = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.TABLE);
                        if (tileObject != null) {
                            hasJob = character.jobComponent.TriggerPartyDrinkJob(tileObject as Table, out producedJob);
                        }
                    } else if (roll >= 40 && roll < 50) {
                        //Note: Removed eating while partying because this might lead to the character getting a food in the city center to drop at the table in the tavern

                        //TileObject tileObject = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.TABLE);
                        //if (tileObject != null) {
                        //    hasJob = character.jobComponent.TriggerPartyEatJob(tileObject as Table, out producedJob);
                        //}
                        TileObject tileObject = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK);
                        if (tileObject != null) {
                            hasJob = character.jobComponent.TriggerPlayCardsJob(tileObject as Desk, out producedJob);
                        }
                    } else if (roll >= 50 && roll < 70) {
                        Character chosenCharacter = character.currentStructure.GetRandomCharacterThatIsAliveCanPerformAndWitnessAndNotInCombatExcept(character);
                        if (chosenCharacter != null && character.nonActionEventsComponent.CanChat(chosenCharacter)) {
                            hasJob = character.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, chosenCharacter);
                        }
                    } else if (roll >= 70 && roll < 85) {
                        TileObject tileObject = character.currentStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.DESK);
                        if (tileObject != null) {
                            hasJob = character.jobComponent.TriggerPlayCardsJob(tileObject as Desk, out producedJob);
                        }
                    } else {
                        hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    }

                }
            } else {
#if DEBUG_LOG
                log += $"\n-Character is not in target structure, go to it";
#endif
                if (socialGathering.target is LocationStructure targetStructure) {
                    LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
                    hasJob = character.jobComponent.CreatePartyGoToJob(targetTile, out producedJob);
                }
            }
        }
        if (producedJob != null) {
            producedJob.SetIsThisAGatheringJob(true);
        }
        return hasJob;
    }
}
