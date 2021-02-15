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
            log += $"\n-Party is active, will try to do party behaviour";
            if (party.partyState == PARTY_STATE.Waiting) {
                log += $"\n-Party is waiting";
                if(party.meetingPlace != null && !party.meetingPlace.hasBeenDestroyed && party.meetingPlace.passableTiles.Count > 0) {
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
            } else {
                if (party.membersThatJoinedQuest.Contains(character)) {
                    if (party.IsMemberActive(character)) {
                        bool stillProcess = true;
                        if (character.limiterComponent.canTakeJobs) {
                            JobQueueItem jobToAssign = party.jobBoard.GetFirstJobBasedOnVision(character);
                            if (jobToAssign != null) {
                                producedJob = jobToAssign;
                                hasJob = true;
                                stillProcess = false;
                            } else {
                                jobToAssign = party.jobBoard.GetFirstUnassignedJobToCharacterJob(character);
                                if (jobToAssign != null) {
                                    producedJob = jobToAssign;
                                    hasJob = true;
                                    stillProcess = false;
                                }
                            }
                        }
                        if (stillProcess) {
                            if (party.partyState == PARTY_STATE.Moving) {
                                log += $"\n-Party is moving";
                                if (party.targetDestination != null && !party.targetDestination.hasBeenDestroyed) {
                                    if (party.targetDestination.IsAtTargetDestination(character)) {
                                        if (party.targetDestination == party.partySettlement) {
                                            party.currentQuest.EndQuest("Finished quest");
                                        } else {
                                            party.SetPartyState(PARTY_STATE.Working);
                                        }
                                        hasJob = true;
                                        return hasJob;
                                        //hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                                    } else {
                                        LocationGridTile tile = party.targetDestination.GetRandomPassableTile();
                                        if (tile != null) {
                                            hasJob = character.jobComponent.CreatePartyGoToJob(tile, out producedJob);
                                        }
                                    }
                                }
                            } else if (party.partyState == PARTY_STATE.Resting) {
                                log += $"\n-Party is resting";
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
                                    return hasJob;
                                }
                            } else if (party.partyState == PARTY_STATE.Working) {
                                log += $"\n-Party is working";
                                if (!party.targetDestination.IsAtTargetDestination(character)) {
                                    if (party.hasChangedTargetDestination) {
                                        party.SetHasChangedTargetDestination(false);
                                        party.SetPartyState(PARTY_STATE.Moving);
                                        return true;
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

    private bool CampBehaviour(Character character, Party party, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;

        //if (party.campSetter == null) {
        //    party.SetCampSetter(character);
        //} else if (party.foodProducer == null) {
        //    party.SetFoodProducer(character);
        //}

        //if(party.campSetter == character) {
        //    hasJob = CampSetterBehaviour(character, party, out producedJob);
        //    if (hasJob) {
        //        return hasJob;
        //    }
        //}
        //if(party.foodProducer == character) {
        //    hasJob = FoodProducerBehaviour(character, party, out producedJob);
        //    if (hasJob) {
        //        return hasJob;
        //    }
        //}

        //if(!character.needsComponent.isHungry && !character.needsComponent.isStarving 
        //    && !character.needsComponent.isTired && !character.needsComponent.isExhausted
        //    && !character.needsComponent.isBored && !character.needsComponent.isSulking) {

        //} else {
        //    character.needsComponent.CheckExtremeNeedsWhileInActiveParty();
        //}

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
        producedJob = null;
        bool hasJob = false;

        //if (!character.needsComponent.isHungry && !character.needsComponent.isStarving
        //    && !character.needsComponent.isTired && !character.needsComponent.isExhausted
        //    && !character.needsComponent.isBored && !character.needsComponent.isSulking) {

        //} else {
        //    character.needsComponent.CheckExtremeNeedsWhileInActiveParty();
        //}

        hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
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
            if (tile.objHere != null && tile.objHere is Campfire campfire) {
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
            if (tile.objHere != null && tile.objHere is FoodPile foodPile && foodPile.storedResources[RESOURCE.FOOD] >= 12) {
                chosenFoodPile = foodPile;
                break;
            }
        }
        return chosenFoodPile;
    }
}
