using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class HeirloomHuntBehaviour : CharacterBehaviourComponent {
    public HeirloomHuntBehaviour() {
        priority = 200;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
#if DEBUG_LOG
        log += $"\n-Character is hunting heirloom";
#endif
        //HeirloomHuntParty heirloomHuntParty = character.partyComponent.currentParty as HeirloomHuntParty;
        //log += $"\n-Heirloom is already in designated spot, leave party";
        //if (heirloomHuntParty.targetHeirloom.IsInStructureSpot()) {
        //    heirloomHuntParty.RemoveMember(character);
        //    return true;
        //}

        //if (!heirloomHuntParty.isWaitTimeOver) {
        //    log += $"\n-Party is waiting";
        //    if (character.homeSettlement != null) {
        //        log += $"\n-Character has home settlement";
        //        if (character.homeSettlement.locationType == LOCATION_TYPE.DUNGEON) {
        //            log += $"\n-Character home settlement is a special structure";
        //            character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        //        } else {
        //            log += $"\n-Character home settlement is a village";
        //            LocationStructure targetStructure = null;
        //            if (character.currentStructure.structureType == STRUCTURE_TYPE.TAVERN) {
        //                targetStructure = character.currentStructure;
        //            } else {
        //                targetStructure = character.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN);
        //            }
        //            if (targetStructure == null) {
        //                if (character.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
        //                    targetStructure = character.currentStructure;
        //                } else {
        //                    targetStructure = character.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
        //                }
        //            }

        //            if (targetStructure != null) {
        //                log += $"\n-Character will roam around " + targetStructure.name;
        //                LocationGridTile targetTile = null;
        //                if (character.currentStructure != targetStructure) {
        //                    targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
        //                }
        //                character.jobComponent.TriggerRoamAroundStructure(out producedJob, targetTile);
        //            }
        //        }
        //    }
        //} else {
        //    log += $"\n-Party is not waiting";
        //    HexTile targetHex = heirloomHuntParty.targetHex;
        //    Heirloom targetHeirloom = heirloomHuntParty.targetHeirloom;
        //    if (heirloomHuntParty.foundHeirloom) {
        //        Character heirloomCarrier = heirloomHuntParty.GetMemberCarryingHeirloom();
        //        if(heirloomCarrier != null) {
        //            if (character.marker.IsPOIInVision(heirloomCarrier)) {
        //                log += $"\n-Do nothing";
        //                producedJob = null;
        //                return true;
        //            } else {
        //                log += $"\n-Character cannot see heirloom carrier, go to him";
        //                if (character.jobComponent.CreateGoToJob(heirloomCarrier)) {
        //                    producedJob = null;
        //                    return true;
        //                }
        //            }
        //        } else {
        //            if ((targetHeirloom.gridTileLocation == null && heirloomHuntParty.GetMemberCarryingHeirloom() == null) || targetHeirloom.isBeingSeized) {
        //                heirloomHuntParty.RemoveMember(character);
        //                return true;
        //            }
        //        }
        //        //if (targetHeirloom.gridTileLocation != null && targetHeirloom.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
        //        //    targetHex = targetHeirloom.gridTileLocation.hexTileOwner;
        //        //}
        //    }
        //    if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap &&
        //           character.gridTileLocation.hexTileOwner == targetHex) {
        //        Character memberInCombat = heirloomHuntParty.GetMemberInCombatExcept(character);
        //        if (memberInCombat != null && memberInCombat.gridTileLocation.collectionOwner.isPartOfParentRegionMap &&
        //            memberInCombat.gridTileLocation.hexTileOwner == targetHex) {
        //            log += $"\n-{memberInCombat.name} is in combat, will try to combat also";
        //            bool hasFought = false;
        //            CombatState combatState = memberInCombat.stateComponent.currentState as CombatState;
        //            if (combatState.currentClosestHostile != null) {
        //                CombatData combatData = memberInCombat.combatComponent.GetCombatData(combatState.currentClosestHostile);
        //                character.combatComponent.Fight(combatState.currentClosestHostile, combatData.reasonForCombat, combatData.connectedAction, combatData.isLethal);
        //                hasFought = true;
        //            }
        //            if (hasFought) {
        //                producedJob = null;
        //                return true;
        //            }
        //            log += $"\n-Roam around";
        //            character.jobComponent.TriggerRoamAroundTile(out producedJob);
        //        } else {
        //            log += $"\n-Roam around";
        //            character.jobComponent.TriggerRoamAroundTile(out producedJob);
        //        }
        //    } else {
        //        log += $"\n-Character is not in target structure, go to it";
        //        LocationGridTile targetTile = targetHex.GetRandomTile();
        //        character.jobComponent.CreatePartyGoToJob(targetTile, out producedJob);
        //    }
        //}
        if (producedJob != null) {
            producedJob.SetIsThisAPartyJob(true);
        }
        return true;
    }

    private bool RoamAroundStructureOrHex(Character actor, IPartyQuestTarget target, out JobQueueItem producedJob) {
        if(target.currentStructure != null && target.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS) {
            if(target is Character targetCharacter) {
                Area targetArea = targetCharacter.areaLocation;
                return actor.jobComponent.TriggerRoamAroundTile(JOB_TYPE.ROAM_AROUND_STRUCTURE, out producedJob, targetArea.gridTileComponent.GetRandomTile());
            }
        }
        return actor.jobComponent.TriggerRoamAroundStructure(out producedJob);
    }
}
