using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class PartyBehaviour : CharacterBehaviourComponent {
    public PartyBehaviour() {
        priority = 29;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;
        Party party = character.partyComponent.currentParty;
        if (party.isActive) {
#if DEBUG_LOG
            log += $"\n-Party is active, will try to do party behaviour";
#endif
            if (party.partyState == PARTY_STATE.Waiting) {
#if DEBUG_LOG
                log += $"\n-Party is waiting";
#endif
                if (party.meetingPlace != null && !party.meetingPlace.hasBeenDestroyed && party.meetingPlace.passableTiles.Count > 0) {
                    hasJob = true;
                    if (character.currentStructure == party.meetingPlace) {
                        party.AddMemberThatJoinedQuest(character);
                        character.trapStructure.SetForcedStructure(party.meetingPlace);
                        character.needsComponent.CheckExtremeNeedsWhileInActiveParty();

                        character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    } else {
                        if (!party.CanAMemberGoTo(party.meetingPlace)) {
                            //If no party member can go to meeting place, set new meeting place
                            party.SetMeetingPlace();
                        }
                        LocationGridTile targetTile = party.meetingPlace.GetRandomPassableTile();
                        if(targetTile != null) {
                            if (character.movementComponent.HasPathToEvenIfDiffRegion(targetTile)) {
                                character.jobComponent.CreateGoToWaitingJob(targetTile, out producedJob);
                            } else {
                                hasJob = false;
                            }
                        } else {
                            //Character must go to other behaviours if there is no passable tile in meeting place, that's why this is set to false
                            party.SetMeetingPlace();
                            hasJob = false;
                        }
                    }
                }
                if (party.isPlayerParty) {
                    //if demon party, should not go through other behaviours, so return true here
                    return true;
                }
            } else if (party.partyState != PARTY_STATE.None) {
                if (party.membersThatJoinedQuest.Contains(character)) {
                    NonWaitingJoinedQuestBehaviour(character, party, ref producedJob, ref hasJob, ref log);
                    if (hasJob) {
                        if (producedJob != null) {
                            producedJob.SetIsThisAPartyJob(true);
                        }
                        return hasJob;
                    }
                } else {
                    NonWaitingNotJoinedQuestBehaviour(character, party, ref producedJob, ref hasJob, ref log);
                    if (hasJob) {
                        if (producedJob != null) {
                            producedJob.SetIsThisAPartyJob(true);
                        }
                        return hasJob;
                    }
                }
            }
        }
        //else {
        //    hasJob = false;
        //}
        if (producedJob != null) {
            producedJob.SetIsThisAPartyJob(true);
        }
        return hasJob;
        //return true;
    }
    private void NonWaitingJoinedQuestBehaviour(Character character, Party party, ref JobQueueItem producedJob, ref bool hasJob, ref string log) {
        if (party.IsMemberActive(character)) {
            hasJob = DoPartyJobsInPartyJobBoard(character, party, ref producedJob);
            if (!hasJob) { //If no job is assigned from job board continue doing behaviour
                if (party.partyState == PARTY_STATE.Moving) {
#if DEBUG_LOG
                    log += $"\n-Party is moving";
#endif
                    if (party.targetDestination != null && !party.targetDestination.hasBeenDestroyed) {
                        if (party.targetDestination.IsAtTargetDestination(character)) {
                            if (party.targetDestination == party.partySettlement) {
                                if (party.currentQuest is DemonSnatchPartyQuest quest) {
                                    if (party.jobBoard.HasJob(JOB_TYPE.SNATCH, quest.targetCharacter)) {
                                        LocationGridTile tile = character.areaLocation.gridTileComponent.GetRandomPassableTileThatIsNotPartOfAStructure();
                                        if (tile != null) {
                                            character.jobComponent.CreatePartyGoToJob(tile, out producedJob);
                                        }
                                    } else {
                                        party.currentQuest.EndQuest("Finished quest");
                                    }
                                } else {
                                    party.currentQuest.EndQuest("Finished quest");
                                }
                            } else {
                                party.SetPartyState(PARTY_STATE.Working);
                            }
                            hasJob = true;
                            return;
                            //hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                        } else {
                            if (character.partyComponent.CanFollowBeacon()) {
                                character.partyComponent.FollowBeacon();
                            } else {
                                LocationGridTile tile = null;
                                if (party.isPlayerParty && party.targetDestination == party.partySettlement) {
                                    tile = party.partySettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL).GetRandomPassableTile();
                                } else {
                                    tile = party.targetDestination.GetRandomPassableTile();
                                }
                                if (tile != null) {
                                    hasJob = character.jobComponent.CreatePartyGoToJob(tile, out producedJob);
                                }
                            }
                        }
                    }
                } else if (party.partyState == PARTY_STATE.Resting) {
#if DEBUG_LOG
                    log += $"\n-Party is resting";
#endif
                    if (party.targetRestingTavern != null && !party.targetRestingTavern.hasBeenDestroyed && party.targetRestingTavern.passableTiles.Count > 0) {
                        if (character.currentStructure == party.targetRestingTavern) {
                            //Removed this because this is the reason why the characters in party are not eating on adjacent hex tiles
                            //because they are forced to do actions inside the forced structure only
                            //character.trapStructure.SetForcedStructure(party.targetRestingTavern);
                            character.needsComponent.CheckExtremeNeedsWhileInActiveParty();

                            hasJob = TavernBehaviour(character, party, out producedJob);
                        } else {
                            LocationGridTile tile = UtilityScripts.CollectionUtilities.GetRandomElement(party.targetRestingTavern.passableTiles);
                            if (tile != null) {
                                hasJob = character.jobComponent.CreatePartyGoToJob(tile, out producedJob);
                            }
                        }
                    } else if (party.targetCamp != null) {
                        if (character.gridTileLocation != null && character.areaLocation == party.targetCamp) {
                            //Removed this because this is the reason why the characters in party are not eating on adjacent hex tiles
                            //because they are forced to do actions inside the forced hex only
                            //character.trapStructure.SetForcedHex(party.targetCamp);
                            character.needsComponent.CheckExtremeNeedsWhileInActiveParty();

                            hasJob = CampBehaviour(character, party, out producedJob);
                        } else {
                            LocationGridTile targetTile = party.targetCamp.GetRandomPassableTile();
                            hasJob = character.jobComponent.CreatePartyGoToSpecificTileJob(targetTile, out producedJob);
                        }
                    } else {
                        party.SetPartyState(PARTY_STATE.Moving);
                        hasJob = true;
                        return;
                    }
                } else if (party.partyState == PARTY_STATE.Working) {
#if DEBUG_LOG
                    log += $"\n-Party is working";
#endif
                    if (!party.targetDestination.IsAtTargetDestination(character)) {
                        if (party.hasChangedTargetDestination) {
                            party.SetHasChangedTargetDestination(false);
                            party.SetPartyState(PARTY_STATE.Moving);
                            hasJob = true;
                            return;
                        } else {
                            LocationGridTile tile = party.targetDestination.GetRandomPassableTile();
                            hasJob = character.jobComponent.CreatePartyGoToJob(tile, out producedJob);
                        }
                    }
                }
                //hasJob = true;
            }
            hasJob = true;
        }
    }
    private void NonWaitingNotJoinedQuestBehaviour(Character character, Party party, ref JobQueueItem producedJob, ref bool hasJob, ref string log) {
#if DEBUG_LOG
        log += $"\n-Character has not yet joined quest and party is no longer waiting";
#endif
        PartyQuest quest = party.currentQuest;
        if (quest.canStillJoinQuestAnytime) {
#if DEBUG_LOG
            log += $"\n-Character can still join quest anytime, will join quest";
#endif
            party.AddMemberThatJoinedQuest(character);
            hasJob = true;
        }
    }

    private bool CampBehaviour(Character character, Party party, out JobQueueItem producedJob) {
        bool hasJob;
        if (GameUtilities.RollChance(50)) {
            Campfire campfire = GetPartyCampfireInArea(character.areaLocation, character, party);
            if(campfire != null) {
                hasJob = character.jobComponent.TriggerWarmUp(campfire, out producedJob);
                if (hasJob) {
                    return hasJob;
                }
            }
        }
        hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        return hasJob;
    }

    private bool TavernBehaviour(Character character, Party party, out JobQueueItem producedJob) {
        bool hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        return hasJob;
    }

    private bool CampSetterBehaviour(Character character, Party party, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;

        Campfire campfire = GetPartyCampfireInArea(character.areaLocation, character, party);
        if (campfire == null) {
            hasJob = character.jobComponent.TriggerBuildCampfireJob(JOB_TYPE.BUILD_CAMP, out producedJob);
        }
        return hasJob;
    }

    private bool FoodProducerBehaviour(Character character, Party party, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;

        if(HasMemberThatIsHungryOrStarvingAndThereIsNoFoodInCamp(character.areaLocation, party)) {
            hasJob = character.jobComponent.CreateProduceFoodForCampJob(out producedJob);
        }
        return hasJob;
    }

    private Campfire GetPartyCampfireInArea(Area p_area, Character character, Party party) {
        Campfire chosenCampfire = null;
        for (int i = 0; i < p_area.gridTileComponent.gridTiles.Count; i++) {
            LocationGridTile tile = p_area.gridTileComponent.gridTiles[i];
            if (tile.tileObjectComponent.objHere != null && tile.tileObjectComponent.objHere is Campfire campfire) {
                if (campfire.characterOwner == null 
                    || campfire.IsOwnedBy(character) 
                    || (!character.IsHostileWith(campfire.characterOwner) && !character.relationshipContainer.IsEnemiesWith(campfire.characterOwner))
                    || party.IsMember(campfire.characterOwner)) {
                    chosenCampfire = campfire;
                    break;
                }
            }
        }
        return chosenCampfire;
    }

    private bool HasMemberThatIsHungryOrStarvingAndThereIsNoFoodInCamp(Area p_area, Party party) {
        bool hasHungryStarvingMember = false;
        for (int i = 0; i < party.membersThatJoinedQuest.Count; i++) {
            Character character = party.membersThatJoinedQuest[i];
            if (party.IsMemberActive(character)) {
                if(character.needsComponent.isHungry || character.needsComponent.isStarving) {
                    hasHungryStarvingMember = true;
                    break;
                }
            }
        }
        return hasHungryStarvingMember && GetFoodPileInCamp(p_area, party) == null;
    }

    private FoodPile GetFoodPileInCamp(Area p_area, Party party) {
        FoodPile chosenFoodPile = null;
        for (int i = 0; i < p_area.gridTileComponent.gridTiles.Count; i++) {
            LocationGridTile tile = p_area.gridTileComponent.gridTiles[i];
            if (tile.tileObjectComponent.objHere != null && tile.tileObjectComponent.objHere is FoodPile foodPile && 
                foodPile.resourceStorageComponent.GetResourceValue(RESOURCE.FOOD) >= 12) {
                chosenFoodPile = foodPile;
                break;
            }
        }
        return chosenFoodPile;
    }
}
