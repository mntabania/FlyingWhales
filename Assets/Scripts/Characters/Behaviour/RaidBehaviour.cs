using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class RaidBehaviour : CharacterBehaviourComponent {
    public RaidBehaviour() {
        priority = 200;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;
#if DEBUG_LOG
        log += $"\n-Character is raiding";
#endif
        Party party = character.partyComponent.currentParty;
        if (party.isActive && party.partyState == PARTY_STATE.Working) {
#if DEBUG_LOG
            log += $"\n-Party is working";
#endif
            if (party.targetDestination.IsAtTargetDestination(character)) {
#if DEBUG_LOG
                log += $"\n-Character is at target destination, do work";
#endif
                RaidPartyQuest quest = party.currentQuest as RaidPartyQuest;
                if (quest.target == null) {
                    party.GoBackHomeAndEndQuest();
                    return true;
                } else {
                    //if (quest.target is BaseSettlement targetSettlement) {
                    //    if (targetSettlement.owner == null || character.faction == null || !character.faction.IsHostileWith(targetSettlement.owner)) {
                    //        party.GoBackHomeAndEndQuest();
                    //        log += $"\n-No settlement/character faction or factions are no longer hostile, leave party";
                    //        return true;
                    //    }
                    //}
                }

                //Character target = GetRandomAliveResidentInsideSettlementThatIsHostileWith(character, quest.targetSettlement);
                //if (target != null) {
                //    log += $"\n-Chosen target is {target.name}";
                //    character.combatComponent.Fight(target, CombatManager.Hostility, isLethal: false);
                //    return true;
                //} else {
                if(party.targetDestination is BaseSettlement settlement) {
#if DEBUG_LOG
                    log += $"\n-Roam around";
#endif
                    LocationStructure structure = settlement.GetRandomDwellingOrResourceProducingStructure();
                    if(structure != null) {
                        LocationGridTile tile = structure.GetRandomPassableTile();
                        if(tile != null) {
                            hasJob = character.jobComponent.CreateGoToSpecificTileJob(tile, out producedJob);
                        } else {
                            hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                        }
                    } else {
                        hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    }
                }
                //}
            } 
            //else {
            //    LocationGridTile tile = party.targetDestination.GetRandomPassableTile();
            //    hasJob = character.jobComponent.CreatePartyGoToJob(tile, out producedJob);
            //}
        }

        //if(party.target == null) {
        //    party.RemoveMember(character);
        //    log += $"\n-No target settlement, leave party";
        //    return true;
        //} else {
        //    if (party.target is BaseSettlement targetSettlement) {
        //        if (targetSettlement.owner == null || character.faction == null || !character.faction.IsHostileWith(targetSettlement.owner)) {
        //            party.RemoveMember(character);
        //            log += $"\n-No settlement/character faction or factions are no longer hostile, leave party";
        //            return true;
        //        }
        //    }
        //}
        //if (!party.isWaitTimeOver) {
        //    log += $"\n-Party is waiting";
        //    if(party.waitingHexArea != null) {
        //        log += $"\n-Party has waiting area";
        //        if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
        //            if (character.gridTileLocation.hexTileOwner == party.waitingHexArea) {
        //                log += $"\n-Character is in waiting area, roam";
        //                character.jobComponent.TriggerRoamAroundTile(out producedJob);
        //            } else {
        //                log += $"\n-Character is not in waiting area, go to it";
        //                LocationGridTile targetTile = party.waitingHexArea.GetRandomTile();
        //                character.jobComponent.CreatePartyGoToJob(targetTile, out producedJob);
        //            }
        //        }
        //    } else {
        //        log += $"\n-Party has no waiting area";
        //    }
        //} else {
        //    log += $"\n-Party is not waiting";
        //    if(character.currentStructure.settlementLocation == party.target) {
        //        log += $"\n-Character is already in target settlement";
        //        Character target = character.currentStructure.settlementLocation.GetRandomAliveResidentInsideSettlement();
        //        if (target != null) {
        //            log += $"\n-Chosen target is {target.name}";
        //            character.combatComponent.Fight(target, CombatManager.Hostility);
        //        } else {
        //            log += $"\n-Roam around";
        //            character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        //        }
        //    } else {
        //        log += $"\n-Character is not in target structure, go to it";
        //        if (party.target is BaseSettlement targetSettlement) {
        //            LocationStructure targetStructure = UtilityScripts.CollectionUtilities.GetRandomElement(targetSettlement.allStructures);
        //            LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
        //            character.jobComponent.CreatePartyGoToJob(targetTile, out producedJob);
        //        }
        //    }
        //}
        if (producedJob != null) {
            producedJob.SetIsThisAPartyJob(true);
        }
        return hasJob;
    }
    private Character GetRandomAliveResidentInsideSettlementThatIsHostileWith(Character character, BaseSettlement settlement) {
        List<Character> choices = null;
        for (int i = 0; i < settlement.residents.Count; i++) {
            Character resident = settlement.residents[i];
            if (character != resident
                && !resident.isDead
                && !resident.isBeingSeized
                && resident.gridTileLocation != null
                && resident.gridTileLocation.IsPartOfSettlement(settlement)
                && (resident.faction == null || character.faction == null || character.faction.IsHostileWith(resident.faction))) {
                if (choices == null) { choices = new List<Character>(); }
                choices.Add(resident);
            }
        }
        if (choices != null && choices.Count > 0) {
            return choices[UnityEngine.Random.Range(0, choices.Count)];
        }
        return null;
    }
}
